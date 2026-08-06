using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Fin;
using Erp.Application.Interfaces.Services.Fin;
using Erp.Domain.Entities.Crm;
using Erp.Domain.Entities.Fin;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Fin;

public sealed class FinArService : IFinArService
{
    private readonly AppDbContext _db;
    private readonly IFinAccountingService _fin;
    private readonly IFinCashService _cash;
    private readonly IFinBankService _bank;
    private readonly IFinVatService _vat;
    private readonly IFinRevenueService _rev;

    public FinArService(
        AppDbContext db, IFinAccountingService fin, IFinCashService cash, IFinBankService bank,
        IFinVatService vat, IFinRevenueService rev)
    {
        _db = db;
        _fin = fin;
        _cash = cash;
        _bank = bank;
        _rev = rev;
        _vat = vat;
    }

    public async Task<IReadOnlyList<FinArInvoiceDto>> ListInvoicesAsync(
        Guid tenantId, Guid? customerId = null, string? status = null, CancellationToken ct = default)
    {
        var q = _db.FinArInvoices.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (customerId is Guid cid) q = q.Where(x => x.CustomerId == cid);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status.Trim());
        var list = await q.OrderByDescending(x => x.InvoiceDate).ThenByDescending(x => x.Code).Take(300).ToListAsync(ct);
        return await MapInvoicesAsync(tenantId, list, ct);
    }

    public async Task<FinArInvoiceDto> UpsertInvoiceAsync(
        Guid tenantId, Guid userId, FinArInvoiceUpsertRequest req, CancellationToken ct = default)
    {
        var customer = await RequireCustomer(tenantId, req.CustomerId, ct);
        if (customer.Status != "Active") throw new AppException("Khách hàng không Active.");
        if (req.SubTotal < 0 || req.TaxAmount < 0) throw new AppException("Số tiền không âm.");
        if (req.DueDate < req.InvoiceDate.Date) throw new AppException("Hạn TT không trước ngày HĐ.");
        if (req.PeriodId is Guid pid)
        {
            var period = await RequirePeriod(tenantId, pid, ct);
            if (period.Status == "Locked") throw new AppException("Kỳ đã khóa sổ.");
        }
        if (req.ArAccountId is Guid arId) _ = await RequireAccount(tenantId, arId, ct);
        if (req.RevenueAccountId is Guid revId) _ = await RequireAccount(tenantId, revId, ct);

        FinArInvoice entity;
        if (req.Id is Guid id)
        {
            entity = await RequireInvoice(tenantId, id, ct);
            if (entity.Status != "Draft") throw new AppException("Chỉ sửa HĐ Draft.");
        }
        else
        {
            entity = new FinArInvoice
            {
                TenantId = tenantId,
                Code = string.IsNullOrWhiteSpace(req.Code)
                    ? await NextCodeAsync("AR", tenantId, ct)
                    : NormCode(req.Code),
                CreatedByUserId = userId,
                CreatedBy = userId
            };
            if (await _db.FinArInvoices.AnyAsync(x => x.TenantId == tenantId && x.Code == entity.Code && !x.IsDeleted, ct))
                throw new AppException("Mã HĐ AR đã tồn tại.");
            _db.FinArInvoices.Add(entity);
        }

        entity.CustomerId = customer.Id;
        entity.CustomerInvoiceNo = NullIfEmpty(req.CustomerInvoiceNo);
        entity.CrmOrderId = req.CrmOrderId;
        entity.InvoiceDate = req.InvoiceDate == default ? DateTimeOffset.UtcNow : req.InvoiceDate;
        entity.DueDate = req.DueDate == default ? entity.InvoiceDate.AddDays(30) : req.DueDate;
        entity.SubTotal = decimal.Round(req.SubTotal, 2);
        entity.TaxAmount = decimal.Round(req.TaxAmount, 2);
        entity.TotalAmount = entity.SubTotal + entity.TaxAmount;
        entity.PeriodId = req.PeriodId;
        entity.ArAccountId = req.ArAccountId;
        entity.RevenueAccountId = req.RevenueAccountId;
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapInvoicesAsync(tenantId, [entity], ct))[0];
    }

    public async Task<FinArInvoiceDto> PostInvoiceAsync(
        Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var inv = await RequireInvoice(tenantId, id, ct);
        if (inv.Status != "Draft") throw new AppException("Chỉ ghi sổ HĐ Draft.");
        if (inv.TotalAmount <= 0) throw new AppException("Tổng HĐ phải > 0.");

        var openBefore = await OpenBalanceAsync(tenantId, inv.CustomerId, ct);
        var projected = openBefore + inv.TotalAmount;
        var limit = await _db.FinArCreditLimits.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.CustomerId == inv.CustomerId
                                      && x.IsActive && !x.IsDeleted, ct);
        inv.CreditLimitWarned = limit is not null && limit.CreditLimit > 0 && projected > limit.CreditLimit;

        if (inv.ArAccountId is Guid arId && inv.RevenueAccountId is Guid revId && inv.PeriodId is Guid periodId)
        {
            var period = await RequirePeriod(tenantId, periodId, ct);
            if (period.Status == "Locked") throw new AppException("Kỳ đã khóa sổ.");
            var customer = await RequireCustomer(tenantId, inv.CustomerId, ct);
            var lines = new List<FinJournalLineUpsertRequest>
            {
                new(null, arId, inv.TotalAmount, 0, customer.Code, null, "Phải thu KH"),
                new(null, revId, 0, inv.TotalAmount, customer.Code, null, "Doanh thu AR"),
            };
            var je = await _fin.CreateAutoJournalStubAsync(tenantId, userId, new FinJournalUpsertRequest(
                null, null, periodId, inv.InvoiceDate, $"AR {inv.Code}: {inv.CustomerInvoiceNo ?? inv.Code}",
                customer.Code, null, "Auto", lines), ct);
            je = await _fin.PostJournalAsync(tenantId, userId, je.Id, ct);
            inv.FinJournalId = je.Id;
            inv.FinJournalCode = je.Code;
        }

        inv.Status = "Open";
        inv.PostedAt = DateTimeOffset.UtcNow;
        inv.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        if (inv.TaxAmount > 0)
        {
            try { await _vat.RegisterFromArAsync(tenantId, userId, inv.Id, null, ct); }
            catch (AppException) { /* chưa cấu hình thuế suất — bỏ qua */ }
        }
        try { await _rev.RecognizeFromArInvoiceAsync(tenantId, userId, inv.Id, null, ct); }
        catch (AppException) { /* bỏ qua nếu chưa sẵn sàng */ }
        return (await MapInvoicesAsync(tenantId, [inv], ct))[0];
    }

    public async Task<FinArInvoiceDto> VoidInvoiceAsync(
        Guid tenantId, Guid userId, Guid id, string? note = null, CancellationToken ct = default)
    {
        var inv = await RequireInvoice(tenantId, id, ct);
        if (inv.Status == "Void") throw new AppException("HĐ đã hủy.");
        if (inv.ReceivedAmount > 0) throw new AppException("HĐ đã có thu tiền — không hủy.");
        if ((inv.Status is "Open" or "Partial") && inv.FinJournalId.HasValue)
            throw new AppException("HĐ đã đẩy BT — đảo BT trước khi hủy (Cap sau).");
        inv.Status = "Void";
        if (!string.IsNullOrWhiteSpace(note)) inv.Note = note.Trim();
        inv.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapInvoicesAsync(tenantId, [inv], ct))[0];
    }

    public async Task<IReadOnlyList<FinArCustomerBalanceDto>> ListCustomerBalancesAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var open = await _db.FinArInvoices.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && (x.Status == "Open" || x.Status == "Partial"))
            .ToListAsync(ct);
        if (open.Count == 0) return Array.Empty<FinArCustomerBalanceDto>();
        var cids = open.Select(x => x.CustomerId).Distinct().ToList();
        var customers = await _db.CrmCustomers.AsNoTracking()
            .Where(x => cids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var limits = await _db.FinArCreditLimits.AsNoTracking()
            .Where(x => x.TenantId == tenantId && cids.Contains(x.CustomerId) && x.IsActive && !x.IsDeleted)
            .ToDictionaryAsync(x => x.CustomerId, ct);
        var today = DateTimeOffset.UtcNow.Date;

        return open.GroupBy(x => x.CustomerId).Select(g =>
        {
            customers.TryGetValue(g.Key, out var c);
            limits.TryGetValue(g.Key, out var lim);
            decimal OpenOf(FinArInvoice i) => Math.Max(0, i.TotalAmount - i.ReceivedAmount);
            var total = g.Sum(OpenOf);
            decimal? pct = lim is { CreditLimit: > 0 } ? Math.Round(total / lim.CreditLimit * 100, 1) : null;
            return new FinArCustomerBalanceDto(
                g.Key, c?.Code ?? "", c?.DisplayName ?? "",
                g.Count(), total,
                g.Where(x => x.DueDate.Date < today).Sum(OpenOf),
                g.Where(x => x.DueDate.Date >= today).Sum(OpenOf),
                lim?.CreditLimit, pct, CreditStatus(total, lim));
        }).OrderBy(x => x.CustomerCode).ToList();
    }

    public async Task<IReadOnlyList<FinArCreditLimitDto>> ListCreditLimitsAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FinArCreditLimits.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.CustomerId).Take(200).ToListAsync(ct);
        return await MapCreditLimitsAsync(tenantId, list, ct);
    }

    public async Task<FinArCreditLimitDto> UpsertCreditLimitAsync(
        Guid tenantId, Guid userId, FinArCreditLimitUpsertRequest req, CancellationToken ct = default)
    {
        _ = await RequireCustomer(tenantId, req.CustomerId, ct);
        if (req.CreditLimit < 0) throw new AppException("Hạn mức ≥ 0.");
        var warn = req.WarningPercent ?? 80;
        if (warn is < 1 or > 100) throw new AppException("WarningPercent 1–100.");

        FinArCreditLimit entity;
        if (req.Id is Guid id)
        {
            entity = await _db.FinArCreditLimits.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy hạn mức.");
        }
        else
        {
            var existing = await _db.FinArCreditLimits.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.CustomerId == req.CustomerId && !x.IsDeleted, ct);
            if (existing is not null) entity = existing;
            else
            {
                entity = new FinArCreditLimit { TenantId = tenantId, CustomerId = req.CustomerId, CreatedBy = userId };
                _db.FinArCreditLimits.Add(entity);
            }
        }

        entity.CustomerId = req.CustomerId;
        entity.CreditLimit = decimal.Round(req.CreditLimit, 2);
        entity.WarningPercent = warn;
        entity.IsActive = req.IsActive ?? true;
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapCreditLimitsAsync(tenantId, [entity], ct))[0];
    }

    public async Task<IReadOnlyList<FinArCreditLimitDto>> ListCreditAlertsAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var all = await ListCreditLimitsAsync(tenantId, ct);
        return all.Where(x => x.IsActive && x.CreditStatus is "Warning" or "Exceeded").ToList();
    }

    public async Task<IReadOnlyList<FinArReceiptDto>> ListReceiptsAsync(
        Guid tenantId, Guid? customerId = null, CancellationToken ct = default)
    {
        var q = _db.FinArReceipts.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (customerId is Guid cid) q = q.Where(x => x.CustomerId == cid);
        var list = await q.OrderByDescending(x => x.ReceiptDate).ThenByDescending(x => x.Code).Take(200).ToListAsync(ct);
        return await MapReceiptsAsync(tenantId, list, ct);
    }

    public async Task<FinArReceiptDto> UpsertReceiptAsync(
        Guid tenantId, Guid userId, FinArReceiptUpsertRequest req, CancellationToken ct = default)
    {
        _ = await RequireCustomer(tenantId, req.CustomerId, ct);
        var method = (req.PayMethod ?? "Bank").Trim();
        if (method is not ("Cash" or "Bank")) throw new AppException("PayMethod: Cash | Bank.");
        if (method == "Cash" && req.CashFundId is null) throw new AppException("Chọn quỹ.");
        if (method == "Bank" && req.BankAccountId is null) throw new AppException("Chọn TKNH.");
        if (req.Allocations is null || req.Allocations.Count == 0) throw new AppException("Cần phân bổ HĐ.");
        var allocs = req.Allocations.Where(x => x.Amount > 0).ToList();
        if (allocs.Count == 0) throw new AppException("Số tiền phân bổ > 0.");
        await ValidateAllocations(tenantId, req.CustomerId, allocs, ct);

        FinArReceipt entity;
        if (req.Id is Guid id)
        {
            entity = await RequireReceipt(tenantId, id, ct);
            if (entity.Status != "Draft") throw new AppException("Chỉ sửa phiếu thu Draft.");
            var old = await _db.FinArReceiptAllocations
                .Where(x => x.TenantId == tenantId && x.ReceiptId == entity.Id && !x.IsDeleted).ToListAsync(ct);
            foreach (var o in old) { o.IsDeleted = true; o.UpdatedBy = userId; }
        }
        else
        {
            entity = new FinArReceipt
            {
                TenantId = tenantId,
                Code = string.IsNullOrWhiteSpace(req.Code)
                    ? await NextCodeAsync("PTAR", tenantId, ct)
                    : NormCode(req.Code),
                CreatedByUserId = userId,
                CreatedBy = userId
            };
            if (await _db.FinArReceipts.AnyAsync(x => x.TenantId == tenantId && x.Code == entity.Code && !x.IsDeleted, ct))
                throw new AppException("Mã phiếu thu đã tồn tại.");
            _db.FinArReceipts.Add(entity);
        }

        entity.CustomerId = req.CustomerId;
        entity.ReceiptDate = req.ReceiptDate == default ? DateTimeOffset.UtcNow : req.ReceiptDate;
        entity.PayMethod = method;
        entity.CashFundId = method == "Cash" ? req.CashFundId : null;
        entity.BankAccountId = method == "Bank" ? req.BankAccountId : null;
        entity.PeriodId = req.PeriodId;
        entity.Amount = decimal.Round(allocs.Sum(x => x.Amount), 2);
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        foreach (var a in allocs)
        {
            _db.FinArReceiptAllocations.Add(new FinArReceiptAllocation
            {
                TenantId = tenantId,
                ReceiptId = entity.Id,
                ArInvoiceId = a.ArInvoiceId,
                Amount = decimal.Round(a.Amount, 2),
                CreatedBy = userId
            });
        }
        await _db.SaveChangesAsync(ct);
        return (await MapReceiptsAsync(tenantId, [entity], ct))[0];
    }

    public async Task<FinArReceiptDto> PostReceiptAsync(
        Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var receipt = await RequireReceipt(tenantId, id, ct);
        if (receipt.Status != "Draft") throw new AppException("Chỉ ghi sổ phiếu thu Draft.");
        var allocs = await _db.FinArReceiptAllocations
            .Where(x => x.TenantId == tenantId && x.ReceiptId == receipt.Id && !x.IsDeleted).ToListAsync(ct);
        if (allocs.Count == 0) throw new AppException("Phiếu thu chưa phân bổ.");
        await ValidateAllocations(tenantId, receipt.CustomerId,
            allocs.Select(x => new FinArReceiptAllocationInput(x.ArInvoiceId, x.Amount)).ToList(), ct);

        var customer = await RequireCustomer(tenantId, receipt.CustomerId, ct);

        if (receipt.PayMethod == "Cash")
        {
            var cv = await _cash.UpsertVoucherAsync(tenantId, userId, new FinCashVoucherUpsertRequest(
                null, null, receipt.CashFundId!.Value, "Receipt", receipt.ReceiptDate, receipt.Amount,
                $"Thu AR {receipt.Code} · {customer.DisplayName}", customer.Code, null, receipt.PeriodId,
                $"AR {receipt.Code}"), ct);
            cv = await _cash.PostVoucherAsync(tenantId, userId, cv.Id, ct);
            receipt.CashVoucherId = cv.Id;
        }
        else
        {
            var bv = await _bank.UpsertVoucherAsync(tenantId, userId, new FinBankVoucherUpsertRequest(
                null, null, receipt.BankAccountId!.Value, "Credit", receipt.ReceiptDate, receipt.Amount,
                $"Thu AR {receipt.Code} · {customer.DisplayName}", receipt.Code, customer.Code, null,
                receipt.PeriodId, $"AR {receipt.Code}"), ct);
            bv = await _bank.PostVoucherAsync(tenantId, userId, bv.Id, ct);
            receipt.BankVoucherId = bv.Id;
        }

        foreach (var a in allocs)
        {
            var inv = await RequireInvoice(tenantId, a.ArInvoiceId, ct);
            inv.ReceivedAmount = decimal.Round(inv.ReceivedAmount + a.Amount, 2);
            inv.Status = inv.ReceivedAmount + 0.01m >= inv.TotalAmount ? "Paid"
                : inv.ReceivedAmount > 0 ? "Partial" : inv.Status;
            inv.UpdatedBy = userId;
        }

        receipt.Status = "Posted";
        receipt.PostedAt = DateTimeOffset.UtcNow;
        receipt.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapReceiptsAsync(tenantId, [receipt], ct))[0];
    }

    public async Task<FinArAgingDto> GetAgingAsync(
        Guid tenantId, DateTimeOffset? asOf = null, CancellationToken ct = default)
    {
        var asOfDt = (asOf ?? DateTimeOffset.UtcNow).Date;
        var open = await _db.FinArInvoices.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && (x.Status == "Open" || x.Status == "Partial"))
            .ToListAsync(ct);
        var cids = open.Select(x => x.CustomerId).Distinct().ToList();
        var customers = cids.Count == 0 ? new Dictionary<Guid, CrmCustomer>()
            : await _db.CrmCustomers.AsNoTracking().Where(x => cids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);

        static string BucketOf(int days) => days switch
        {
            <= 0 => "Current",
            <= 30 => "1-30",
            <= 60 => "31-60",
            <= 90 => "61-90",
            _ => ">90"
        };

        var rows = new List<FinArAgingRowDto>();
        foreach (var g in open.GroupBy(x => x.CustomerId))
        {
            customers.TryGetValue(g.Key, out var c);
            decimal cur = 0, d1 = 0, d2 = 0, d3 = 0, d4 = 0;
            foreach (var inv in g)
            {
                var openAmt = Math.Max(0, inv.TotalAmount - inv.ReceivedAmount);
                var days = (asOfDt - inv.DueDate.Date).Days;
                switch (BucketOf(days))
                {
                    case "Current": cur += openAmt; break;
                    case "1-30": d1 += openAmt; break;
                    case "31-60": d2 += openAmt; break;
                    case "61-90": d3 += openAmt; break;
                    default: d4 += openAmt; break;
                }
            }
            rows.Add(new FinArAgingRowDto(
                g.Key, c?.Code ?? "", c?.DisplayName ?? "", cur, d1, d2, d3, d4, cur + d1 + d2 + d3 + d4));
        }

        var buckets = new[]
        {
            new FinArAgingBucketDto("Current", rows.Sum(x => x.Current), open.Count(x => (asOfDt - x.DueDate.Date).Days <= 0)),
            new FinArAgingBucketDto("1-30", rows.Sum(x => x.D1To30), open.Count(x => { var d = (asOfDt - x.DueDate.Date).Days; return d is > 0 and <= 30; })),
            new FinArAgingBucketDto("31-60", rows.Sum(x => x.D31To60), open.Count(x => { var d = (asOfDt - x.DueDate.Date).Days; return d is > 30 and <= 60; })),
            new FinArAgingBucketDto("61-90", rows.Sum(x => x.D61To90), open.Count(x => { var d = (asOfDt - x.DueDate.Date).Days; return d is > 60 and <= 90; })),
            new FinArAgingBucketDto(">90", rows.Sum(x => x.Over90), open.Count(x => (asOfDt - x.DueDate.Date).Days > 90)),
        };

        return new FinArAgingDto(asOfDt, buckets, rows.OrderByDescending(x => x.Total).ToList());
    }

    private async Task ValidateAllocations(
        Guid tenantId, Guid customerId, IReadOnlyList<FinArReceiptAllocationInput> allocs, CancellationToken ct)
    {
        var ids = allocs.Select(x => x.ArInvoiceId).Distinct().ToList();
        var invoices = await _db.FinArInvoices
            .Where(x => x.TenantId == tenantId && ids.Contains(x.Id) && !x.IsDeleted).ToListAsync(ct);
        if (invoices.Count != ids.Count) throw new AppException("Có HĐ AR không hợp lệ.");
        foreach (var inv in invoices)
        {
            if (inv.CustomerId != customerId) throw new AppException($"HĐ {inv.Code} khác khách.");
            if (inv.Status is not ("Open" or "Partial")) throw new AppException($"HĐ {inv.Code} không Open/Partial.");
            var amt = allocs.Where(x => x.ArInvoiceId == inv.Id).Sum(x => x.Amount);
            var open = inv.TotalAmount - inv.ReceivedAmount;
            if (amt > open + 0.01m) throw new AppException($"HĐ {inv.Code} vượt số còn lại ({open:N0}).");
        }
    }

    private async Task<decimal> OpenBalanceAsync(Guid tenantId, Guid customerId, CancellationToken ct)
    {
        var open = await _db.FinArInvoices.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.CustomerId == customerId && !x.IsDeleted
                        && (x.Status == "Open" || x.Status == "Partial"))
            .Select(x => new { x.TotalAmount, x.ReceivedAmount }).ToListAsync(ct);
        return open.Sum(x => Math.Max(0, x.TotalAmount - x.ReceivedAmount));
    }

    private static string CreditStatus(decimal open, FinArCreditLimit? lim)
    {
        if (lim is null || lim.CreditLimit <= 0) return "None";
        var pct = open / lim.CreditLimit * 100;
        if (open > lim.CreditLimit) return "Exceeded";
        if (pct >= lim.WarningPercent) return "Warning";
        return "Ok";
    }

    private async Task<IReadOnlyList<FinArInvoiceDto>> MapInvoicesAsync(
        Guid tenantId, List<FinArInvoice> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<FinArInvoiceDto>();
        var cids = list.Select(x => x.CustomerId).Distinct().ToList();
        var pids = list.Where(x => x.PeriodId.HasValue).Select(x => x.PeriodId!.Value).Distinct().ToList();
        var customers = await _db.CrmCustomers.AsNoTracking().Where(x => cids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var periods = pids.Count == 0 ? new Dictionary<Guid, FinPeriod>()
            : await _db.FinPeriods.AsNoTracking().Where(x => pids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        return list.Select(i =>
        {
            customers.TryGetValue(i.CustomerId, out var c);
            FinPeriod? pe = null;
            if (i.PeriodId is Guid pid) periods.TryGetValue(pid, out pe);
            var open = Math.Max(0, i.TotalAmount - i.ReceivedAmount);
            return new FinArInvoiceDto(
                i.Id, i.Code, i.CustomerId, c?.Code, c?.DisplayName, i.CustomerInvoiceNo, i.CrmOrderId,
                i.InvoiceDate, i.DueDate, i.SubTotal, i.TaxAmount, i.TotalAmount, i.ReceivedAmount, open,
                i.Status, i.CreditLimitWarned, i.PeriodId, pe?.Code, i.FinJournalId, i.FinJournalCode, i.PostedAt, i.Note);
        }).ToList();
    }

    private async Task<IReadOnlyList<FinArCreditLimitDto>> MapCreditLimitsAsync(
        Guid tenantId, List<FinArCreditLimit> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<FinArCreditLimitDto>();
        var cids = list.Select(x => x.CustomerId).Distinct().ToList();
        var customers = await _db.CrmCustomers.AsNoTracking().Where(x => cids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var opens = await _db.FinArInvoices.AsNoTracking()
            .Where(x => x.TenantId == tenantId && cids.Contains(x.CustomerId) && !x.IsDeleted
                        && (x.Status == "Open" || x.Status == "Partial"))
            .GroupBy(x => x.CustomerId)
            .Select(g => new { g.Key, Open = g.Sum(x => x.TotalAmount - x.ReceivedAmount) })
            .ToDictionaryAsync(x => x.Key, x => x.Open, ct);

        return list.Select(l =>
        {
            customers.TryGetValue(l.CustomerId, out var c);
            opens.TryGetValue(l.CustomerId, out var open);
            open = Math.Max(0, open);
            return new FinArCreditLimitDto(
                l.Id, l.CustomerId, c?.Code, c?.DisplayName, l.CreditLimit, l.WarningPercent, l.IsActive, l.Note,
                open, CreditStatus(open, l));
        }).ToList();
    }

    private async Task<IReadOnlyList<FinArReceiptDto>> MapReceiptsAsync(
        Guid tenantId, List<FinArReceipt> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<FinArReceiptDto>();
        var ids = list.Select(x => x.Id).ToList();
        var cids = list.Select(x => x.CustomerId).Distinct().ToList();
        var customers = await _db.CrmCustomers.AsNoTracking().Where(x => cids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var allocs = await _db.FinArReceiptAllocations.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.ReceiptId) && !x.IsDeleted).ToListAsync(ct);
        var invIds = allocs.Select(x => x.ArInvoiceId).Distinct().ToList();
        var invoices = invIds.Count == 0 ? new Dictionary<Guid, FinArInvoice>()
            : await _db.FinArInvoices.AsNoTracking().Where(x => invIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);

        return list.Select(r =>
        {
            customers.TryGetValue(r.CustomerId, out var c);
            var aLines = allocs.Where(x => x.ReceiptId == r.Id).Select(a =>
            {
                invoices.TryGetValue(a.ArInvoiceId, out var inv);
                return new FinArReceiptAllocationDto(a.ArInvoiceId, inv?.Code, a.Amount);
            }).ToList();
            return new FinArReceiptDto(
                r.Id, r.Code, r.CustomerId, c?.Code, c?.DisplayName, r.ReceiptDate, r.Amount, r.PayMethod,
                r.CashFundId, r.BankAccountId, r.CashVoucherId, r.BankVoucherId,
                r.Status, r.FinJournalId, r.FinJournalCode, r.PostedAt, r.Note, aLines);
        }).ToList();
    }

    private async Task<FinArInvoice> RequireInvoice(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FinArInvoices.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy HĐ AR.");

    private async Task<FinArReceipt> RequireReceipt(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FinArReceipts.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy phiếu thu AR.");

    private async Task<CrmCustomer> RequireCustomer(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.CrmCustomers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy khách hàng.");

    private async Task<FinAccount> RequireAccount(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FinAccounts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy tài khoản.");

    private async Task<FinPeriod> RequirePeriod(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FinPeriods.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy kỳ KT.");

    private async Task<string> NextCodeAsync(string prefix, Guid tenantId, CancellationToken ct)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var stem = $"{prefix}-{today}-";
        string? last = prefix == "AR"
            ? await _db.FinArInvoices.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Code.StartsWith(stem))
                .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct)
            : await _db.FinArReceipts.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Code.StartsWith(stem))
                .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct);
        var seq = 1;
        if (last is not null && int.TryParse(last[stem.Length..], out var n)) seq = n + 1;
        return $"{stem}{seq:D4}";
    }

    private static string NormCode(string? code)
    {
        var c = (code ?? "").Trim().ToUpperInvariant();
        if (c.Length is < 1 or > 40) throw new AppException("Mã 1–40 ký tự.");
        return c;
    }

    private static string? NullIfEmpty(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
