using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Fin;
using Erp.Application.Interfaces.Services.Fin;
using Erp.Domain.Entities.Fin;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Fin;

public sealed class FinCashService : IFinCashService
{
    private readonly AppDbContext _db;
    private readonly IFinAccountingService _fin;

    public FinCashService(AppDbContext db, IFinAccountingService fin)
    {
        _db = db;
        _fin = fin;
    }

    public async Task<IReadOnlyList<FinCashFundDto>> ListFundsAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FinCashFunds.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.Code).Take(100).ToListAsync(ct);
        return await MapFundsAsync(tenantId, list, ct);
    }

    public async Task<FinCashFundDto> UpsertFundAsync(
        Guid tenantId, Guid userId, FinCashFundUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên quỹ");
        var status = string.IsNullOrWhiteSpace(req.Status) ? "Active" : req.Status.Trim();
        if (status is not ("Active" or "Inactive")) throw new AppException("Status: Active | Inactive.");
        var acc = await RequireAccount(tenantId, req.CashAccountId, ct);
        if (!acc.IsPostable) throw new AppException("TK quỹ phải hạch toán được.");

        string? custodian = NullIfEmpty(req.CustodianName);
        if (req.CustodianUserId is Guid uid)
        {
            var u = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == uid && x.TenantId == tenantId, ct);
            custodian ??= u?.DisplayName ?? u?.Username;
        }

        FinCashFund entity;
        if (req.Id is Guid id)
        {
            entity = await RequireFund(tenantId, id, ct);
            if (!entity.Code.Equals(code, StringComparison.OrdinalIgnoreCase)
                && await _db.FinCashFunds.AnyAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã quỹ đã tồn tại.");
        }
        else
        {
            if (await _db.FinCashFunds.AnyAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã quỹ đã tồn tại.");
            entity = new FinCashFund { TenantId = tenantId, CreatedBy = userId };
            _db.FinCashFunds.Add(entity);
        }

        entity.Code = code;
        entity.Name = name;
        entity.CashAccountId = req.CashAccountId;
        entity.CustodianUserId = req.CustodianUserId;
        entity.CustodianName = custodian;
        entity.OpeningBalance = Math.Max(0, decimal.Round(req.OpeningBalance ?? entity.OpeningBalance, 2));
        entity.Status = status;
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapFundsAsync(tenantId, [entity], ct))[0];
    }

    public async Task<IReadOnlyList<FinCashVoucherDto>> ListVouchersAsync(
        Guid tenantId, Guid? fundId = null, string? type = null, CancellationToken ct = default)
    {
        var q = _db.FinCashVouchers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (fundId is Guid fid) q = q.Where(x => x.FundId == fid);
        if (!string.IsNullOrWhiteSpace(type)) q = q.Where(x => x.VoucherType == type.Trim());
        var list = await q.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Code).Take(300).ToListAsync(ct);
        return await MapVouchersAsync(tenantId, list, ct);
    }

    public async Task<FinCashVoucherDto> UpsertVoucherAsync(
        Guid tenantId, Guid userId, FinCashVoucherUpsertRequest req, CancellationToken ct = default)
    {
        var fund = await RequireFund(tenantId, req.FundId, ct);
        if (fund.Status != "Active") throw new AppException("Quỹ không Active.");
        var vtype = (req.VoucherType ?? "").Trim();
        if (vtype is not ("Receipt" or "Payment")) throw new AppException("Loại: Receipt | Payment.");
        if (req.Amount <= 0) throw new AppException("Số tiền > 0.");
        var desc = Req(req.Description, 500, "Diễn giải");
        if (req.CounterAccountId is Guid caid)
            _ = await RequireAccount(tenantId, caid, ct);
        if (req.PeriodId is Guid pid)
        {
            var period = await RequirePeriod(tenantId, pid, ct);
            if (period.Status == "Locked") throw new AppException("Kỳ đã khóa sổ.");
        }

        FinCashVoucher entity;
        if (req.Id is Guid id)
        {
            entity = await RequireVoucher(tenantId, id, ct);
            if (entity.Status != "Draft") throw new AppException("Chỉ sửa phiếu Draft.");
        }
        else
        {
            var prefix = vtype == "Receipt" ? "PT" : "PC";
            entity = new FinCashVoucher
            {
                TenantId = tenantId,
                Code = string.IsNullOrWhiteSpace(req.Code)
                    ? await NextCodeAsync(tenantId, prefix, ct)
                    : NormCode(req.Code),
                CreatedByUserId = userId,
                CreatedBy = userId
            };
            if (await _db.FinCashVouchers.AnyAsync(x => x.TenantId == tenantId && x.Code == entity.Code && !x.IsDeleted, ct))
                throw new AppException("Mã phiếu đã tồn tại.");
            _db.FinCashVouchers.Add(entity);
        }

        entity.FundId = fund.Id;
        entity.VoucherType = vtype;
        entity.DocDate = req.DocDate == default ? DateTimeOffset.UtcNow : req.DocDate;
        entity.Amount = decimal.Round(req.Amount, 2);
        entity.Description = desc;
        entity.PartnerCode = NullIfEmpty(req.PartnerCode)?.ToUpperInvariant();
        entity.CounterAccountId = req.CounterAccountId;
        entity.PeriodId = req.PeriodId;
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapVouchersAsync(tenantId, [entity], ct))[0];
    }

    public async Task<FinCashVoucherDto> PostVoucherAsync(
        Guid tenantId, Guid userId, Guid voucherId, CancellationToken ct = default)
    {
        var v = await RequireVoucher(tenantId, voucherId, ct);
        if (v.Status != "Draft") throw new AppException("Chỉ ghi sổ phiếu Draft.");
        var fund = await RequireFund(tenantId, v.FundId, ct);
        if (fund.Status != "Active") throw new AppException("Quỹ không Active.");

        if (v.VoucherType == "Payment")
        {
            var book = await CalcBookBalance(tenantId, fund, ct);
            if (book + 0.01m < v.Amount)
                throw new AppException($"Quỹ không đủ tiền (số dư {book:N0}).");
        }

        // UC_FIN_019: luôn tạo JE thật (auto-resolve TK đối ứng + kỳ Open nếu thiếu).
        var periodId = await _fin.ResolveOpenPeriodIdAsync(tenantId, v.PeriodId, v.DocDate, ct);
        v.PeriodId = periodId;
        var cashId = fund.CashAccountId;
        var counterId = v.CounterAccountId
            ?? await _fin.ResolvePostableAccountIdAsync(
                tenantId,
                v.VoucherType == "Receipt" ? ["131", "511"] : ["331", "642", "156"],
                v.VoucherType == "Receipt" ? "TK đối ứng thu (131*/511*)" : "TK đối ứng chi (331*/642*)",
                ct);
        v.CounterAccountId = counterId;

        var lines = v.VoucherType == "Receipt"
            ? new List<FinJournalLineUpsertRequest>
            {
                new(null, cashId, v.Amount, 0, v.PartnerCode, null, "Thu quỹ"),
                new(null, counterId, 0, v.Amount, v.PartnerCode, null, "Đối ứng thu"),
            }
            : new List<FinJournalLineUpsertRequest>
            {
                new(null, counterId, v.Amount, 0, v.PartnerCode, null, "Đối ứng chi"),
                new(null, cashId, 0, v.Amount, v.PartnerCode, null, "Chi quỹ"),
            };

        var je = await _fin.CreateAutoJournalAsync(tenantId, userId, new FinJournalUpsertRequest(
            null, null, periodId, v.DocDate, $"{v.VoucherType} {v.Code}: {v.Description}",
            v.PartnerCode, null, "Auto", lines), ct);
        je = await _fin.PostJournalAsync(tenantId, userId, je.Id, ct);
        v.FinJournalId = je.Id;
        v.FinJournalCode = je.Code;

        v.Status = "Posted";
        v.PostedAt = DateTimeOffset.UtcNow;
        v.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapVouchersAsync(tenantId, [v], ct))[0];
    }

    public async Task<FinCashVoucherDto> VoidVoucherAsync(
        Guid tenantId, Guid userId, Guid voucherId, string? note = null, CancellationToken ct = default)
    {
        var v = await RequireVoucher(tenantId, voucherId, ct);
        if (v.Status == "Void") throw new AppException("Phiếu đã hủy.");
        if (v.Status == "Posted" && v.FinJournalId.HasValue)
            throw new AppException("Phiếu đã đẩy BT — đảo BT trước khi hủy (Cap sau).");
        v.Status = "Void";
        if (!string.IsNullOrWhiteSpace(note)) v.Note = note.Trim();
        v.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapVouchersAsync(tenantId, [v], ct))[0];
    }

    public async Task<FinCashBookDto> GetCashBookAsync(
        Guid tenantId, Guid fundId, DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken ct = default)
    {
        var fund = await RequireFund(tenantId, fundId, ct, track: false);
        var q = _db.FinCashVouchers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.FundId == fundId && !x.IsDeleted && x.Status == "Posted");
        if (from is DateTimeOffset f) q = q.Where(x => x.DocDate >= f);
        if (to is DateTimeOffset t) q = q.Where(x => x.DocDate <= t);
        var vouchers = await q.OrderBy(x => x.DocDate).ThenBy(x => x.Code).ToListAsync(ct);

        var bal = fund.OpeningBalance;
        // opening before `from`: add earlier posted
        if (from is DateTimeOffset fromDt)
        {
            var prior = await _db.FinCashVouchers.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.FundId == fundId && !x.IsDeleted
                            && x.Status == "Posted" && x.DocDate < fromDt)
                .Select(x => new { x.VoucherType, x.Amount }).ToListAsync(ct);
            bal += prior.Where(x => x.VoucherType == "Receipt").Sum(x => x.Amount)
                   - prior.Where(x => x.VoucherType == "Payment").Sum(x => x.Amount);
        }

        var opening = bal;
        var rows = new List<FinCashBookRowDto>();
        decimal totalR = 0, totalP = 0;
        foreach (var v in vouchers)
        {
            var r = v.VoucherType == "Receipt" ? v.Amount : 0;
            var p = v.VoucherType == "Payment" ? v.Amount : 0;
            bal += r - p;
            totalR += r;
            totalP += p;
            rows.Add(new FinCashBookRowDto(
                v.DocDate, v.Code, v.VoucherType, v.Description, v.PartnerCode, r, p, bal));
        }

        return new FinCashBookDto(
            fund.Id, fund.Code, fund.Name, opening, totalR, totalP, bal, rows);
    }

    private async Task<decimal> CalcBookBalance(Guid tenantId, FinCashFund fund, CancellationToken ct)
    {
        var posted = await _db.FinCashVouchers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.FundId == fund.Id && !x.IsDeleted && x.Status == "Posted")
            .Select(x => new { x.VoucherType, x.Amount }).ToListAsync(ct);
        return fund.OpeningBalance
               + posted.Where(x => x.VoucherType == "Receipt").Sum(x => x.Amount)
               - posted.Where(x => x.VoucherType == "Payment").Sum(x => x.Amount);
    }

    private async Task<IReadOnlyList<FinCashFundDto>> MapFundsAsync(
        Guid tenantId, List<FinCashFund> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<FinCashFundDto>();
        var ids = list.Select(x => x.Id).ToList();
        var aids = list.Select(x => x.CashAccountId).Distinct().ToList();
        var accs = await _db.FinAccounts.AsNoTracking()
            .Where(x => aids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var posted = await _db.FinCashVouchers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.FundId) && !x.IsDeleted && x.Status == "Posted")
            .GroupBy(x => x.FundId)
            .Select(g => new
            {
                g.Key,
                R = g.Where(x => x.VoucherType == "Receipt").Sum(x => x.Amount),
                P = g.Where(x => x.VoucherType == "Payment").Sum(x => x.Amount)
            }).ToDictionaryAsync(x => x.Key, ct);

        return list.Select(f =>
        {
            accs.TryGetValue(f.CashAccountId, out var a);
            posted.TryGetValue(f.Id, out var p);
            var r = p?.R ?? 0;
            var pay = p?.P ?? 0;
            return new FinCashFundDto(
                f.Id, f.Code, f.Name, f.CashAccountId, a?.Code, a?.Name,
                f.CustodianUserId, f.CustodianName, f.OpeningBalance, f.Status, f.Note,
                r, pay, f.OpeningBalance + r - pay);
        }).ToList();
    }

    private async Task<IReadOnlyList<FinCashVoucherDto>> MapVouchersAsync(
        Guid tenantId, List<FinCashVoucher> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<FinCashVoucherDto>();
        var fids = list.Select(x => x.FundId).Distinct().ToList();
        var aids = list.Where(x => x.CounterAccountId.HasValue).Select(x => x.CounterAccountId!.Value).Distinct().ToList();
        var pids = list.Where(x => x.PeriodId.HasValue).Select(x => x.PeriodId!.Value).Distinct().ToList();
        var funds = await _db.FinCashFunds.AsNoTracking().Where(x => fids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
        var accs = aids.Count == 0 ? new Dictionary<Guid, FinAccount>()
            : await _db.FinAccounts.AsNoTracking().Where(x => aids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var periods = pids.Count == 0 ? new Dictionary<Guid, FinPeriod>()
            : await _db.FinPeriods.AsNoTracking().Where(x => pids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);

        return list.Select(v =>
        {
            funds.TryGetValue(v.FundId, out var f);
            FinAccount? ca = null;
            if (v.CounterAccountId is Guid cid) accs.TryGetValue(cid, out ca);
            FinPeriod? pe = null;
            if (v.PeriodId is Guid pid) periods.TryGetValue(pid, out pe);
            return new FinCashVoucherDto(
                v.Id, v.Code, v.FundId, f?.Code, f?.Name, v.VoucherType, v.DocDate, v.Amount, v.Description,
                v.PartnerCode, v.CounterAccountId, ca?.Code, v.PeriodId, pe?.Code, v.Status,
                v.FinJournalId, v.FinJournalCode, v.PostedAt, v.Note);
        }).ToList();
    }

    private async Task<FinCashFund> RequireFund(Guid tenantId, Guid id, CancellationToken ct, bool track = true)
    {
        var q = track ? _db.FinCashFunds.AsQueryable() : _db.FinCashFunds.AsNoTracking();
        return await q.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy quỹ.");
    }

    private async Task<FinCashVoucher> RequireVoucher(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FinCashVouchers.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy phiếu quỹ.");

    private async Task<FinAccount> RequireAccount(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FinAccounts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy tài khoản.");

    private async Task<FinPeriod> RequirePeriod(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FinPeriods.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy kỳ KT.");

    private async Task<string> NextCodeAsync(Guid tenantId, string prefix, CancellationToken ct)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var stem = $"{prefix}-{today}-";
        var last = await _db.FinCashVouchers.AsNoTracking()
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

    private static string Req(string? s, int max, string label)
    {
        var v = (s ?? "").Trim();
        if (v.Length is < 1 || v.Length > max) throw new AppException($"{label} 1–{max} ký tự.");
        return v;
    }

    private static string? NullIfEmpty(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
