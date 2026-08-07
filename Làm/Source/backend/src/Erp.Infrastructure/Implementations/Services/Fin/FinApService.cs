using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Fin;
using Erp.Application.Interfaces.Services.Fin;
using Erp.Domain.Entities.Fin;
using Erp.Domain.Entities.Pur;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Fin;

public sealed class FinApService : IFinApService
{
    private readonly AppDbContext _db;
    private readonly IFinAccountingService _fin;
    private readonly IFinCashService _cash;
    private readonly IFinBankService _bank;
    private readonly IFinVatService _vat;

    public FinApService(
        AppDbContext db, IFinAccountingService fin, IFinCashService cash, IFinBankService bank, IFinVatService vat)
    {
        _db = db;
        _fin = fin;
        _cash = cash;
        _bank = bank;
        _vat = vat;
    }

    public async Task<IReadOnlyList<FinApInvoiceDto>> ListInvoicesAsync(
        Guid tenantId, Guid? vendorId = null, string? status = null, CancellationToken ct = default)
    {
        var q = _db.FinApInvoices.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (vendorId is Guid vid) q = q.Where(x => x.VendorId == vid);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status.Trim());
        var list = await q.OrderByDescending(x => x.InvoiceDate).ThenByDescending(x => x.Code).Take(300).ToListAsync(ct);
        return await MapInvoicesAsync(tenantId, list, ct);
    }

    public async Task<FinApInvoiceDto> UpsertInvoiceAsync(
        Guid tenantId, Guid userId, FinApInvoiceUpsertRequest req, CancellationToken ct = default)
    {
        var vendor = await RequireVendor(tenantId, req.VendorId, ct);
        if (vendor.Status != "Active") throw new AppException("NCC không Active.");
        if (req.SubTotal < 0 || req.TaxAmount < 0) throw new AppException("Số tiền không âm.");
        if (req.DueDate < req.InvoiceDate.Date) throw new AppException("Hạn TT không trước ngày HĐ.");
        if (req.PeriodId is Guid pid)
        {
            var period = await RequirePeriod(tenantId, pid, ct);
            if (period.Status == "Locked") throw new AppException("Kỳ đã khóa sổ.");
        }
        if (req.ApAccountId is Guid apId) _ = await RequireAccount(tenantId, apId, ct);
        if (req.ExpenseAccountId is Guid exId) _ = await RequireAccount(tenantId, exId, ct);
        if (req.PurVendorInvoiceId is Guid purId)
        {
            var pur = await _db.PurVendorInvoices.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == purId && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy HĐ PUR.");
            if (pur.VendorId != req.VendorId) throw new AppException("HĐ PUR khác NCC.");
        }

        FinApInvoice entity;
        if (req.Id is Guid id)
        {
            entity = await RequireInvoice(tenantId, id, ct);
            if (entity.Status != "Draft") throw new AppException("Chỉ sửa HĐ Draft.");
        }
        else
        {
            entity = new FinApInvoice
            {
                TenantId = tenantId,
                Code = string.IsNullOrWhiteSpace(req.Code)
                    ? await NextCodeAsync("AP", ct, tenantId)
                    : NormCode(req.Code),
                CreatedByUserId = userId,
                CreatedBy = userId
            };
            if (await _db.FinApInvoices.AnyAsync(x => x.TenantId == tenantId && x.Code == entity.Code && !x.IsDeleted, ct))
                throw new AppException("Mã HĐ AP đã tồn tại.");
            _db.FinApInvoices.Add(entity);
        }

        entity.VendorId = vendor.Id;
        entity.VendorInvoiceNo = NullIfEmpty(req.VendorInvoiceNo);
        entity.PurVendorInvoiceId = req.PurVendorInvoiceId;
        entity.InvoiceDate = req.InvoiceDate == default ? DateTimeOffset.UtcNow : req.InvoiceDate;
        entity.DueDate = req.DueDate == default ? entity.InvoiceDate.AddDays(30) : req.DueDate;
        entity.SubTotal = decimal.Round(req.SubTotal, 2);
        entity.TaxAmount = decimal.Round(req.TaxAmount, 2);
        entity.TotalAmount = entity.SubTotal + entity.TaxAmount;
        entity.PeriodId = req.PeriodId;
        entity.ApAccountId = req.ApAccountId;
        entity.ExpenseAccountId = req.ExpenseAccountId;
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapInvoicesAsync(tenantId, [entity], ct))[0];
    }

    public async Task<FinApInvoiceDto> PostInvoiceAsync(
        Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var inv = await RequireInvoice(tenantId, id, ct);
        if (inv.Status != "Draft") throw new AppException("Chỉ ghi sổ HĐ Draft.");
        if (inv.TotalAmount <= 0) throw new AppException("Tổng HĐ phải > 0.");

        // UC_FIN_039: luôn tạo JE Nợ chi phí / Có 331 (auto-resolve nếu thiếu TK/kỳ).
        var periodId = await _fin.ResolveOpenPeriodIdAsync(tenantId, inv.PeriodId, inv.InvoiceDate, ct);
        inv.PeriodId = periodId;
        var apId = inv.ApAccountId
            ?? await _fin.ResolvePostableAccountIdAsync(tenantId, ["331"], "TK phải trả (331*)", ct);
        var exId = inv.ExpenseAccountId
            ?? await _fin.ResolvePostableAccountIdAsync(tenantId, ["156", "152", "642", "627"], "TK chi phí/hàng mua AP", ct);
        inv.ApAccountId = apId;
        inv.ExpenseAccountId = exId;

        var vendor = await RequireVendor(tenantId, inv.VendorId, ct);
        var lines = new List<FinJournalLineUpsertRequest>
        {
            new(null, exId, inv.TotalAmount, 0, vendor.Code, null, "Chi phí/Hàng mua AP"),
            new(null, apId, 0, inv.TotalAmount, vendor.Code, null, "Phải trả NCC"),
        };
        var je = await _fin.CreateAutoJournalAsync(tenantId, userId, new FinJournalUpsertRequest(
            null, null, periodId, inv.InvoiceDate, $"AP {inv.Code}: {inv.VendorInvoiceNo ?? inv.Code}",
            vendor.Code, null, "Auto", lines), ct);
        je = await _fin.PostJournalAsync(tenantId, userId, je.Id, ct);
        inv.FinJournalId = je.Id;
        inv.FinJournalCode = je.Code;

        inv.Status = "Open";
        inv.PostedAt = DateTimeOffset.UtcNow;
        inv.UpdatedBy = userId;

        if (inv.PurVendorInvoiceId is Guid purId)
        {
            var pur = await _db.PurVendorInvoices.FirstOrDefaultAsync(
                x => x.Id == purId && x.TenantId == tenantId && !x.IsDeleted, ct);
            if (pur is not null)
            {
                pur.ApPushStatus = "Pushed";
                pur.UpdatedBy = userId;
            }
        }

        await _db.SaveChangesAsync(ct);
        if (inv.TaxAmount > 0)
        {
            try { await _vat.RegisterFromApAsync(tenantId, userId, inv.Id, null, ct); }
            catch (AppException) { /* chưa cấu hình thuế suất — bỏ qua */ }
        }
        return (await MapInvoicesAsync(tenantId, [inv], ct))[0];
    }

    public async Task<FinApInvoiceDto> VoidInvoiceAsync(
        Guid tenantId, Guid userId, Guid id, string? note = null, CancellationToken ct = default)
    {
        var inv = await RequireInvoice(tenantId, id, ct);
        if (inv.Status == "Void") throw new AppException("HĐ đã hủy.");
        if (inv.PaidAmount > 0) throw new AppException("HĐ đã có thanh toán — không hủy.");
        if ((inv.Status is "Open" or "Partial") && inv.FinJournalId.HasValue)
            throw new AppException("HĐ đã đẩy BT — đảo BT trước khi hủy (Cap sau).");
        inv.Status = "Void";
        if (!string.IsNullOrWhiteSpace(note)) inv.Note = note.Trim();
        inv.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapInvoicesAsync(tenantId, [inv], ct))[0];
    }

    public async Task<IReadOnlyList<FinApVendorBalanceDto>> ListVendorBalancesAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var open = await _db.FinApInvoices.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && (x.Status == "Open" || x.Status == "Partial"))
            .ToListAsync(ct);
        if (open.Count == 0) return Array.Empty<FinApVendorBalanceDto>();
        var vids = open.Select(x => x.VendorId).Distinct().ToList();
        var vendors = await _db.PurVendors.AsNoTracking()
            .Where(x => vids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var today = DateTimeOffset.UtcNow.Date;

        return open.GroupBy(x => x.VendorId).Select(g =>
        {
            vendors.TryGetValue(g.Key, out var v);
            var rows = g.ToList();
            decimal OpenOf(FinApInvoice i) => Math.Max(0, i.TotalAmount - i.PaidAmount);
            return new FinApVendorBalanceDto(
                g.Key, v?.Code ?? "", v?.Name ?? "",
                rows.Count,
                rows.Sum(OpenOf),
                rows.Where(x => x.DueDate.Date < today).Sum(OpenOf),
                rows.Where(x => x.DueDate.Date >= today).Sum(OpenOf));
        }).OrderBy(x => x.VendorCode).ToList();
    }

    public async Task<IReadOnlyList<FinApPaymentRequestDto>> ListPaymentRequestsAsync(
        Guid tenantId, Guid? vendorId = null, CancellationToken ct = default)
    {
        var q = _db.FinApPaymentRequests.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (vendorId is Guid vid) q = q.Where(x => x.VendorId == vid);
        var list = await q.OrderByDescending(x => x.RequestDate).ThenByDescending(x => x.Code).Take(200).ToListAsync(ct);
        return await MapRequestsAsync(tenantId, list, ct);
    }

    public async Task<FinApPaymentRequestDto> UpsertPaymentRequestAsync(
        Guid tenantId, Guid userId, FinApPaymentRequestUpsertRequest req, CancellationToken ct = default)
    {
        _ = await RequireVendor(tenantId, req.VendorId, ct);
        var method = (req.PayMethod ?? "Bank").Trim();
        if (method is not ("Cash" or "Bank")) throw new AppException("PayMethod: Cash | Bank.");
        if (method == "Cash" && req.CashFundId is null) throw new AppException("Chọn quỹ tiền mặt.");
        if (method == "Bank" && req.BankAccountId is null) throw new AppException("Chọn TKNH.");
        if (req.Lines is null || req.Lines.Count == 0) throw new AppException("Cần ít nhất 1 dòng HĐ.");

        var lineInputs = req.Lines.Where(x => x.Amount > 0).ToList();
        if (lineInputs.Count == 0) throw new AppException("Số tiền dòng > 0.");
        var invoiceIds = lineInputs.Select(x => x.ApInvoiceId).Distinct().ToList();
        var invoices = await _db.FinApInvoices
            .Where(x => x.TenantId == tenantId && invoiceIds.Contains(x.Id) && !x.IsDeleted).ToListAsync(ct);
        if (invoices.Count != invoiceIds.Count) throw new AppException("Có HĐ AP không hợp lệ.");
        foreach (var inv in invoices)
        {
            if (inv.VendorId != req.VendorId) throw new AppException($"HĐ {inv.Code} khác NCC.");
            if (inv.Status is not ("Open" or "Partial")) throw new AppException($"HĐ {inv.Code} không Open/Partial.");
            var amt = lineInputs.First(x => x.ApInvoiceId == inv.Id).Amount;
            var open = inv.TotalAmount - inv.PaidAmount;
            if (amt > open + 0.01m) throw new AppException($"HĐ {inv.Code} vượt số còn lại ({open:N0}).");
        }

        FinApPaymentRequest entity;
        if (req.Id is Guid id)
        {
            entity = await RequireRequest(tenantId, id, ct);
            if (entity.Status != "Draft") throw new AppException("Chỉ sửa đề nghị Draft.");
            var oldLines = await _db.FinApPaymentRequestLines
                .Where(x => x.TenantId == tenantId && x.PaymentRequestId == entity.Id && !x.IsDeleted).ToListAsync(ct);
            foreach (var ol in oldLines) { ol.IsDeleted = true; ol.UpdatedBy = userId; }
        }
        else
        {
            entity = new FinApPaymentRequest
            {
                TenantId = tenantId,
                Code = string.IsNullOrWhiteSpace(req.Code)
                    ? await NextCodeAsync("DNTT", ct, tenantId)
                    : NormCode(req.Code),
                RequestedByUserId = userId,
                CreatedBy = userId
            };
            if (await _db.FinApPaymentRequests.AnyAsync(x => x.TenantId == tenantId && x.Code == entity.Code && !x.IsDeleted, ct))
                throw new AppException("Mã đề nghị đã tồn tại.");
            _db.FinApPaymentRequests.Add(entity);
        }

        entity.VendorId = req.VendorId;
        entity.RequestDate = req.RequestDate ?? DateTimeOffset.UtcNow;
        entity.PayMethod = method;
        entity.CashFundId = method == "Cash" ? req.CashFundId : null;
        entity.BankAccountId = method == "Bank" ? req.BankAccountId : null;
        entity.RequestAmount = decimal.Round(lineInputs.Sum(x => x.Amount), 2);
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        foreach (var li in lineInputs)
        {
            _db.FinApPaymentRequestLines.Add(new FinApPaymentRequestLine
            {
                TenantId = tenantId,
                PaymentRequestId = entity.Id,
                ApInvoiceId = li.ApInvoiceId,
                Amount = decimal.Round(li.Amount, 2),
                CreatedBy = userId
            });
        }
        await _db.SaveChangesAsync(ct);
        return (await MapRequestsAsync(tenantId, [entity], ct))[0];
    }

    public async Task<FinApPaymentRequestDto> SubmitPaymentRequestAsync(
        Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var r = await RequireRequest(tenantId, id, ct);
        if (r.Status != "Draft") throw new AppException("Chỉ gửi đề nghị Draft.");
        r.Status = "Submitted";
        r.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapRequestsAsync(tenantId, [r], ct))[0];
    }

    public async Task<FinApPaymentRequestDto> ApprovePaymentRequestAsync(
        Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var r = await RequireRequest(tenantId, id, ct);
        if (r.Status != "Submitted") throw new AppException("Chỉ duyệt đề nghị Submitted.");
        r.Status = "Approved";
        r.ApprovedByUserId = userId;
        r.ApprovedAt = DateTimeOffset.UtcNow;
        r.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapRequestsAsync(tenantId, [r], ct))[0];
    }

    public async Task<FinApPaymentRequestDto> RejectPaymentRequestAsync(
        Guid tenantId, Guid userId, Guid id, string? note = null, CancellationToken ct = default)
    {
        var r = await RequireRequest(tenantId, id, ct);
        if (r.Status is not ("Submitted" or "Approved")) throw new AppException("Chỉ từ chối Submitted/Approved.");
        r.Status = "Rejected";
        if (!string.IsNullOrWhiteSpace(note)) r.Note = note.Trim();
        r.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapRequestsAsync(tenantId, [r], ct))[0];
    }

    public async Task<FinApPaymentRequestDto> VoidPaymentRequestAsync(
        Guid tenantId, Guid userId, Guid id, string? note = null, CancellationToken ct = default)
    {
        var r = await RequireRequest(tenantId, id, ct);
        if (r.Status is "Paid") throw new AppException("Đã thanh toán — không hủy.");
        if (r.Status is "Void") throw new AppException("Đề nghị đã hủy.");
        r.Status = "Void";
        if (!string.IsNullOrWhiteSpace(note)) r.Note = note.Trim();
        r.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapRequestsAsync(tenantId, [r], ct))[0];
    }

    public async Task<IReadOnlyList<FinApPaymentDto>> ListPaymentsAsync(
        Guid tenantId, Guid? vendorId = null, CancellationToken ct = default)
    {
        var q = _db.FinApPayments.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (vendorId is Guid vid) q = q.Where(x => x.VendorId == vid);
        var list = await q.OrderByDescending(x => x.PayDate).ThenByDescending(x => x.Code).Take(200).ToListAsync(ct);
        return await MapPaymentsAsync(tenantId, list, ct);
    }

    public async Task<FinApPaymentDto> UpsertPaymentAsync(
        Guid tenantId, Guid userId, FinApPaymentUpsertRequest req, CancellationToken ct = default)
    {
        _ = await RequireVendor(tenantId, req.VendorId, ct);
        var method = (req.PayMethod ?? "Bank").Trim();
        if (method is not ("Cash" or "Bank")) throw new AppException("PayMethod: Cash | Bank.");
        if (method == "Cash" && req.CashFundId is null) throw new AppException("Chọn quỹ.");
        if (method == "Bank" && req.BankAccountId is null) throw new AppException("Chọn TKNH.");
        if (req.Allocations is null || req.Allocations.Count == 0) throw new AppException("Cần phân bổ HĐ.");
        var allocs = req.Allocations.Where(x => x.Amount > 0).ToList();
        if (allocs.Count == 0) throw new AppException("Số tiền phân bổ > 0.");
        await ValidateAllocations(tenantId, req.VendorId, allocs, ct);

        FinApPayment entity;
        if (req.Id is Guid id)
        {
            entity = await RequirePayment(tenantId, id, ct);
            if (entity.Status != "Draft") throw new AppException("Chỉ sửa phiếu TT Draft.");
            var old = await _db.FinApPaymentAllocations
                .Where(x => x.TenantId == tenantId && x.PaymentId == entity.Id && !x.IsDeleted).ToListAsync(ct);
            foreach (var o in old) { o.IsDeleted = true; o.UpdatedBy = userId; }
        }
        else
        {
            entity = new FinApPayment
            {
                TenantId = tenantId,
                Code = string.IsNullOrWhiteSpace(req.Code)
                    ? await NextCodeAsync("TTAP", ct, tenantId)
                    : NormCode(req.Code),
                CreatedByUserId = userId,
                CreatedBy = userId
            };
            if (await _db.FinApPayments.AnyAsync(x => x.TenantId == tenantId && x.Code == entity.Code && !x.IsDeleted, ct))
                throw new AppException("Mã phiếu TT đã tồn tại.");
            _db.FinApPayments.Add(entity);
        }

        entity.VendorId = req.VendorId;
        entity.PayDate = req.PayDate == default ? DateTimeOffset.UtcNow : req.PayDate;
        entity.PayMethod = method;
        entity.CashFundId = method == "Cash" ? req.CashFundId : null;
        entity.BankAccountId = method == "Bank" ? req.BankAccountId : null;
        entity.PaymentRequestId = req.PaymentRequestId;
        entity.PeriodId = req.PeriodId;
        entity.Amount = decimal.Round(allocs.Sum(x => x.Amount), 2);
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        foreach (var a in allocs)
        {
            _db.FinApPaymentAllocations.Add(new FinApPaymentAllocation
            {
                TenantId = tenantId,
                PaymentId = entity.Id,
                ApInvoiceId = a.ApInvoiceId,
                Amount = decimal.Round(a.Amount, 2),
                CreatedBy = userId
            });
        }
        await _db.SaveChangesAsync(ct);
        return (await MapPaymentsAsync(tenantId, [entity], ct))[0];
    }

    public async Task<FinApPaymentDto> PostPaymentAsync(
        Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var pay = await RequirePayment(tenantId, id, ct);
        if (pay.Status != "Draft") throw new AppException("Chỉ ghi sổ phiếu TT Draft.");
        var allocs = await _db.FinApPaymentAllocations
            .Where(x => x.TenantId == tenantId && x.PaymentId == pay.Id && !x.IsDeleted).ToListAsync(ct);
        if (allocs.Count == 0) throw new AppException("Phiếu TT chưa phân bổ.");
        await ValidateAllocations(tenantId, pay.VendorId,
            allocs.Select(x => new FinApPaymentAllocationInput(x.ApInvoiceId, x.Amount)).ToList(), ct);

        var vendor = await RequireVendor(tenantId, pay.VendorId, ct);

        if (pay.PayMethod == "Cash")
        {
            var cv = await _cash.UpsertVoucherAsync(tenantId, userId, new FinCashVoucherUpsertRequest(
                null, null, pay.CashFundId!.Value, "Payment", pay.PayDate, pay.Amount,
                $"Thanh toán AP {pay.Code} · {vendor.Name}", vendor.Code, null, pay.PeriodId, $"AP {pay.Code}"), ct);
            cv = await _cash.PostVoucherAsync(tenantId, userId, cv.Id, ct);
            pay.CashVoucherId = cv.Id;
        }
        else
        {
            var bv = await _bank.UpsertVoucherAsync(tenantId, userId, new FinBankVoucherUpsertRequest(
                null, null, pay.BankAccountId!.Value, "Debit", pay.PayDate, pay.Amount,
                $"Thanh toán AP {pay.Code} · {vendor.Name}", pay.Code, vendor.Code, null, pay.PeriodId, $"AP {pay.Code}"), ct);
            bv = await _bank.PostVoucherAsync(tenantId, userId, bv.Id, ct);
            pay.BankVoucherId = bv.Id;
        }

        foreach (var a in allocs)
        {
            var inv = await RequireInvoice(tenantId, a.ApInvoiceId, ct);
            inv.PaidAmount = decimal.Round(inv.PaidAmount + a.Amount, 2);
            inv.Status = inv.PaidAmount + 0.01m >= inv.TotalAmount ? "Paid"
                : inv.PaidAmount > 0 ? "Partial" : inv.Status;
            inv.UpdatedBy = userId;
        }

        if (pay.PaymentRequestId is Guid rid)
        {
            var req = await RequireRequest(tenantId, rid, ct);
            req.Status = "Paid";
            req.PaymentId = pay.Id;
            req.PaymentCode = pay.Code;
            req.UpdatedBy = userId;
        }

        pay.Status = "Posted";
        pay.PostedAt = DateTimeOffset.UtcNow;
        pay.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapPaymentsAsync(tenantId, [pay], ct))[0];
    }

    public async Task<FinApPaymentDto> PayFromRequestAsync(
        Guid tenantId, Guid userId, Guid requestId, CancellationToken ct = default)
    {
        var req = await RequireRequest(tenantId, requestId, ct);
        if (req.Status != "Approved") throw new AppException("Chỉ thanh toán đề nghị Approved.");
        var lines = await _db.FinApPaymentRequestLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PaymentRequestId == req.Id && !x.IsDeleted).ToListAsync(ct);
        if (lines.Count == 0) throw new AppException("Đề nghị không có dòng.");

        var payment = await UpsertPaymentAsync(tenantId, userId, new FinApPaymentUpsertRequest(
            null, null, req.VendorId, DateTimeOffset.UtcNow, req.PayMethod,
            req.CashFundId, req.BankAccountId, req.Id, null, $"Từ đề nghị {req.Code}",
            lines.Select(x => new FinApPaymentAllocationInput(x.ApInvoiceId, x.Amount)).ToList()), ct);
        return await PostPaymentAsync(tenantId, userId, payment.Id, ct);
    }

    public async Task<FinApAgingDto> GetAgingAsync(
        Guid tenantId, DateTimeOffset? asOf = null, CancellationToken ct = default)
    {
        var asOfDt = (asOf ?? DateTimeOffset.UtcNow).Date;
        var open = await _db.FinApInvoices.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && (x.Status == "Open" || x.Status == "Partial"))
            .ToListAsync(ct);
        var vids = open.Select(x => x.VendorId).Distinct().ToList();
        var vendors = vids.Count == 0 ? new Dictionary<Guid, PurVendor>()
            : await _db.PurVendors.AsNoTracking().Where(x => vids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);

        static string BucketOf(int days) => days switch
        {
            <= 0 => "Current",
            <= 30 => "1-30",
            <= 60 => "31-60",
            <= 90 => "61-90",
            _ => ">90"
        };

        var rows = new List<FinApAgingRowDto>();
        foreach (var g in open.GroupBy(x => x.VendorId))
        {
            vendors.TryGetValue(g.Key, out var v);
            decimal c = 0, d1 = 0, d2 = 0, d3 = 0, d4 = 0;
            foreach (var inv in g)
            {
                var openAmt = Math.Max(0, inv.TotalAmount - inv.PaidAmount);
                var days = (asOfDt - inv.DueDate.Date).Days;
                switch (BucketOf(days))
                {
                    case "Current": c += openAmt; break;
                    case "1-30": d1 += openAmt; break;
                    case "31-60": d2 += openAmt; break;
                    case "61-90": d3 += openAmt; break;
                    default: d4 += openAmt; break;
                }
            }
            rows.Add(new FinApAgingRowDto(
                g.Key, v?.Code ?? "", v?.Name ?? "", c, d1, d2, d3, d4, c + d1 + d2 + d3 + d4));
        }

        var buckets = new[]
        {
            new FinApAgingBucketDto("Current", rows.Sum(x => x.Current), open.Count(x => (asOfDt - x.DueDate.Date).Days <= 0)),
            new FinApAgingBucketDto("1-30", rows.Sum(x => x.D1To30), open.Count(x => { var d = (asOfDt - x.DueDate.Date).Days; return d is > 0 and <= 30; })),
            new FinApAgingBucketDto("31-60", rows.Sum(x => x.D31To60), open.Count(x => { var d = (asOfDt - x.DueDate.Date).Days; return d is > 30 and <= 60; })),
            new FinApAgingBucketDto("61-90", rows.Sum(x => x.D61To90), open.Count(x => { var d = (asOfDt - x.DueDate.Date).Days; return d is > 60 and <= 90; })),
            new FinApAgingBucketDto(">90", rows.Sum(x => x.Over90), open.Count(x => (asOfDt - x.DueDate.Date).Days > 90)),
        };

        return new FinApAgingDto(asOfDt, buckets, rows.OrderByDescending(x => x.Total).ToList());
    }

    private async Task ValidateAllocations(
        Guid tenantId, Guid vendorId, IReadOnlyList<FinApPaymentAllocationInput> allocs, CancellationToken ct)
    {
        var ids = allocs.Select(x => x.ApInvoiceId).Distinct().ToList();
        var invoices = await _db.FinApInvoices
            .Where(x => x.TenantId == tenantId && ids.Contains(x.Id) && !x.IsDeleted).ToListAsync(ct);
        if (invoices.Count != ids.Count) throw new AppException("Có HĐ AP không hợp lệ.");
        foreach (var inv in invoices)
        {
            if (inv.VendorId != vendorId) throw new AppException($"HĐ {inv.Code} khác NCC.");
            if (inv.Status is not ("Open" or "Partial")) throw new AppException($"HĐ {inv.Code} không Open/Partial.");
            var amt = allocs.Where(x => x.ApInvoiceId == inv.Id).Sum(x => x.Amount);
            var open = inv.TotalAmount - inv.PaidAmount;
            if (amt > open + 0.01m) throw new AppException($"HĐ {inv.Code} vượt số còn lại ({open:N0}).");
        }
    }

    private async Task<IReadOnlyList<FinApInvoiceDto>> MapInvoicesAsync(
        Guid tenantId, List<FinApInvoice> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<FinApInvoiceDto>();
        var vids = list.Select(x => x.VendorId).Distinct().ToList();
        var pids = list.Where(x => x.PeriodId.HasValue).Select(x => x.PeriodId!.Value).Distinct().ToList();
        var vendors = await _db.PurVendors.AsNoTracking().Where(x => vids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var periods = pids.Count == 0 ? new Dictionary<Guid, FinPeriod>()
            : await _db.FinPeriods.AsNoTracking().Where(x => pids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        return list.Select(i =>
        {
            vendors.TryGetValue(i.VendorId, out var v);
            FinPeriod? pe = null;
            if (i.PeriodId is Guid pid) periods.TryGetValue(pid, out pe);
            var open = Math.Max(0, i.TotalAmount - i.PaidAmount);
            return new FinApInvoiceDto(
                i.Id, i.Code, i.VendorId, v?.Code, v?.Name, i.VendorInvoiceNo, i.PurVendorInvoiceId,
                i.InvoiceDate, i.DueDate, i.SubTotal, i.TaxAmount, i.TotalAmount, i.PaidAmount, open,
                i.Status, i.PeriodId, pe?.Code, i.FinJournalId, i.FinJournalCode, i.PostedAt, i.Note);
        }).ToList();
    }

    private async Task<IReadOnlyList<FinApPaymentRequestDto>> MapRequestsAsync(
        Guid tenantId, List<FinApPaymentRequest> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<FinApPaymentRequestDto>();
        var ids = list.Select(x => x.Id).ToList();
        var vids = list.Select(x => x.VendorId).Distinct().ToList();
        var vendors = await _db.PurVendors.AsNoTracking().Where(x => vids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var lines = await _db.FinApPaymentRequestLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.PaymentRequestId) && !x.IsDeleted).ToListAsync(ct);
        var invIds = lines.Select(x => x.ApInvoiceId).Distinct().ToList();
        var invoices = invIds.Count == 0 ? new Dictionary<Guid, FinApInvoice>()
            : await _db.FinApInvoices.AsNoTracking().Where(x => invIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);

        return list.Select(r =>
        {
            vendors.TryGetValue(r.VendorId, out var v);
            var rLines = lines.Where(x => x.PaymentRequestId == r.Id).Select(l =>
            {
                invoices.TryGetValue(l.ApInvoiceId, out var inv);
                var open = inv is null ? 0 : Math.Max(0, inv.TotalAmount - inv.PaidAmount);
                return new FinApPaymentRequestLineDto(l.ApInvoiceId, inv?.Code, l.Amount, open);
            }).ToList();
            return new FinApPaymentRequestDto(
                r.Id, r.Code, r.VendorId, v?.Code, v?.Name, r.RequestDate, r.RequestAmount, r.PayMethod,
                r.CashFundId, r.BankAccountId, r.Status, r.PaymentId, r.PaymentCode, r.ApprovedAt, r.Note, rLines);
        }).ToList();
    }

    private async Task<IReadOnlyList<FinApPaymentDto>> MapPaymentsAsync(
        Guid tenantId, List<FinApPayment> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<FinApPaymentDto>();
        var ids = list.Select(x => x.Id).ToList();
        var vids = list.Select(x => x.VendorId).Distinct().ToList();
        var vendors = await _db.PurVendors.AsNoTracking().Where(x => vids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var allocs = await _db.FinApPaymentAllocations.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.PaymentId) && !x.IsDeleted).ToListAsync(ct);
        var invIds = allocs.Select(x => x.ApInvoiceId).Distinct().ToList();
        var invoices = invIds.Count == 0 ? new Dictionary<Guid, FinApInvoice>()
            : await _db.FinApInvoices.AsNoTracking().Where(x => invIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);

        return list.Select(p =>
        {
            vendors.TryGetValue(p.VendorId, out var v);
            var aLines = allocs.Where(x => x.PaymentId == p.Id).Select(a =>
            {
                invoices.TryGetValue(a.ApInvoiceId, out var inv);
                return new FinApPaymentAllocationDto(a.ApInvoiceId, inv?.Code, a.Amount);
            }).ToList();
            return new FinApPaymentDto(
                p.Id, p.Code, p.VendorId, v?.Code, v?.Name, p.PayDate, p.Amount, p.PayMethod,
                p.CashFundId, p.BankAccountId, p.PaymentRequestId, p.CashVoucherId, p.BankVoucherId,
                p.Status, p.FinJournalId, p.FinJournalCode, p.PostedAt, p.Note, aLines);
        }).ToList();
    }

    private async Task<FinApInvoice> RequireInvoice(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FinApInvoices.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy HĐ AP.");

    private async Task<FinApPaymentRequest> RequireRequest(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FinApPaymentRequests.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy đề nghị TT.");

    private async Task<FinApPayment> RequirePayment(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FinApPayments.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy phiếu TT AP.");

    private async Task<PurVendor> RequireVendor(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.PurVendors.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy NCC.");

    private async Task<FinAccount> RequireAccount(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FinAccounts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy tài khoản.");

    private async Task<FinPeriod> RequirePeriod(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FinPeriods.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy kỳ KT.");

    private async Task<string> NextCodeAsync(string prefix, CancellationToken ct, Guid tenantId)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var stem = $"{prefix}-{today}-";
        string? last = prefix switch
        {
            "AP" => await _db.FinApInvoices.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Code.StartsWith(stem))
                .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct),
            "DNTT" => await _db.FinApPaymentRequests.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Code.StartsWith(stem))
                .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct),
            _ => await _db.FinApPayments.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Code.StartsWith(stem))
                .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct),
        };
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
