using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Fin;
using Erp.Application.Interfaces.Services.Fin;
using Erp.Domain.Entities.Fin;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Fin;

public sealed class FinBankService : IFinBankService
{
    private readonly AppDbContext _db;
    private readonly IFinAccountingService _fin;

    public FinBankService(AppDbContext db, IFinAccountingService fin)
    {
        _db = db;
        _fin = fin;
    }

    public async Task<IReadOnlyList<FinBankAccountDto>> ListAccountsAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FinBankAccounts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.Code).Take(100).ToListAsync(ct);
        return await MapAccountsAsync(tenantId, list, ct);
    }

    public async Task<FinBankAccountDto> UpsertAccountAsync(
        Guid tenantId, Guid userId, FinBankAccountUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên TK");
        var bank = Req(req.BankName, 200, "Ngân hàng");
        var accNo = Req(req.AccountNumber, 60, "Số TK");
        var status = string.IsNullOrWhiteSpace(req.Status) ? "Active" : req.Status.Trim();
        if (status is not ("Active" or "Inactive")) throw new AppException("Status: Active | Inactive.");
        var gl = await RequireAccount(tenantId, req.GlAccountId, ct);
        if (!gl.IsPostable) throw new AppException("TK hạch toán phải postable.");

        FinBankAccount entity;
        if (req.Id is Guid id)
        {
            entity = await RequireBank(tenantId, id, ct);
            if (!entity.Code.Equals(code, StringComparison.OrdinalIgnoreCase)
                && await _db.FinBankAccounts.AnyAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã TKNH đã tồn tại.");
        }
        else
        {
            if (await _db.FinBankAccounts.AnyAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct))
                throw new AppException("Mã TKNH đã tồn tại.");
            entity = new FinBankAccount { TenantId = tenantId, CreatedBy = userId };
            _db.FinBankAccounts.Add(entity);
        }

        entity.Code = code;
        entity.Name = name;
        entity.BankName = bank;
        entity.AccountNumber = accNo;
        entity.BranchName = NullIfEmpty(req.BranchName);
        entity.GlAccountId = req.GlAccountId;
        entity.OpeningBalance = Math.Max(0, decimal.Round(req.OpeningBalance ?? entity.OpeningBalance, 2));
        entity.Status = status;
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapAccountsAsync(tenantId, [entity], ct))[0];
    }

    public async Task<IReadOnlyList<FinBankVoucherDto>> ListVouchersAsync(
        Guid tenantId, Guid? bankAccountId = null, string? type = null, CancellationToken ct = default)
    {
        var q = _db.FinBankVouchers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (bankAccountId is Guid bid) q = q.Where(x => x.BankAccountId == bid);
        if (!string.IsNullOrWhiteSpace(type)) q = q.Where(x => x.VoucherType == type.Trim());
        var list = await q.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Code).Take(300).ToListAsync(ct);
        return await MapVouchersAsync(tenantId, list, ct);
    }

    public async Task<FinBankVoucherDto> UpsertVoucherAsync(
        Guid tenantId, Guid userId, FinBankVoucherUpsertRequest req, CancellationToken ct = default)
    {
        var bank = await RequireBank(tenantId, req.BankAccountId, ct);
        if (bank.Status != "Active") throw new AppException("TKNH không Active.");
        var vtype = (req.VoucherType ?? "").Trim();
        if (vtype is not ("Credit" or "Debit")) throw new AppException("Loại: Credit | Debit.");
        if (req.Amount <= 0) throw new AppException("Số tiền > 0.");
        var desc = Req(req.Description, 500, "Diễn giải");
        if (req.CounterAccountId is Guid caid)
            _ = await RequireAccount(tenantId, caid, ct);
        if (req.PeriodId is Guid pid)
        {
            var period = await RequirePeriod(tenantId, pid, ct);
            if (period.Status == "Locked") throw new AppException("Kỳ đã khóa sổ.");
        }

        FinBankVoucher entity;
        if (req.Id is Guid id)
        {
            entity = await RequireVoucher(tenantId, id, ct);
            if (entity.Status != "Draft") throw new AppException("Chỉ sửa giấy báo Draft.");
        }
        else
        {
            var prefix = vtype == "Credit" ? "GBC" : "GBN";
            entity = new FinBankVoucher
            {
                TenantId = tenantId,
                Code = string.IsNullOrWhiteSpace(req.Code)
                    ? await NextCodeAsync(_db.FinBankVouchers, tenantId, prefix, ct)
                    : NormCode(req.Code),
                CreatedByUserId = userId,
                CreatedBy = userId
            };
            if (await _db.FinBankVouchers.AnyAsync(x => x.TenantId == tenantId && x.Code == entity.Code && !x.IsDeleted, ct))
                throw new AppException("Mã giấy báo đã tồn tại.");
            _db.FinBankVouchers.Add(entity);
        }

        entity.BankAccountId = bank.Id;
        entity.VoucherType = vtype;
        entity.DocDate = req.DocDate == default ? DateTimeOffset.UtcNow : req.DocDate;
        entity.Amount = decimal.Round(req.Amount, 2);
        entity.Description = desc;
        entity.BankRef = NullIfEmpty(req.BankRef);
        entity.PartnerCode = NullIfEmpty(req.PartnerCode)?.ToUpperInvariant();
        entity.CounterAccountId = req.CounterAccountId;
        entity.PeriodId = req.PeriodId;
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapVouchersAsync(tenantId, [entity], ct))[0];
    }

    public async Task<FinBankVoucherDto> PostVoucherAsync(
        Guid tenantId, Guid userId, Guid voucherId, CancellationToken ct = default)
    {
        var v = await RequireVoucher(tenantId, voucherId, ct);
        if (v.Status != "Draft") throw new AppException("Chỉ ghi sổ giấy báo Draft.");
        var bank = await RequireBank(tenantId, v.BankAccountId, ct);
        if (bank.Status != "Active") throw new AppException("TKNH không Active.");

        if (v.VoucherType == "Debit")
        {
            var book = await CalcBookBalance(tenantId, bank, ct);
            if (book + 0.01m < v.Amount)
                throw new AppException($"Số dư NH không đủ (số dư {book:N0}).");
        }

        // UC_FIN_025: luôn tạo JE thật (auto-resolve TK đối ứng + kỳ Open nếu thiếu).
        var periodId = await _fin.ResolveOpenPeriodIdAsync(tenantId, v.PeriodId, v.DocDate, ct);
        v.PeriodId = periodId;
        var bankGl = bank.GlAccountId;
        var counterId = v.CounterAccountId
            ?? await _fin.ResolvePostableAccountIdAsync(
                tenantId,
                v.VoucherType == "Credit" ? ["131", "511"] : ["331", "642", "156"],
                v.VoucherType == "Credit" ? "TK đối ứng Báo Có (131*/511*)" : "TK đối ứng Báo Nợ (331*/642*)",
                ct);
        v.CounterAccountId = counterId;

        var lines = v.VoucherType == "Credit"
            ? new List<FinJournalLineUpsertRequest>
            {
                new(null, bankGl, v.Amount, 0, v.PartnerCode, null, "Báo Có NH"),
                new(null, counterId, 0, v.Amount, v.PartnerCode, null, "Đối ứng Có"),
            }
            : new List<FinJournalLineUpsertRequest>
            {
                new(null, counterId, v.Amount, 0, v.PartnerCode, null, "Đối ứng Nợ"),
                new(null, bankGl, 0, v.Amount, v.PartnerCode, null, "Báo Nợ NH"),
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

    public async Task<FinBankVoucherDto> VoidVoucherAsync(
        Guid tenantId, Guid userId, Guid voucherId, string? note = null, CancellationToken ct = default)
    {
        var v = await RequireVoucher(tenantId, voucherId, ct);
        if (v.Status == "Void") throw new AppException("Giấy báo đã hủy.");
        if (v.Status == "Posted" && v.FinJournalId.HasValue)
            throw new AppException("Giấy báo đã đẩy BT — đảo BT trước khi hủy (Cap sau).");
        if (await _db.FinBankStatementLines.AnyAsync(
                x => x.TenantId == tenantId && x.MatchedVoucherId == voucherId && !x.IsDeleted && x.Status == "Matched", ct))
            throw new AppException("Giấy báo đã khớp sao kê — bỏ khớp trước.");
        v.Status = "Void";
        if (!string.IsNullOrWhiteSpace(note)) v.Note = note.Trim();
        v.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapVouchersAsync(tenantId, [v], ct))[0];
    }

    public async Task<IReadOnlyList<FinBankTransferRequestDto>> ListTransfersAsync(
        Guid tenantId, Guid? bankAccountId = null, CancellationToken ct = default)
    {
        var q = _db.FinBankTransferRequests.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (bankAccountId is Guid bid) q = q.Where(x => x.FromBankAccountId == bid);
        var list = await q.OrderByDescending(x => x.RequestDate).ThenByDescending(x => x.Code).Take(200).ToListAsync(ct);
        return await MapTransfersAsync(tenantId, list, ct);
    }

    public async Task<FinBankTransferRequestDto> UpsertTransferAsync(
        Guid tenantId, Guid userId, FinBankTransferUpsertRequest req, CancellationToken ct = default)
    {
        var bank = await RequireBank(tenantId, req.FromBankAccountId, ct);
        if (bank.Status != "Active") throw new AppException("TKNH không Active.");
        if (req.Amount <= 0) throw new AppException("Số tiền > 0.");
        var benName = Req(req.BeneficiaryName, 200, "Người thụ hưởng");
        var benAcc = Req(req.BeneficiaryAccount, 60, "TK thụ hưởng");
        var benBank = Req(req.BeneficiaryBank, 200, "NH thụ hưởng");
        var desc = Req(req.Description, 500, "Nội dung CK");
        if (req.CounterAccountId is Guid caid)
            _ = await RequireAccount(tenantId, caid, ct);
        if (req.PeriodId is Guid pid)
        {
            var period = await RequirePeriod(tenantId, pid, ct);
            if (period.Status == "Locked") throw new AppException("Kỳ đã khóa sổ.");
        }

        FinBankTransferRequest entity;
        if (req.Id is Guid id)
        {
            entity = await RequireTransfer(tenantId, id, ct);
            if (entity.Status != "Draft") throw new AppException("Chỉ sửa đề nghị Draft.");
        }
        else
        {
            entity = new FinBankTransferRequest
            {
                TenantId = tenantId,
                Code = string.IsNullOrWhiteSpace(req.Code)
                    ? await NextCodeAsync(_db.FinBankTransferRequests, tenantId, "DNCK", ct)
                    : NormCode(req.Code),
                RequestedByUserId = userId,
                CreatedBy = userId
            };
            if (await _db.FinBankTransferRequests.AnyAsync(x => x.TenantId == tenantId && x.Code == entity.Code && !x.IsDeleted, ct))
                throw new AppException("Mã đề nghị đã tồn tại.");
            _db.FinBankTransferRequests.Add(entity);
        }

        entity.FromBankAccountId = bank.Id;
        entity.BeneficiaryName = benName;
        entity.BeneficiaryAccount = benAcc;
        entity.BeneficiaryBank = benBank;
        entity.Amount = decimal.Round(req.Amount, 2);
        entity.Description = desc;
        entity.RequestDate = req.RequestDate ?? DateTimeOffset.UtcNow;
        entity.CounterAccountId = req.CounterAccountId;
        entity.PeriodId = req.PeriodId;
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapTransfersAsync(tenantId, [entity], ct))[0];
    }

    public async Task<FinBankTransferRequestDto> SubmitTransferAsync(
        Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var t = await RequireTransfer(tenantId, id, ct);
        if (t.Status != "Draft") throw new AppException("Chỉ gửi đề nghị Draft.");
        t.Status = "Submitted";
        t.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapTransfersAsync(tenantId, [t], ct))[0];
    }

    public async Task<FinBankTransferRequestDto> ApproveTransferAsync(
        Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var t = await RequireTransfer(tenantId, id, ct);
        if (t.Status != "Submitted") throw new AppException("Chỉ duyệt đề nghị Submitted.");
        t.Status = "Approved";
        t.ApprovedByUserId = userId;
        t.ApprovedAt = DateTimeOffset.UtcNow;
        t.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapTransfersAsync(tenantId, [t], ct))[0];
    }

    public async Task<FinBankTransferRequestDto> RejectTransferAsync(
        Guid tenantId, Guid userId, Guid id, string? note = null, CancellationToken ct = default)
    {
        var t = await RequireTransfer(tenantId, id, ct);
        if (t.Status is not ("Submitted" or "Approved")) throw new AppException("Chỉ từ chối Submitted/Approved.");
        t.Status = "Rejected";
        if (!string.IsNullOrWhiteSpace(note)) t.Note = note.Trim();
        t.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapTransfersAsync(tenantId, [t], ct))[0];
    }

    public async Task<FinBankTransferRequestDto> ExecuteTransferAsync(
        Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var t = await RequireTransfer(tenantId, id, ct);
        if (t.Status != "Approved") throw new AppException("Chỉ thực hiện đề nghị Approved.");

        var voucher = await UpsertVoucherAsync(tenantId, userId, new FinBankVoucherUpsertRequest(
            null, null, t.FromBankAccountId, "Debit", t.RequestDate, t.Amount,
            $"CK {t.Code}: {t.BeneficiaryName} · {t.Description}",
            t.Code, null, t.CounterAccountId, t.PeriodId, $"Từ đề nghị {t.Code}"), ct);
        voucher = await PostVoucherAsync(tenantId, userId, voucher.Id, ct);

        var tracked = await RequireVoucher(tenantId, voucher.Id, ct);
        tracked.TransferRequestId = t.Id;
        t.Status = "Executed";
        t.ExecutedVoucherId = voucher.Id;
        t.ExecutedVoucherCode = voucher.Code;
        t.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapTransfersAsync(tenantId, [t], ct))[0];
    }

    public async Task<FinBankTransferRequestDto> VoidTransferAsync(
        Guid tenantId, Guid userId, Guid id, string? note = null, CancellationToken ct = default)
    {
        var t = await RequireTransfer(tenantId, id, ct);
        if (t.Status is "Executed") throw new AppException("Đã thực hiện — không hủy đề nghị.");
        if (t.Status is "Void") throw new AppException("Đề nghị đã hủy.");
        t.Status = "Void";
        if (!string.IsNullOrWhiteSpace(note)) t.Note = note.Trim();
        t.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapTransfersAsync(tenantId, [t], ct))[0];
    }

    public async Task<IReadOnlyList<FinBankStatementLineDto>> ListStatementsAsync(
        Guid tenantId, Guid? bankAccountId = null, string? status = null, CancellationToken ct = default)
    {
        var q = _db.FinBankStatementLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (bankAccountId is Guid bid) q = q.Where(x => x.BankAccountId == bid);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status.Trim());
        var list = await q.OrderByDescending(x => x.StmtDate).Take(300).ToListAsync(ct);
        return await MapStatementsAsync(tenantId, list, ct);
    }

    public async Task<FinBankStatementLineDto> UpsertStatementAsync(
        Guid tenantId, Guid userId, FinBankStatementUpsertRequest req, CancellationToken ct = default)
    {
        _ = await RequireBank(tenantId, req.BankAccountId, ct, track: false);
        var dir = (req.Direction ?? "").Trim();
        if (dir is not ("Credit" or "Debit")) throw new AppException("Direction: Credit | Debit.");
        if (req.Amount <= 0) throw new AppException("Số tiền > 0.");
        var desc = Req(req.Description, 500, "Diễn giải sao kê");

        FinBankStatementLine entity;
        if (req.Id is Guid id)
        {
            entity = await RequireStatement(tenantId, id, ct);
            if (entity.Status != "Unmatched") throw new AppException("Chỉ sửa dòng Unmatched.");
        }
        else
        {
            entity = new FinBankStatementLine { TenantId = tenantId, CreatedBy = userId };
            _db.FinBankStatementLines.Add(entity);
        }

        entity.BankAccountId = req.BankAccountId;
        entity.StmtDate = req.StmtDate == default ? DateTimeOffset.UtcNow : req.StmtDate;
        entity.Description = desc;
        entity.BankRef = NullIfEmpty(req.BankRef);
        entity.Direction = dir;
        entity.Amount = decimal.Round(req.Amount, 2);
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapStatementsAsync(tenantId, [entity], ct))[0];
    }

    public async Task<FinBankStatementLineDto> MatchStatementAsync(
        Guid tenantId, Guid userId, Guid lineId, Guid voucherId, CancellationToken ct = default)
    {
        var line = await RequireStatement(tenantId, lineId, ct);
        if (line.Status != "Unmatched") throw new AppException("Dòng không Unmatched.");
        var v = await RequireVoucher(tenantId, voucherId, ct);
        if (v.Status != "Posted") throw new AppException("Chỉ khớp giấy báo Posted.");
        if (v.BankAccountId != line.BankAccountId) throw new AppException("Khác TKNH.");
        if (v.VoucherType != line.Direction) throw new AppException("Khác chiều Credit/Debit.");
        if (Math.Abs(v.Amount - line.Amount) > 0.01m) throw new AppException("Số tiền không khớp.");
        if (await _db.FinBankStatementLines.AnyAsync(
                x => x.TenantId == tenantId && x.MatchedVoucherId == voucherId && !x.IsDeleted && x.Status == "Matched", ct))
            throw new AppException("Giấy báo đã khớp dòng khác.");

        line.Status = "Matched";
        line.MatchedVoucherId = v.Id;
        line.MatchedVoucherCode = v.Code;
        line.MatchedAt = DateTimeOffset.UtcNow;
        line.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapStatementsAsync(tenantId, [line], ct))[0];
    }

    public async Task<FinBankStatementLineDto> UnmatchStatementAsync(
        Guid tenantId, Guid userId, Guid lineId, CancellationToken ct = default)
    {
        var line = await RequireStatement(tenantId, lineId, ct);
        if (line.Status != "Matched") throw new AppException("Dòng không Matched.");
        line.Status = "Unmatched";
        line.MatchedVoucherId = null;
        line.MatchedVoucherCode = null;
        line.MatchedAt = null;
        line.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapStatementsAsync(tenantId, [line], ct))[0];
    }

    public async Task<FinBankStatementLineDto> IgnoreStatementAsync(
        Guid tenantId, Guid userId, Guid lineId, CancellationToken ct = default)
    {
        var line = await RequireStatement(tenantId, lineId, ct);
        if (line.Status == "Matched") throw new AppException("Bỏ khớp trước khi bỏ qua.");
        line.Status = "Ignored";
        line.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapStatementsAsync(tenantId, [line], ct))[0];
    }

    public async Task<FinBankBookDto> GetBankBookAsync(
        Guid tenantId, Guid bankAccountId, DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken ct = default)
    {
        var bank = await RequireBank(tenantId, bankAccountId, ct, track: false);
        var q = _db.FinBankVouchers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.BankAccountId == bankAccountId && !x.IsDeleted && x.Status == "Posted");
        if (from is DateTimeOffset f) q = q.Where(x => x.DocDate >= f);
        if (to is DateTimeOffset t) q = q.Where(x => x.DocDate <= t);
        var vouchers = await q.OrderBy(x => x.DocDate).ThenBy(x => x.Code).ToListAsync(ct);

        var bal = bank.OpeningBalance;
        if (from is DateTimeOffset fromDt)
        {
            var prior = await _db.FinBankVouchers.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.BankAccountId == bankAccountId && !x.IsDeleted
                            && x.Status == "Posted" && x.DocDate < fromDt)
                .Select(x => new { x.VoucherType, x.Amount }).ToListAsync(ct);
            bal += prior.Where(x => x.VoucherType == "Credit").Sum(x => x.Amount)
                   - prior.Where(x => x.VoucherType == "Debit").Sum(x => x.Amount);
        }

        var opening = bal;
        var rows = new List<FinBankBookRowDto>();
        decimal totalC = 0, totalD = 0;
        foreach (var v in vouchers)
        {
            var c = v.VoucherType == "Credit" ? v.Amount : 0;
            var d = v.VoucherType == "Debit" ? v.Amount : 0;
            bal += c - d;
            totalC += c;
            totalD += d;
            rows.Add(new FinBankBookRowDto(
                v.DocDate, v.Code, v.VoucherType, v.Description, v.BankRef, c, d, bal));
        }

        return new FinBankBookDto(
            bank.Id, bank.Code, bank.Name, opening, totalC, totalD, bal, rows);
    }

    private async Task<decimal> CalcBookBalance(Guid tenantId, FinBankAccount bank, CancellationToken ct)
    {
        var posted = await _db.FinBankVouchers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.BankAccountId == bank.Id && !x.IsDeleted && x.Status == "Posted")
            .Select(x => new { x.VoucherType, x.Amount }).ToListAsync(ct);
        return bank.OpeningBalance
               + posted.Where(x => x.VoucherType == "Credit").Sum(x => x.Amount)
               - posted.Where(x => x.VoucherType == "Debit").Sum(x => x.Amount);
    }

    private async Task<IReadOnlyList<FinBankAccountDto>> MapAccountsAsync(
        Guid tenantId, List<FinBankAccount> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<FinBankAccountDto>();
        var ids = list.Select(x => x.Id).ToList();
        var aids = list.Select(x => x.GlAccountId).Distinct().ToList();
        var accs = await _db.FinAccounts.AsNoTracking()
            .Where(x => aids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var posted = await _db.FinBankVouchers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.BankAccountId) && !x.IsDeleted && x.Status == "Posted")
            .GroupBy(x => x.BankAccountId)
            .Select(g => new
            {
                g.Key,
                C = g.Where(x => x.VoucherType == "Credit").Sum(x => x.Amount),
                D = g.Where(x => x.VoucherType == "Debit").Sum(x => x.Amount)
            }).ToDictionaryAsync(x => x.Key, ct);

        return list.Select(b =>
        {
            accs.TryGetValue(b.GlAccountId, out var a);
            posted.TryGetValue(b.Id, out var p);
            var c = p?.C ?? 0;
            var d = p?.D ?? 0;
            return new FinBankAccountDto(
                b.Id, b.Code, b.Name, b.BankName, b.AccountNumber, b.BranchName,
                b.GlAccountId, a?.Code, a?.Name, b.OpeningBalance, b.Status, b.Note,
                c, d, b.OpeningBalance + c - d);
        }).ToList();
    }

    private async Task<IReadOnlyList<FinBankVoucherDto>> MapVouchersAsync(
        Guid tenantId, List<FinBankVoucher> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<FinBankVoucherDto>();
        var bids = list.Select(x => x.BankAccountId).Distinct().ToList();
        var aids = list.Where(x => x.CounterAccountId.HasValue).Select(x => x.CounterAccountId!.Value).Distinct().ToList();
        var pids = list.Where(x => x.PeriodId.HasValue).Select(x => x.PeriodId!.Value).Distinct().ToList();
        var banks = await _db.FinBankAccounts.AsNoTracking().Where(x => bids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
        var accs = aids.Count == 0 ? new Dictionary<Guid, FinAccount>()
            : await _db.FinAccounts.AsNoTracking().Where(x => aids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var periods = pids.Count == 0 ? new Dictionary<Guid, FinPeriod>()
            : await _db.FinPeriods.AsNoTracking().Where(x => pids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);

        return list.Select(v =>
        {
            banks.TryGetValue(v.BankAccountId, out var b);
            FinAccount? ca = null;
            if (v.CounterAccountId is Guid cid) accs.TryGetValue(cid, out ca);
            FinPeriod? pe = null;
            if (v.PeriodId is Guid pid) periods.TryGetValue(pid, out pe);
            return new FinBankVoucherDto(
                v.Id, v.Code, v.BankAccountId, b?.Code, b?.Name, v.VoucherType, v.DocDate, v.Amount, v.Description,
                v.BankRef, v.PartnerCode, v.CounterAccountId, ca?.Code, v.PeriodId, pe?.Code, v.Status,
                v.FinJournalId, v.FinJournalCode, v.PostedAt, v.TransferRequestId, v.Note);
        }).ToList();
    }

    private async Task<IReadOnlyList<FinBankTransferRequestDto>> MapTransfersAsync(
        Guid tenantId, List<FinBankTransferRequest> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<FinBankTransferRequestDto>();
        var bids = list.Select(x => x.FromBankAccountId).Distinct().ToList();
        var aids = list.Where(x => x.CounterAccountId.HasValue).Select(x => x.CounterAccountId!.Value).Distinct().ToList();
        var pids = list.Where(x => x.PeriodId.HasValue).Select(x => x.PeriodId!.Value).Distinct().ToList();
        var banks = await _db.FinBankAccounts.AsNoTracking().Where(x => bids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
        var accs = aids.Count == 0 ? new Dictionary<Guid, FinAccount>()
            : await _db.FinAccounts.AsNoTracking().Where(x => aids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var periods = pids.Count == 0 ? new Dictionary<Guid, FinPeriod>()
            : await _db.FinPeriods.AsNoTracking().Where(x => pids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);

        return list.Select(t =>
        {
            banks.TryGetValue(t.FromBankAccountId, out var b);
            FinAccount? ca = null;
            if (t.CounterAccountId is Guid cid) accs.TryGetValue(cid, out ca);
            FinPeriod? pe = null;
            if (t.PeriodId is Guid pid) periods.TryGetValue(pid, out pe);
            return new FinBankTransferRequestDto(
                t.Id, t.Code, t.FromBankAccountId, b?.Code,
                t.BeneficiaryName, t.BeneficiaryAccount, t.BeneficiaryBank,
                t.Amount, t.Description, t.RequestDate,
                t.CounterAccountId, ca?.Code, t.PeriodId, pe?.Code, t.Status,
                t.ExecutedVoucherId, t.ExecutedVoucherCode, t.ApprovedAt, t.Note);
        }).ToList();
    }

    private async Task<IReadOnlyList<FinBankStatementLineDto>> MapStatementsAsync(
        Guid tenantId, List<FinBankStatementLine> list, CancellationToken ct)
    {
        if (list.Count == 0) return Array.Empty<FinBankStatementLineDto>();
        var bids = list.Select(x => x.BankAccountId).Distinct().ToList();
        var banks = await _db.FinBankAccounts.AsNoTracking().Where(x => bids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
        return list.Select(s =>
        {
            banks.TryGetValue(s.BankAccountId, out var b);
            return new FinBankStatementLineDto(
                s.Id, s.BankAccountId, b?.Code, s.StmtDate, s.Description, s.BankRef,
                s.Direction, s.Amount, s.Status, s.MatchedVoucherId, s.MatchedVoucherCode, s.MatchedAt, s.Note);
        }).ToList();
    }

    private async Task<FinBankAccount> RequireBank(Guid tenantId, Guid id, CancellationToken ct, bool track = true)
    {
        var q = track ? _db.FinBankAccounts.AsQueryable() : _db.FinBankAccounts.AsNoTracking();
        return await q.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy TKNH.");
    }

    private async Task<FinBankVoucher> RequireVoucher(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FinBankVouchers.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy giấy báo NH.");

    private async Task<FinBankTransferRequest> RequireTransfer(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FinBankTransferRequests.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy đề nghị CK.");

    private async Task<FinBankStatementLine> RequireStatement(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FinBankStatementLines.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy dòng sao kê.");

    private async Task<FinAccount> RequireAccount(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FinAccounts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy tài khoản.");

    private async Task<FinPeriod> RequirePeriod(Guid tenantId, Guid id, CancellationToken ct) =>
        await _db.FinPeriods.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
        ?? throw new AppException("Không tìm thấy kỳ KT.");

    private async Task<string> NextCodeAsync<T>(DbSet<T> set, Guid tenantId, string prefix, CancellationToken ct)
        where T : class
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var stem = $"{prefix}-{today}-";
        // Use raw query via entity types we know have Code
        if (typeof(T) == typeof(FinBankVoucher))
        {
            var last = await _db.FinBankVouchers.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Code.StartsWith(stem))
                .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct);
            return NextFrom(stem, last);
        }
        if (typeof(T) == typeof(FinBankTransferRequest))
        {
            var last = await _db.FinBankTransferRequests.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Code.StartsWith(stem))
                .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct);
            return NextFrom(stem, last);
        }
        _ = set;
        return NextFrom(stem, null);
    }

    private static string NextFrom(string stem, string? last)
    {
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
