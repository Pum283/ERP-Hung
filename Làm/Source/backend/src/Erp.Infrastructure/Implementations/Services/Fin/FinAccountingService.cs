using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Fin;
using Erp.Application.Interfaces.Services.Fin;
using Erp.Domain.Base;
using Erp.Domain.Entities.Fin;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Fin;

public sealed class FinAccountingService : IFinAccountingService
{
    private static readonly HashSet<string> AccountTypes =
        new(StringComparer.OrdinalIgnoreCase) { "Asset", "Liability", "Equity", "Revenue", "Expense" };

    private readonly AppDbContext _db;
    public FinAccountingService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<FinAccountGroupDto>> ListGroupsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FinAccountGroups.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).OrderBy(x => x.SortOrder).ThenBy(x => x.Code).ToListAsync(ct);
        var counts = await _db.FinAccounts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.GroupId != null)
            .GroupBy(x => x.GroupId!.Value)
            .Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);
        return list.Select(g => new FinAccountGroupDto(
            g.Id, g.Code, g.Name, g.SortOrder, g.IsActive, counts.GetValueOrDefault(g.Id))).ToList();
    }

    public async Task<FinAccountGroupDto> UpsertGroupAsync(
        Guid tenantId, Guid userId, FinAccountGroupUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên nhóm TK");
        FinAccountGroup entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.FinAccountGroups, tenantId, id, "nhóm TK", ct);
        else
        {
            await EnsureCodeAsync(_db.FinAccountGroups, tenantId, code, ct);
            entity = new FinAccountGroup { TenantId = tenantId, CreatedBy = userId };
            _db.FinAccountGroups.Add(entity);
        }
        entity.Code = code; entity.Name = name;
        entity.SortOrder = req.SortOrder ?? entity.SortOrder;
        entity.IsActive = req.IsActive ?? true;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        var count = await _db.FinAccounts.CountAsync(
            x => x.TenantId == tenantId && x.GroupId == entity.Id && !x.IsDeleted, ct);
        return new FinAccountGroupDto(entity.Id, entity.Code, entity.Name, entity.SortOrder, entity.IsActive, count);
    }

    public async Task<IReadOnlyList<FinAccountDto>> ListAccountsAsync(
        Guid tenantId, string? q, CancellationToken ct = default)
    {
        var query = _db.FinAccounts.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term));
        }
        var list = await query.OrderBy(x => x.Code).Take(500).ToListAsync(ct);
        return await MapAccountsAsync(tenantId, list, ct);
    }

    public async Task<FinAccountDto> UpsertAccountAsync(
        Guid tenantId, Guid userId, FinAccountUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên TK");
        var type = (req.AccountType ?? "").Trim();
        if (!AccountTypes.Contains(type)) throw new AppException("Loại TK: Asset|Liability|Equity|Revenue|Expense.");
        if (req.GroupId is Guid gid)
            _ = await RequireAsync(_db.FinAccountGroups, tenantId, gid, "nhóm TK", ct);

        FinAccount entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.FinAccounts, tenantId, id, "tài khoản", ct);
        else
        {
            await EnsureCodeAsync(_db.FinAccounts, tenantId, code, ct);
            entity = new FinAccount { TenantId = tenantId, CreatedBy = userId };
            _db.FinAccounts.Add(entity);
        }

        entity.Code = code; entity.Name = name; entity.GroupId = req.GroupId;
        entity.AccountType = AccountTypes.First(x => x.Equals(type, StringComparison.OrdinalIgnoreCase));
        entity.IsPostable = req.IsPostable ?? true;
        entity.Status = ActiveInactive(req.Status);
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapAccountsAsync(tenantId, [entity], ct))[0];
    }

    public async Task<IReadOnlyList<FinFiscalYearDto>> ListFiscalYearsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FinFiscalYears.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).OrderByDescending(x => x.Year).ToListAsync(ct);
        var ids = list.Select(x => x.Id).ToList();
        var counts = await _db.FinPeriods.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.FiscalYearId) && !x.IsDeleted)
            .GroupBy(x => x.FiscalYearId)
            .Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);
        return list.Select(y => new FinFiscalYearDto(
            y.Id, y.Code, y.Name, y.Year, y.StartDate, y.EndDate, y.IsActive, counts.GetValueOrDefault(y.Id))).ToList();
    }

    public async Task<FinFiscalYearDto> UpsertFiscalYearAsync(
        Guid tenantId, Guid userId, FinFiscalYearUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên năm TC");
        if (req.EndDate < req.StartDate) throw new AppException("Ngày kết thúc phải ≥ ngày bắt đầu.");

        FinFiscalYear entity;
        var isNew = req.Id is null;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.FinFiscalYears, tenantId, id, "năm TC", ct);
        else
        {
            await EnsureCodeAsync(_db.FinFiscalYears, tenantId, code, ct);
            entity = new FinFiscalYear { TenantId = tenantId, CreatedBy = userId };
            _db.FinFiscalYears.Add(entity);
        }

        entity.Code = code; entity.Name = name; entity.Year = req.Year;
        entity.StartDate = req.StartDate; entity.EndDate = req.EndDate;
        entity.IsActive = req.IsActive ?? true; entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        if (isNew && (req.GenerateMonths ?? true))
        {
            for (var m = 1; m <= 12; m++)
            {
                var start = new DateTimeOffset(req.Year, m, 1, 0, 0, 0, TimeSpan.Zero);
                var end = start.AddMonths(1).AddTicks(-1);
                _db.FinPeriods.Add(new FinPeriod
                {
                    TenantId = tenantId, FiscalYearId = entity.Id,
                    Code = $"{req.Year}-{m:D2}", Name = $"Tháng {m}/{req.Year}",
                    StartDate = start, EndDate = end, Status = "Open", CreatedBy = userId
                });
            }
            await _db.SaveChangesAsync(ct);
        }

        var count = await _db.FinPeriods.CountAsync(
            x => x.TenantId == tenantId && x.FiscalYearId == entity.Id && !x.IsDeleted, ct);
        return new FinFiscalYearDto(
            entity.Id, entity.Code, entity.Name, entity.Year, entity.StartDate, entity.EndDate, entity.IsActive, count);
    }

    public async Task<IReadOnlyList<FinPeriodDto>> ListPeriodsAsync(
        Guid tenantId, Guid? fiscalYearId, CancellationToken ct = default)
    {
        var q = _db.FinPeriods.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (fiscalYearId is Guid fy) q = q.Where(x => x.FiscalYearId == fy);
        var list = await q.OrderBy(x => x.StartDate).ToListAsync(ct);
        return list.Select(MapPeriod).ToList();
    }

    public async Task<FinPeriodDto> SetPeriodLockAsync(
        Guid tenantId, Guid userId, Guid periodId, FinPeriodLockRequest req, CancellationToken ct = default)
    {
        var p = await RequireAsync(_db.FinPeriods, tenantId, periodId, "kỳ KT", ct);
        if (req.Lock)
        {
            if (p.Status == "Locked") throw new AppException("Kỳ đã khóa.");
            p.Status = "Locked";
            p.LockedAt = DateTimeOffset.UtcNow;
            p.LockedBy = userId;
        }
        else
        {
            if (p.Status != "Locked") throw new AppException("Kỳ chưa khóa.");
            p.Status = "Open";
            p.LockedAt = null;
            p.LockedBy = null;
        }
        p.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapPeriod(p);
    }

    public async Task<IReadOnlyList<FinCostCenterDto>> ListCostCentersAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FinCostCenters.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).OrderBy(x => x.Code).ToListAsync(ct);
        return list.Select(x => new FinCostCenterDto(x.Id, x.Code, x.Name, x.Status, x.Note)).ToList();
    }

    public async Task<FinCostCenterDto> UpsertCostCenterAsync(
        Guid tenantId, Guid userId, FinCostCenterUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên TTCP");
        FinCostCenter entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.FinCostCenters, tenantId, id, "TTCP", ct);
        else
        {
            await EnsureCodeAsync(_db.FinCostCenters, tenantId, code, ct);
            entity = new FinCostCenter { TenantId = tenantId, CreatedBy = userId };
            _db.FinCostCenters.Add(entity);
        }
        entity.Code = code; entity.Name = name;
        entity.Status = ActiveInactive(req.Status);
        entity.Note = NullIfEmpty(req.Note); entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new FinCostCenterDto(entity.Id, entity.Code, entity.Name, entity.Status, entity.Note);
    }

    public async Task<IReadOnlyList<FinPaymentMethodDto>> ListPaymentMethodsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FinPaymentMethods.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).OrderBy(x => x.Code).ToListAsync(ct);
        return list.Select(x => new FinPaymentMethodDto(x.Id, x.Code, x.Name, x.Status)).ToList();
    }

    public async Task<FinPaymentMethodDto> UpsertPaymentMethodAsync(
        Guid tenantId, Guid userId, FinPaymentMethodUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên HTTT");
        FinPaymentMethod entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.FinPaymentMethods, tenantId, id, "HTTT", ct);
        else
        {
            await EnsureCodeAsync(_db.FinPaymentMethods, tenantId, code, ct);
            entity = new FinPaymentMethod { TenantId = tenantId, CreatedBy = userId };
            _db.FinPaymentMethods.Add(entity);
        }
        entity.Code = code; entity.Name = name;
        entity.Status = ActiveInactive(req.Status); entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new FinPaymentMethodDto(entity.Id, entity.Code, entity.Name, entity.Status);
    }

    public async Task<IReadOnlyList<FinTaxDto>> ListTaxesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.FinTaxes.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).OrderBy(x => x.Code).ToListAsync(ct);
        return list.Select(MapTax).ToList();
    }

    public async Task<FinTaxDto> UpsertTaxAsync(
        Guid tenantId, Guid userId, FinTaxUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên thuế");
        if (req.RatePercent < 0 || req.RatePercent > 100) throw new AppException("Thuế suất 0–100%.");
        var taxType = string.IsNullOrWhiteSpace(req.TaxType) ? "VatOutput" : req.TaxType.Trim();
        if (taxType is not ("VatOutput" or "VatInput" or "Other"))
            throw new AppException("TaxType: VatOutput | VatInput | Other.");
        if (req.EffectiveFrom is DateOnly f && req.EffectiveTo is DateOnly t && t < f)
            throw new AppException("EffectiveTo không trước EffectiveFrom.");

        FinTax entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.FinTaxes, tenantId, id, "thuế", ct);
        else
        {
            await EnsureCodeAsync(_db.FinTaxes, tenantId, code, ct);
            entity = new FinTax { TenantId = tenantId, CreatedBy = userId };
            _db.FinTaxes.Add(entity);
        }

        var isDefault = req.IsDefault ?? entity.IsDefault;
        if (isDefault)
        {
            var others = await _db.FinTaxes
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.TaxType == taxType && x.IsDefault).ToListAsync(ct);
            foreach (var o in others)
            {
                if (req.Id is Guid rid && o.Id == rid) continue;
                o.IsDefault = false;
                o.UpdatedBy = userId;
            }
        }

        entity.Code = code;
        entity.Name = name;
        entity.RatePercent = decimal.Round(req.RatePercent, 4);
        entity.TaxType = taxType;
        entity.IsDefault = isDefault;
        entity.EffectiveFrom = req.EffectiveFrom;
        entity.EffectiveTo = req.EffectiveTo;
        entity.Status = ActiveInactive(req.Status);
        entity.Note = NullIfEmpty(req.Note);
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapTax(entity);
    }

    private static FinTaxDto MapTax(FinTax x) =>
        new(x.Id, x.Code, x.Name, x.RatePercent, x.TaxType, x.IsDefault,
            x.EffectiveFrom, x.EffectiveTo, x.Status, x.Note);

    public async Task<IReadOnlyList<FinJournalDto>> ListJournalsAsync(
        Guid tenantId, string? q, CancellationToken ct = default)
    {
        var query = _db.FinJournals.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(x => x.Code.Contains(term) || x.Description.Contains(term));
        }
        var list = await query.OrderByDescending(x => x.EntryDate).Take(300).ToListAsync(ct);
        return await MapJournalsAsync(tenantId, list, ct);
    }

    public async Task<FinJournalDetailDto> GetJournalDetailAsync(
        Guid tenantId, Guid journalId, CancellationToken ct = default)
    {
        var j = await _db.FinJournals.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == journalId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy bút toán.", 404);
        var dto = (await MapJournalsAsync(tenantId, [j], ct))[0];
        var lines = await LoadLinesAsync(tenantId, journalId, ct);
        return new FinJournalDetailDto(dto, lines);
    }

    public Task<FinJournalDto> UpsertJournalAsync(
        Guid tenantId, Guid userId, FinJournalUpsertRequest req, CancellationToken ct = default)
        => UpsertJournalCoreAsync(tenantId, userId, req, forceSource: null, ct);

    public Task<FinJournalDto> CreateAutoJournalStubAsync(
        Guid tenantId, Guid userId, FinJournalUpsertRequest req, CancellationToken ct = default)
        => UpsertJournalCoreAsync(tenantId, userId, req with { Source = "Auto" }, forceSource: "Auto", ct);

    private async Task<FinJournalDto> UpsertJournalCoreAsync(
        Guid tenantId, Guid userId, FinJournalUpsertRequest req, string? forceSource, CancellationToken ct)
    {
        var period = await RequireAsync(_db.FinPeriods, tenantId, req.PeriodId, "kỳ KT", ct);
        if (period.Status == "Locked") throw new AppException("Kỳ đã khóa sổ — không ghi BT.");
        var desc = Req(req.Description, 500, "Diễn giải");
        var source = forceSource ?? (string.IsNullOrWhiteSpace(req.Source) ? "Manual" : req.Source.Trim());
        if (source is not ("Manual" or "Auto")) throw new AppException("Nguồn BT: Manual | Auto.");

        if (req.CostCenterId is Guid ccid)
            _ = await RequireAsync(_db.FinCostCenters, tenantId, ccid, "TTCP", ct);

        FinJournal entity;
        if (req.Id is Guid id)
        {
            entity = await RequireAsync(_db.FinJournals, tenantId, id, "bút toán", ct);
            if (entity.Status != "Draft") throw new AppException("Chỉ sửa BT Draft.");
        }
        else
        {
            entity = new FinJournal
            {
                TenantId = tenantId,
                Code = string.IsNullOrWhiteSpace(req.Code) ? await NextJeCodeAsync(tenantId, ct) : NormCode(req.Code),
                Status = "Draft", CreatedByUserId = userId, CreatedBy = userId
            };
            if (await _db.FinJournals.AnyAsync(x => x.TenantId == tenantId && x.Code == entity.Code && !x.IsDeleted, ct))
                throw new AppException("Mã BT đã tồn tại.");
            _db.FinJournals.Add(entity);
        }

        entity.PeriodId = req.PeriodId;
        entity.EntryDate = req.EntryDate;
        entity.Description = desc;
        entity.Source = source;
        entity.PartnerCode = NullIfEmpty(req.PartnerCode)?.ToUpperInvariant();
        entity.CostCenterId = req.CostCenterId;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        if (req.Lines is { Count: > 0 })
        {
            var old = await _db.FinJournalLines
                .Where(x => x.TenantId == tenantId && x.JournalId == entity.Id && !x.IsDeleted).ToListAsync(ct);
            foreach (var o in old)
            {
                o.IsDeleted = true; o.DeletedAt = DateTimeOffset.UtcNow; o.UpdatedBy = userId;
            }

            var lineNo = 1;
            decimal td = 0, tc = 0;
            foreach (var l in req.Lines)
            {
                if (l.Debit < 0 || l.Credit < 0) throw new AppException("Nợ/Có ≥ 0.");
                if (l.Debit > 0 && l.Credit > 0) throw new AppException("Một dòng chỉ Nợ hoặc Có.");
                if (l.Debit == 0 && l.Credit == 0) throw new AppException("Dòng BT phải có số tiền.");
                var acc = await RequireAsync(_db.FinAccounts, tenantId, l.AccountId, "tài khoản", ct);
                if (!acc.IsPostable || acc.Status != "Active")
                    throw new AppException($"TK {acc.Code} không được hạch toán.");

                _db.FinJournalLines.Add(new FinJournalLine
                {
                    TenantId = tenantId, JournalId = entity.Id, AccountId = l.AccountId,
                    Debit = l.Debit, Credit = l.Credit,
                    PartnerCode = NullIfEmpty(l.PartnerCode)?.ToUpperInvariant() ?? entity.PartnerCode,
                    CostCenterId = l.CostCenterId ?? entity.CostCenterId,
                    Note = NullIfEmpty(l.Note), LineNo = lineNo++, CreatedBy = userId
                });
                td += l.Debit; tc += l.Credit;
            }
            if (td != tc) throw new AppException($"BT không cân: Nợ {td:N2} ≠ Có {tc:N2}.");
            await _db.SaveChangesAsync(ct);
        }

        return (await MapJournalsAsync(tenantId, [entity], ct))[0];
    }

    public async Task<FinJournalDto> PostJournalAsync(
        Guid tenantId, Guid userId, Guid journalId, CancellationToken ct = default)
    {
        var j = await RequireAsync(_db.FinJournals, tenantId, journalId, "bút toán", ct);
        if (j.Status != "Draft") throw new AppException("Chỉ ghi sổ BT Draft.");
        var period = await RequireAsync(_db.FinPeriods, tenantId, j.PeriodId, "kỳ KT", ct);
        if (period.Status == "Locked") throw new AppException("Kỳ đã khóa sổ.");

        var lines = await _db.FinJournalLines
            .Where(x => x.TenantId == tenantId && x.JournalId == journalId && !x.IsDeleted).ToListAsync(ct);
        if (lines.Count < 2) throw new AppException("BT cần ≥ 2 dòng.");
        var td = lines.Sum(x => x.Debit);
        var tc = lines.Sum(x => x.Credit);
        if (td != tc || td == 0) throw new AppException("BT không cân hoặc trống.");

        j.Status = "Posted";
        j.PostedAt = DateTimeOffset.UtcNow;
        j.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapJournalsAsync(tenantId, [j], ct))[0];
    }

    public async Task<FinJournalDto> ReverseJournalAsync(
        Guid tenantId, Guid userId, Guid journalId, CancellationToken ct = default)
    {
        var src = await RequireAsync(_db.FinJournals, tenantId, journalId, "bút toán", ct);
        if (src.Status != "Posted") throw new AppException("Chỉ đảo BT đã ghi sổ.");
        if (src.ReversalId is not null) throw new AppException("BT đã được đảo.");
        var period = await RequireAsync(_db.FinPeriods, tenantId, src.PeriodId, "kỳ KT", ct);
        if (period.Status == "Locked") throw new AppException("Kỳ đã khóa sổ.");

        var lines = await _db.FinJournalLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.JournalId == journalId && !x.IsDeleted).ToListAsync(ct);

        var rev = new FinJournal
        {
            TenantId = tenantId,
            Code = await NextJeCodeAsync(tenantId, ct),
            PeriodId = src.PeriodId,
            EntryDate = DateTimeOffset.UtcNow,
            Description = $"Đảo {src.Code}: {src.Description}",
            Status = "Posted",
            Source = src.Source,
            ReversedFromId = src.Id,
            PartnerCode = src.PartnerCode,
            CostCenterId = src.CostCenterId,
            CreatedByUserId = userId,
            CreatedBy = userId,
            PostedAt = DateTimeOffset.UtcNow
        };
        _db.FinJournals.Add(rev);
        await _db.SaveChangesAsync(ct);

        var n = 1;
        foreach (var l in lines)
        {
            _db.FinJournalLines.Add(new FinJournalLine
            {
                TenantId = tenantId, JournalId = rev.Id, AccountId = l.AccountId,
                Debit = l.Credit, Credit = l.Debit,
                PartnerCode = l.PartnerCode, CostCenterId = l.CostCenterId,
                Note = l.Note, LineNo = n++, CreatedBy = userId
            });
        }

        src.Status = "Reversed";
        src.ReversalId = rev.Id;
        src.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapJournalsAsync(tenantId, [rev], ct))[0];
    }

    public async Task<IReadOnlyList<FinLedgerRowDto>> GetLedgerAsync(
        Guid tenantId, FinLedgerQuery query, CancellationToken ct = default)
    {
        var lines = await FilterPostedLines(tenantId, query).ToListAsync(ct);
        var groups = lines.GroupBy(x => x.AccountId).ToList();
        var accIds = groups.Select(g => g.Key).ToList();
        var accounts = await _db.FinAccounts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && accIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        return groups.Select(g =>
        {
            accounts.TryGetValue(g.Key, out var a);
            var d = g.Sum(x => x.Debit);
            var c = g.Sum(x => x.Credit);
            return new FinLedgerRowDto(g.Key, a?.Code ?? "?", a?.Name ?? "?", d, c, d - c);
        }).OrderBy(x => x.AccountCode).ToList();
    }

    public async Task<IReadOnlyList<FinDetailLedgerRowDto>> GetDetailLedgerAsync(
        Guid tenantId, FinLedgerQuery query, CancellationToken ct = default)
    {
        var posted = await (
            from l in FilterPostedLines(tenantId, query)
            join j in _db.FinJournals.AsNoTracking() on l.JournalId equals j.Id
            orderby j.EntryDate, j.Code, l.LineNo
            select new { l, j }
        ).Take(500).ToListAsync(ct);

        var accIds = posted.Select(x => x.l.AccountId).Distinct().ToList();
        var ccIds = posted.Where(x => x.l.CostCenterId.HasValue).Select(x => x.l.CostCenterId!.Value).Distinct().ToList();
        var accounts = await _db.FinAccounts.AsNoTracking()
            .Where(x => accIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Code, ct);
        var ccs = ccIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.FinCostCenters.AsNoTracking()
                .Where(x => ccIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        return posted.Select(x => new FinDetailLedgerRowDto(
            x.j.Id, x.j.Code, x.j.EntryDate, x.j.Description,
            x.l.AccountId, accounts.GetValueOrDefault(x.l.AccountId) ?? "?",
            x.l.Debit, x.l.Credit, x.l.PartnerCode, x.l.CostCenterId,
            x.l.CostCenterId is Guid c ? ccs.GetValueOrDefault(c) : null)).ToList();
    }

    private IQueryable<FinJournalLine> FilterPostedLines(Guid tenantId, FinLedgerQuery query)
    {
        var journalQ = _db.FinJournals.AsNoTracking()
            .Where(j => j.TenantId == tenantId && !j.IsDeleted && j.Status == "Posted");
        if (query.PeriodId is Guid pid)
            journalQ = journalQ.Where(j => j.PeriodId == pid);

        var lineQ = _db.FinJournalLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Where(x => journalQ.Select(j => j.Id).Contains(x.JournalId));

        if (query.AccountId is Guid aid) lineQ = lineQ.Where(x => x.AccountId == aid);
        if (!string.IsNullOrWhiteSpace(query.PartnerCode))
        {
            var p = query.PartnerCode.Trim().ToUpperInvariant();
            lineQ = lineQ.Where(x => x.PartnerCode == p);
        }
        if (query.CostCenterId is Guid cc) lineQ = lineQ.Where(x => x.CostCenterId == cc);
        return lineQ;
    }

    private async Task<IReadOnlyList<FinAccountDto>> MapAccountsAsync(
        Guid tenantId, List<FinAccount> list, CancellationToken ct)
    {
        var gids = list.Where(x => x.GroupId.HasValue).Select(x => x.GroupId!.Value).Distinct().ToList();
        var groups = gids.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.FinAccountGroups.AsNoTracking()
                .Where(x => x.TenantId == tenantId && gids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        return list.Select(a => new FinAccountDto(
            a.Id, a.Code, a.Name, a.GroupId,
            a.GroupId is Guid g ? groups.GetValueOrDefault(g) : null,
            a.AccountType, a.IsPostable, a.Status, a.Note)).ToList();
    }

    private async Task<IReadOnlyList<FinJournalDto>> MapJournalsAsync(
        Guid tenantId, List<FinJournal> list, CancellationToken ct)
    {
        var ids = list.Select(x => x.Id).ToList();
        var pids = list.Select(x => x.PeriodId).Distinct().ToList();
        var ccids = list.Where(x => x.CostCenterId.HasValue).Select(x => x.CostCenterId!.Value).Distinct().ToList();

        var periods = await _db.FinPeriods.AsNoTracking()
            .Where(x => pids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Code, ct);
        var ccs = ccids.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.FinCostCenters.AsNoTracking()
                .Where(x => ccids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var aggs = await _db.FinJournalLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && ids.Contains(x.JournalId) && !x.IsDeleted)
            .GroupBy(x => x.JournalId)
            .Select(g => new { g.Key, Debit = g.Sum(x => x.Debit), Credit = g.Sum(x => x.Credit), C = g.Count() })
            .ToDictionaryAsync(x => x.Key, ct);

        return list.Select(j =>
        {
            aggs.TryGetValue(j.Id, out var a);
            return new FinJournalDto(
                j.Id, j.Code, j.PeriodId, periods.GetValueOrDefault(j.PeriodId), j.EntryDate,
                j.Description, j.Status, j.Source, j.ReversedFromId, j.ReversalId,
                j.PartnerCode, j.CostCenterId,
                j.CostCenterId is Guid c ? ccs.GetValueOrDefault(c) : null,
                a?.Debit ?? 0, a?.Credit ?? 0, a?.C ?? 0, j.PostedAt);
        }).ToList();
    }

    private async Task<IReadOnlyList<FinJournalLineDto>> LoadLinesAsync(
        Guid tenantId, Guid journalId, CancellationToken ct)
    {
        var lines = await _db.FinJournalLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.JournalId == journalId && !x.IsDeleted)
            .OrderBy(x => x.LineNo).ToListAsync(ct);
        var accIds = lines.Select(x => x.AccountId).Distinct().ToList();
        var ccIds = lines.Where(x => x.CostCenterId.HasValue).Select(x => x.CostCenterId!.Value).Distinct().ToList();
        var accounts = await _db.FinAccounts.AsNoTracking()
            .Where(x => accIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var ccs = ccIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.FinCostCenters.AsNoTracking()
                .Where(x => ccIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        return lines.Select(l =>
        {
            accounts.TryGetValue(l.AccountId, out var a);
            return new FinJournalLineDto(
                l.Id, l.JournalId, l.AccountId, a?.Code, a?.Name,
                l.Debit, l.Credit, l.PartnerCode, l.CostCenterId,
                l.CostCenterId is Guid c ? ccs.GetValueOrDefault(c) : null,
                l.Note, l.LineNo);
        }).ToList();
    }

    private async Task<string> NextJeCodeAsync(Guid tenantId, CancellationToken ct)
    {
        var prefix = $"JE-{DateTime.UtcNow:yyyyMM}-";
        var last = await _db.FinJournals.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Code.StartsWith(prefix) && !x.IsDeleted)
            .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct);
        var n = 1;
        if (last is not null && int.TryParse(last.AsSpan(prefix.Length), out var parsed)) n = parsed + 1;
        return $"{prefix}{n:D4}";
    }

    private static FinPeriodDto MapPeriod(FinPeriod p) =>
        new(p.Id, p.FiscalYearId, p.Code, p.Name, p.StartDate, p.EndDate, p.Status, p.LockedAt);

    private static async Task<T> RequireAsync<T>(
        DbSet<T> set, Guid tenantId, Guid id, string label, CancellationToken ct)
        where T : TenantEntity
        => await set.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
           ?? throw new AppException($"Không tìm thấy {label}.", 404);

    private static async Task EnsureCodeAsync<T>(
        DbSet<T> set, Guid tenantId, string code, CancellationToken ct)
        where T : TenantEntity
    {
        // Code property via dynamic filter — use EF for known types via Exists pattern in callers
        // Generic constraint: check with reflection-free approach by casting common interface
        var exists = await set.AnyAsync(x => x.TenantId == tenantId && !x.IsDeleted
            && EF.Property<string>(x, "Code") == code, ct);
        if (exists) throw new AppException("Mã đã tồn tại.");
    }

    private static string NormCode(string? code)
    {
        var c = (code ?? "").Trim().ToUpperInvariant();
        if (c.Length is < 1 or > 40) throw new AppException("Mã 1–40 ký tự.");
        return c;
    }

    private static string Req(string? value, int max, string label)
    {
        var v = (value ?? "").Trim();
        if (v.Length is < 1 || v.Length > max) throw new AppException($"{label} 1–{max} ký tự.");
        return v;
    }

    private static string ActiveInactive(string? s)
    {
        var v = string.IsNullOrWhiteSpace(s) ? "Active" : s.Trim();
        if (v is not ("Active" or "Inactive")) throw new AppException("Trạng thái: Active | Inactive.");
        return v;
    }

    private static string? NullIfEmpty(string? s)
    {
        var v = s?.Trim();
        return string.IsNullOrEmpty(v) ? null : v;
    }

    public async Task<FinJournalDto> RunClosingTransferAsync(
        Guid tenantId, Guid userId, FinClosingTransferRequest req, CancellationToken ct = default)
    {
        var period = await RequireAsync(_db.FinPeriods, tenantId, req.PeriodId, "kỳ KT", ct);
        if (period.Status == "Locked") throw new AppException("Kỳ đã khóa sổ — không thể kết chuyển.");

        // Gather posted journal lines in this period
        var postedJournals = _db.FinJournals.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PeriodId == req.PeriodId && x.Status == "Posted" && !x.IsDeleted);

        var lines = await _db.FinJournalLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && postedJournals.Select(j => j.Id).Contains(x.JournalId))
            .ToListAsync(ct);

        var accIds = lines.Select(x => x.AccountId).Distinct().ToList();
        var accounts = await _db.FinAccounts.AsNoTracking()
            .Where(x => accIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);

        // Find Account 911 (Xác định KQKĐ) or 421 (Lợi nhuận)
        var tk911 = await _db.FinAccounts.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && !x.IsDeleted && (x.Code == "911" || x.Code.StartsWith("911")), ct);
        if (tk911 == null)
        {
            tk911 = new FinAccount
            {
                TenantId = tenantId, Code = "911", Name = "Xác định kết quả kinh doanh",
                AccountType = "Equity", IsPostable = true, Status = "Active", CreatedBy = userId
            };
            _db.FinAccounts.Add(tk911);
            await _db.SaveChangesAsync(ct);
        }

        var closingLines = new List<FinJournalLineUpsertRequest>();
        decimal totalRevenue = 0, totalExpense = 0;

        foreach (var group in lines.GroupBy(x => x.AccountId))
        {
            if (!accounts.TryGetValue(group.Key, out var acc)) continue;
            var netDebit = group.Sum(x => x.Debit) - group.Sum(x => x.Credit);
            if (netDebit == 0) continue;

            if (acc.AccountType == "Revenue" || acc.Code.StartsWith("5") || acc.Code.StartsWith("7"))
            {
                // Revenue: Transfer Credit balance to 911 (Debit Revenue Account, Credit 911)
                var rev = -netDebit;
                if (rev > 0)
                {
                    closingLines.Add(new FinJournalLineUpsertRequest(null, acc.Id, rev, 0, null, null, "Kết chuyển doanh thu sang 911"));
                    totalRevenue += rev;
                }
            }
            else if (acc.AccountType == "Expense" || acc.Code.StartsWith("6") || acc.Code.StartsWith("8"))
            {
                // Expense: Transfer Debit balance to 911 (Credit Expense Account, Debit 911)
                if (netDebit > 0)
                {
                    closingLines.Add(new FinJournalLineUpsertRequest(null, acc.Id, 0, netDebit, null, null, "Kết chuyển chi phí sang 911"));
                    totalExpense += netDebit;
                }
            }
        }

        if (totalRevenue > 0)
            closingLines.Add(new FinJournalLineUpsertRequest(null, tk911.Id, 0, totalRevenue, null, null, "Ghi nhận tổng doanh thu vào 911"));
        if (totalExpense > 0)
            closingLines.Add(new FinJournalLineUpsertRequest(null, tk911.Id, totalExpense, 0, null, null, "Ghi nhận tổng chi phí vào 911"));

        if (closingLines.Count == 0)
            throw new AppException("Không có số dư doanh thu/chi phí cần kết chuyển trong kỳ.");

        var upsertReq = new FinJournalUpsertRequest(
            null, null, req.PeriodId, DateTimeOffset.UtcNow,
            req.Note ?? $"Bút toán kết chuyển xác định KQKD kỳ {period.Code}",
            null, null, "Auto", closingLines);

        var je = await UpsertJournalCoreAsync(tenantId, userId, upsertReq, forceSource: "Auto", ct);
        return await PostJournalAsync(tenantId, userId, je.Id, ct);
    }

    public async Task<bool> CloseFiscalYearAsync(
        Guid tenantId, Guid userId, FinYearEndClosingRequest req, CancellationToken ct = default)
    {
        var fy = await RequireAsync(_db.FinFiscalYears, tenantId, req.FiscalYearId, "năm tài chính", ct);
        var periods = await _db.FinPeriods
            .Where(x => x.TenantId == tenantId && x.FiscalYearId == fy.Id && !x.IsDeleted).ToListAsync(ct);

        foreach (var p in periods)
        {
            p.Status = "Locked";
            p.LockedAt = DateTimeOffset.UtcNow;
            p.UpdatedBy = userId;
        }

        fy.IsActive = false;
        fy.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<FinArApReconciliationRowDto>> ReconcileArApAsync(
        Guid tenantId, string type, CancellationToken ct = default)
    {
        var isAr = type.Equals("AR", StringComparison.OrdinalIgnoreCase);
        var targetAccountPrefix = isAr ? "131" : "331";

        // Subledger totals
        Dictionary<string, decimal> subledgerBalances;
        if (isAr)
        {
            var invoices = await _db.FinArInvoices.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted)
                .GroupBy(x => x.CustomerId)
                .Select(g => new { Code = g.Key.ToString().Substring(0, 8), Bal = g.Sum(x => x.TotalAmount - x.ReceivedAmount) })
                .ToListAsync(ct);
            subledgerBalances = invoices.ToDictionary(x => x.Code, x => x.Bal);
        }
        else
        {
            var invoices = await _db.FinApInvoices.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted)
                .GroupBy(x => x.VendorId)
                .Select(g => new { Code = g.Key.ToString().Substring(0, 8), Bal = g.Sum(x => x.TotalAmount - x.PaidAmount) })
                .ToListAsync(ct);
            subledgerBalances = invoices.ToDictionary(x => x.Code, x => x.Bal);
        }

        // General Ledger lines on 131 / 331 by PartnerCode
        var postedJournals = _db.FinJournals.AsNoTracking()
            .Where(j => j.TenantId == tenantId && !j.IsDeleted && j.Status == "Posted");

        var glLines = await (
            from l in _db.FinJournalLines.AsNoTracking()
            join a in _db.FinAccounts.AsNoTracking() on l.AccountId equals a.Id
            where l.TenantId == tenantId && !l.IsDeleted && postedJournals.Select(j => j.Id).Contains(l.JournalId)
                  && a.Code.StartsWith(targetAccountPrefix) && l.PartnerCode != null
            group l by l.PartnerCode into g
            select new { PartnerCode = g.Key!, Bal = isAr ? g.Sum(x => x.Debit - x.Credit) : g.Sum(x => x.Credit - x.Debit) }
        ).ToListAsync(ct);

        var glBalances = glLines.ToDictionary(x => x.PartnerCode, x => x.Bal);
        var allPartners = subledgerBalances.Keys.Union(glBalances.Keys).Distinct().OrderBy(x => x).ToList();

        var result = new List<FinArApReconciliationRowDto>();
        foreach (var p in allPartners)
        {
            var sub = subledgerBalances.GetValueOrDefault(p);
            var gl = glBalances.GetValueOrDefault(p);
            var varAmount = sub - gl;
            result.Add(new FinArApReconciliationRowDto(p, sub, gl, varAmount, varAmount == 0));
        }

        return result;
    }

    public async Task<IReadOnlyList<FinTrialBalanceRowDto>> GetTrialBalanceAsync(
        Guid tenantId, Guid? periodId, CancellationToken ct = default)
    {
        var accounts = await _db.FinAccounts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.IsPostable)
            .OrderBy(x => x.Code).ToListAsync(ct);

        var postedJournals = _db.FinJournals.AsNoTracking()
            .Where(j => j.TenantId == tenantId && !j.IsDeleted && j.Status == "Posted");
        if (periodId is Guid pid)
            postedJournals = postedJournals.Where(j => j.PeriodId == pid);

        var lines = await _db.FinJournalLines.AsNoTracking()
            .Where(l => l.TenantId == tenantId && !l.IsDeleted && postedJournals.Select(j => j.Id).Contains(l.JournalId))
            .GroupBy(l => l.AccountId)
            .Select(g => new { AccountId = g.Key, Debit = g.Sum(x => x.Debit), Credit = g.Sum(x => x.Credit) })
            .ToDictionaryAsync(x => x.AccountId, ct);

        var result = new List<FinTrialBalanceRowDto>();
        foreach (var a in accounts)
        {
            var l = lines.GetValueOrDefault(a.Id);
            var pDebit = l?.Debit ?? 0;
            var pCredit = l?.Credit ?? 0;
            var cNet = pDebit - pCredit;
            var cDebit = cNet > 0 ? cNet : 0;
            var cCredit = cNet < 0 ? -cNet : 0;
            result.Add(new FinTrialBalanceRowDto(
                a.Id, a.Code, a.Name, a.AccountType, 0, 0, pDebit, pCredit, cDebit, cCredit));
        }
        return result;
    }

    public async Task<IReadOnlyList<FinBalanceSheetRowDto>> GetBalanceSheetAsync(
        Guid tenantId, Guid? periodId, CancellationToken ct = default)
    {
        var tb = await GetTrialBalanceAsync(tenantId, periodId, ct);
        var list = new List<FinBalanceSheetRowDto>();

        foreach (var r in tb)
        {
            string cat;
            if (r.AccountCode.StartsWith("1") || r.AccountCode.StartsWith("2") || r.AccountType == "Asset") cat = "Tài sản";
            else if (r.AccountCode.StartsWith("3") || r.AccountType == "Liability") cat = "Nợ phải trả";
            else if (r.AccountCode.StartsWith("4") || r.AccountType == "Equity") cat = "Vốn chủ sở hữu";
            else continue;

            var amt = r.AccountType == "Asset" || r.AccountCode.StartsWith("1") || r.AccountCode.StartsWith("2")
                ? (r.ClosingDebit - r.ClosingCredit)
                : (r.ClosingCredit - r.ClosingDebit);

            list.Add(new FinBalanceSheetRowDto(r.AccountCode, r.AccountName, cat, amt));
        }
        return list;
    }

    public async Task<IReadOnlyList<FinProfitLossRowDto>> GetProfitLossAsync(
        Guid tenantId, Guid? periodId, CancellationToken ct = default)
    {
        var tb = await GetTrialBalanceAsync(tenantId, periodId, ct);
        var result = new List<FinProfitLossRowDto>();

        var revTotal = tb.Where(x => x.AccountCode.StartsWith("5") || x.AccountType == "Revenue")
            .Sum(x => x.PeriodCredit - x.PeriodDebit);

        var expTotal = tb.Where(x => x.AccountCode.StartsWith("6") || x.AccountType == "Expense")
            .Sum(x => x.PeriodDebit - x.PeriodCredit);

        result.Add(new FinProfitLossRowDto("5xx", "Doanh thu bán hàng và dịch vụ", revTotal, 0));
        result.Add(new FinProfitLossRowDto("6xx", "Tổng chi phí hoạt động & giá vốn", expTotal, 0));
        result.Add(new FinProfitLossRowDto("NET", "Lợi nhuận ròng", revTotal - expTotal, 0));

        return result;
    }

    public async Task<IReadOnlyList<FinCashFlowRowDto>> GetCashFlowAsync(
        Guid tenantId, Guid? periodId, CancellationToken ct = default)
    {
        var cashFunds = await _db.FinCashFunds.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).SumAsync(x => x.OpeningBalance, ct);
        var bankAccounts = await _db.FinBankAccounts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).SumAsync(x => x.OpeningBalance, ct);

        return new List<FinCashFlowRowDto>
        {
            new("Operating", "Dòng tiền từ hoạt động kinh doanh", cashFunds + bankAccounts),
            new("Investing", "Dòng tiền từ hoạt động đầu tư", 0),
            new("Financing", "Dòng tiền từ hoạt động tài chính", 0)
        };
    }

    public async Task<FinDashboardSummaryDto> GetDashboardSummaryAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var pl = await GetProfitLossAsync(tenantId, null, ct);
        var rev = pl.FirstOrDefault(x => x.ItemCode == "5xx")?.CurrentPeriodAmount ?? 0;
        var exp = pl.FirstOrDefault(x => x.ItemCode == "6xx")?.CurrentPeriodAmount ?? 0;
        var net = pl.FirstOrDefault(x => x.ItemCode == "NET")?.CurrentPeriodAmount ?? 0;

        var cash = await _db.FinCashFunds.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).SumAsync(x => x.OpeningBalance, ct);
        var bank = await _db.FinBankAccounts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).SumAsync(x => x.OpeningBalance, ct);

        var ar = await _db.FinArInvoices.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status != "Paid")
            .SumAsync(x => x.TotalAmount - x.ReceivedAmount, ct);

        var ap = await _db.FinApInvoices.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status != "Paid")
            .SumAsync(x => x.TotalAmount - x.PaidAmount, ct);

        return new FinDashboardSummaryDto(rev, exp, net, cash + bank, ar, ap);
    }

}

