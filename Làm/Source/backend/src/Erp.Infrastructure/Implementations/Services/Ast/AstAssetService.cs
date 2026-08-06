using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Ast;
using Erp.Application.Interfaces.Services.Ast;
using Erp.Domain.Base;
using Erp.Domain.Entities.Ast;
using Erp.Domain.Entities.Fin;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Ast;

public sealed class AstAssetService : IAstAssetService
{
    private static readonly HashSet<string> MethodTypes =
        new(StringComparer.OrdinalIgnoreCase) { "StraightLine", "DecliningBalance" };
    private static readonly HashSet<string> AssetStatuses =
        new(StringComparer.OrdinalIgnoreCase) { "Draft", "Active", "Disposed" };

    private readonly AppDbContext _db;
    public AstAssetService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<AstAssetGroupDto>> ListGroupsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.AstAssetGroups.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).OrderBy(x => x.Code).ToListAsync(ct);
        var counts = await _db.AstAssets.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.GroupId != null)
            .GroupBy(x => x.GroupId!.Value)
            .Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C, ct);
        return list.Select(g => new AstAssetGroupDto(
            g.Id, g.Code, g.Name, g.DefaultUsefulLifeMonths, g.DefaultDepreciationRate,
            g.Status, g.Note, counts.GetValueOrDefault(g.Id))).ToList();
    }

    public async Task<AstAssetGroupDto> UpsertGroupAsync(
        Guid tenantId, Guid userId, AstAssetGroupUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên nhóm TS");
        AstAssetGroup entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.AstAssetGroups, tenantId, id, "nhóm TS", ct);
        else
        {
            await EnsureCodeAsync(_db.AstAssetGroups, tenantId, code, ct);
            entity = new AstAssetGroup { TenantId = tenantId, CreatedBy = userId };
            _db.AstAssetGroups.Add(entity);
        }
        entity.Code = code; entity.Name = name;
        entity.DefaultUsefulLifeMonths = req.DefaultUsefulLifeMonths ?? entity.DefaultUsefulLifeMonths;
        if (entity.DefaultUsefulLifeMonths < 1) throw new AppException("Thời gian KH ≥ 1 tháng.");
        entity.DefaultDepreciationRate = req.DefaultDepreciationRate ?? entity.DefaultDepreciationRate;
        entity.Status = ActiveInactive(req.Status);
        entity.Note = NullIfEmpty(req.Note); entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        var count = await _db.AstAssets.CountAsync(
            x => x.TenantId == tenantId && x.GroupId == entity.Id && !x.IsDeleted, ct);
        return new AstAssetGroupDto(
            entity.Id, entity.Code, entity.Name, entity.DefaultUsefulLifeMonths,
            entity.DefaultDepreciationRate, entity.Status, entity.Note, count);
    }

    public async Task<IReadOnlyList<AstLocationDto>> ListLocationsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.AstLocations.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).OrderBy(x => x.Code).ToListAsync(ct);
        return list.Select(x => new AstLocationDto(x.Id, x.Code, x.Name, x.BranchName, x.Status, x.Note)).ToList();
    }

    public async Task<AstLocationDto> UpsertLocationAsync(
        Guid tenantId, Guid userId, AstLocationUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên vị trí");
        AstLocation entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.AstLocations, tenantId, id, "vị trí", ct);
        else
        {
            await EnsureCodeAsync(_db.AstLocations, tenantId, code, ct);
            entity = new AstLocation { TenantId = tenantId, CreatedBy = userId };
            _db.AstLocations.Add(entity);
        }
        entity.Code = code; entity.Name = name;
        entity.BranchName = NullIfEmpty(req.BranchName);
        entity.Status = ActiveInactive(req.Status);
        entity.Note = NullIfEmpty(req.Note); entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new AstLocationDto(entity.Id, entity.Code, entity.Name, entity.BranchName, entity.Status, entity.Note);
    }

    public async Task<IReadOnlyList<AstDepreciationMethodDto>> ListMethodsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.AstDepreciationMethods.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted).OrderBy(x => x.Code).ToListAsync(ct);
        return list.Select(x => new AstDepreciationMethodDto(
            x.Id, x.Code, x.Name, x.MethodType, x.DefaultUsefulLifeMonths,
            x.DefaultRatePercent, x.Status, x.Note)).ToList();
    }

    public async Task<AstDepreciationMethodDto> UpsertMethodAsync(
        Guid tenantId, Guid userId, AstDepreciationMethodUpsertRequest req, CancellationToken ct = default)
    {
        var code = NormCode(req.Code);
        var name = Req(req.Name, 200, "Tên PP KH");
        var type = (req.MethodType ?? "").Trim();
        if (!MethodTypes.Contains(type)) throw new AppException("PP KH: StraightLine | DecliningBalance.");
        AstDepreciationMethod entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.AstDepreciationMethods, tenantId, id, "PP KH", ct);
        else
        {
            await EnsureCodeAsync(_db.AstDepreciationMethods, tenantId, code, ct);
            entity = new AstDepreciationMethod { TenantId = tenantId, CreatedBy = userId };
            _db.AstDepreciationMethods.Add(entity);
        }
        entity.Code = code; entity.Name = name;
        entity.MethodType = MethodTypes.First(x => x.Equals(type, StringComparison.OrdinalIgnoreCase));
        entity.DefaultUsefulLifeMonths = req.DefaultUsefulLifeMonths ?? entity.DefaultUsefulLifeMonths;
        if (entity.DefaultUsefulLifeMonths < 1) throw new AppException("Thời gian KH ≥ 1 tháng.");
        entity.DefaultRatePercent = req.DefaultRatePercent ?? entity.DefaultRatePercent;
        entity.Status = ActiveInactive(req.Status);
        entity.Note = NullIfEmpty(req.Note); entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new AstDepreciationMethodDto(
            entity.Id, entity.Code, entity.Name, entity.MethodType,
            entity.DefaultUsefulLifeMonths, entity.DefaultRatePercent, entity.Status, entity.Note);
    }

    public async Task<IReadOnlyList<AstAssetDto>> ListAssetsAsync(
        Guid tenantId, string? q, CancellationToken ct = default)
    {
        var query = _db.AstAssets.AsNoTracking().Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(x => x.Code.Contains(term) || x.Name.Contains(term)
                || (x.PurchaseRef != null && x.PurchaseRef.Contains(term)));
        }
        var list = await query.OrderByDescending(x => x.CapitalizeDate).Take(300).ToListAsync(ct);
        return await MapAssetsAsync(tenantId, list, ct);
    }

    public async Task<AstAssetDto> UpsertAssetAsync(
        Guid tenantId, Guid userId, AstAssetUpsertRequest req, CancellationToken ct = default)
    {
        var name = Req(req.Name, 200, "Tên TS");
        if (req.OriginalCost < 0) throw new AppException("Nguyên giá ≥ 0.");
        if (req.GroupId is Guid gid)
            _ = await RequireAsync(_db.AstAssetGroups, tenantId, gid, "nhóm TS", ct);
        if (req.LocationId is Guid lid)
            _ = await RequireAsync(_db.AstLocations, tenantId, lid, "vị trí", ct);
        if (req.DepreciationMethodId is Guid mid)
            _ = await RequireAsync(_db.AstDepreciationMethods, tenantId, mid, "PP KH", ct);

        var status = string.IsNullOrWhiteSpace(req.Status) ? "Draft" : req.Status.Trim();
        if (!AssetStatuses.Contains(status)) throw new AppException("TT TS: Draft | Active | Disposed.");

        AstAsset entity;
        if (req.Id is Guid id)
            entity = await RequireAsync(_db.AstAssets, tenantId, id, "tài sản", ct);
        else
        {
            entity = new AstAsset
            {
                TenantId = tenantId,
                Code = string.IsNullOrWhiteSpace(req.Code) ? await NextAssetCodeAsync(tenantId, ct) : NormCode(req.Code),
                CreatedBy = userId
            };
            if (await _db.AstAssets.AnyAsync(x => x.TenantId == tenantId && x.Code == entity.Code && !x.IsDeleted, ct))
                throw new AppException("Mã TS đã tồn tại.");
            _db.AstAssets.Add(entity);
        }

        entity.Name = name;
        entity.GroupId = req.GroupId;
        entity.LocationId = req.LocationId;
        entity.DepreciationMethodId = req.DepreciationMethodId;
        if (req.AssignedEmployeeId is Guid empId)
        {
            var emp = await _db.Employees.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == empId && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy nhân viên.", 404);
            entity.AssignedEmployeeId = emp.Id;
            entity.AssignedEmployeeName = string.IsNullOrWhiteSpace(req.AssignedEmployeeName)
                ? emp.FullName : req.AssignedEmployeeName.Trim();
        }
        else if (req.AssignedEmployeeId is null && req.Id is null)
        {
            entity.AssignedEmployeeId = null;
            entity.AssignedEmployeeName = null;
        }
        entity.OriginalCost = req.OriginalCost;
        entity.CapitalizeDate = req.CapitalizeDate ?? entity.CapitalizeDate;
        entity.UsefulLifeMonths = req.UsefulLifeMonths ?? entity.UsefulLifeMonths;
        if (entity.UsefulLifeMonths < 1) throw new AppException("Thời gian KH ≥ 1 tháng.");
        entity.DepreciationRatePercent = req.DepreciationRatePercent
            ?? (entity.UsefulLifeMonths > 0 ? Math.Round(100m / entity.UsefulLifeMonths * 12, 4) : 0);
        entity.PurchaseRef = NullIfEmpty(req.PurchaseRef);
        entity.Note = NullIfEmpty(req.Note);
        entity.Status = AssetStatuses.First(x => x.Equals(status, StringComparison.OrdinalIgnoreCase));

        if (req.CapitalizeFromPurchase == true)
        {
            if (string.IsNullOrWhiteSpace(entity.PurchaseRef))
                throw new AppException("Ghi tăng từ mua sắm cần PurchaseRef.");
            entity.CapitalizeDate ??= DateTimeOffset.UtcNow;
            entity.Status = "Active";
        }

        if (entity.Status == "Active" && entity.CapitalizeDate is null)
            throw new AppException("TS Active cần ngày ghi tăng.");

        if (entity.BookValue == 0 && entity.AccumulatedDepreciation == 0)
            entity.BookValue = entity.OriginalCost;

        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapAssetsAsync(tenantId, [entity], ct))[0];
    }

    public async Task<IReadOnlyList<AstDepreciationRunDto>> ListRunsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.AstDepreciationRuns.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month).Take(60).ToListAsync(ct);
        return list.Select(MapRun).ToList();
    }

    public async Task<AstDepreciationRunDetailDto> GetRunDetailAsync(
        Guid tenantId, Guid runId, CancellationToken ct = default)
    {
        var run = await _db.AstDepreciationRuns.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == runId && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Không tìm thấy kỳ KH.", 404);
        var lines = await _db.AstDepreciationLines.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.RunId == runId && !x.IsDeleted)
            .OrderBy(x => x.LineNo).ToListAsync(ct);
        var assetIds = lines.Select(x => x.AssetId).Distinct().ToList();
        var assets = await _db.AstAssets.AsNoTracking()
            .Where(x => assetIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var lineDtos = lines.Select(l =>
        {
            assets.TryGetValue(l.AssetId, out var a);
            return new AstDepreciationLineDto(
                l.Id, l.RunId, l.AssetId, a?.Code, a?.Name,
                l.Amount, l.BookValueBefore, l.BookValueAfter, l.LineNo);
        }).ToList();
        return new AstDepreciationRunDetailDto(MapRun(run), lineDtos);
    }

    public async Task<AstDepreciationRunDto> CalculatePeriodAsync(
        Guid tenantId, Guid userId, AstDepreciationCalcRequest req, CancellationToken ct = default)
    {
        if (req.Month is < 1 or > 12) throw new AppException("Tháng 1–12.");
        if (await _db.AstDepreciationRuns.AnyAsync(
                x => x.TenantId == tenantId && x.Year == req.Year && x.Month == req.Month && !x.IsDeleted, ct))
            throw new AppException("Kỳ KH đã tính.");

        var periodStart = new DateTimeOffset(req.Year, req.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var periodEnd = periodStart.AddMonths(1).AddTicks(-1);

        var assets = await _db.AstAssets
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Active"
                        && x.CapitalizeDate != null && x.CapitalizeDate <= periodEnd
                        && x.BookValue > 0)
            .ToListAsync(ct);

        var methodIds = assets.Where(x => x.DepreciationMethodId.HasValue)
            .Select(x => x.DepreciationMethodId!.Value).Distinct().ToList();
        var methods = methodIds.Count == 0
            ? new Dictionary<Guid, AstDepreciationMethod>()
            : await _db.AstDepreciationMethods
                .Where(x => methodIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);

        var run = new AstDepreciationRun
        {
            TenantId = tenantId,
            Code = $"DEP-{req.Year}{req.Month:D2}",
            Year = req.Year, Month = req.Month,
            PeriodStart = periodStart, PeriodEnd = periodEnd,
            Status = "Posted", CreatedByUserId = userId, CreatedBy = userId,
            PostedAt = DateTimeOffset.UtcNow
        };
        _db.AstDepreciationRuns.Add(run);
        await _db.SaveChangesAsync(ct);

        var lineNo = 1;
        decimal total = 0;
        foreach (var a in assets)
        {
            var methodType = a.DepreciationMethodId is Guid mid && methods.TryGetValue(mid, out var m)
                ? m.MethodType : "StraightLine";
            var amount = CalcMonthly(a, methodType);
            if (amount <= 0) continue;
            if (amount > a.BookValue) amount = a.BookValue;

            var before = a.BookValue;
            a.AccumulatedDepreciation += amount;
            a.BookValue -= amount;
            a.UpdatedBy = userId;

            _db.AstDepreciationLines.Add(new AstDepreciationLine
            {
                TenantId = tenantId, RunId = run.Id, AssetId = a.Id,
                Amount = amount, BookValueBefore = before, BookValueAfter = a.BookValue,
                LineNo = lineNo++, CreatedBy = userId
            });
            total += amount;
        }

        run.TotalAmount = total;
        run.LineCount = lineNo - 1;
        run.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapRun(run);
    }

    public async Task<AstDepreciationRunDto> PushToFinStubAsync(
        Guid tenantId, Guid userId, Guid runId, AstPushFinRequest req, CancellationToken ct = default)
    {
        var run = await RequireAsync(_db.AstDepreciationRuns, tenantId, runId, "kỳ KH", ct);
        if (run.Status == "Pushed" && run.FinJournalId is not null)
            throw new AppException("Đã đẩy FIN.");
        if (run.LineCount == 0 || run.TotalAmount <= 0)
            throw new AppException("Kỳ KH trống — không đẩy FIN.");

        FinAccount? expense = null;
        FinAccount? accum = null;
        if (req.ExpenseAccountId is Guid eid)
            expense = await RequireAsync(_db.FinAccounts, tenantId, eid, "TK chi phí KH", ct);
        if (req.AccumAccountId is Guid aid)
            accum = await RequireAsync(_db.FinAccounts, tenantId, aid, "TK KH lũy kế", ct);

        // Stub: nếu chưa chọn TK thì chỉ đánh dấu Pushed (không tạo JE)
        if (expense is null || accum is null)
        {
            run.Status = "Pushed";
            run.UpdatedBy = userId;
            await _db.SaveChangesAsync(ct);
            return MapRun(run);
        }

        FinPeriod period;
        if (req.PeriodId is Guid pid)
            period = await RequireAsync(_db.FinPeriods, tenantId, pid, "kỳ KT", ct);
        else
        {
            period = await _db.FinPeriods
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Open"
                            && x.StartDate.Year == run.Year && x.StartDate.Month == run.Month)
                .OrderBy(x => x.StartDate).FirstOrDefaultAsync(ct)
                ?? throw new AppException("Không tìm thấy kỳ FIN Open khớp tháng KH.");
        }
        if (period.Status == "Locked") throw new AppException("Kỳ FIN đã khóa.");

        var jeCode = $"JE-AST-{run.Code}";
        if (await _db.FinJournals.AnyAsync(x => x.TenantId == tenantId && x.Code == jeCode && !x.IsDeleted, ct))
            throw new AppException("BT FIN đã tồn tại cho kỳ này.");

        var je = new FinJournal
        {
            TenantId = tenantId, Code = jeCode, PeriodId = period.Id,
            EntryDate = run.PeriodEnd, Description = $"Khấu hao TSCĐ {run.Month:D2}/{run.Year}",
            Status = "Posted", Source = "Auto", CreatedByUserId = userId, CreatedBy = userId,
            PostedAt = DateTimeOffset.UtcNow
        };
        _db.FinJournals.Add(je);
        await _db.SaveChangesAsync(ct);

        _db.FinJournalLines.Add(new FinJournalLine
        {
            TenantId = tenantId, JournalId = je.Id, AccountId = expense.Id,
            Debit = run.TotalAmount, Credit = 0, LineNo = 1, CreatedBy = userId,
            Note = "Chi phí KH (AST→FIN stub)"
        });
        _db.FinJournalLines.Add(new FinJournalLine
        {
            TenantId = tenantId, JournalId = je.Id, AccountId = accum.Id,
            Debit = 0, Credit = run.TotalAmount, LineNo = 2, CreatedBy = userId,
            Note = "KH lũy kế (AST→FIN stub)"
        });

        run.Status = "Pushed";
        run.FinJournalId = je.Id;
        run.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapRun(run);
    }

    private static decimal CalcMonthly(AstAsset a, string methodType)
    {
        if (methodType.Equals("DecliningBalance", StringComparison.OrdinalIgnoreCase))
        {
            var rate = a.DepreciationRatePercent > 0
                ? a.DepreciationRatePercent / 100m / 12m
                : (a.UsefulLifeMonths > 0 ? 1m / a.UsefulLifeMonths : 0);
            return Math.Round(a.BookValue * rate, 2);
        }
        // StraightLine
        if (a.UsefulLifeMonths <= 0) return 0;
        return Math.Round(a.OriginalCost / a.UsefulLifeMonths, 2);
    }

    private async Task<IReadOnlyList<AstAssetDto>> MapAssetsAsync(
        Guid tenantId, List<AstAsset> list, CancellationToken ct)
    {
        var gids = list.Where(x => x.GroupId.HasValue).Select(x => x.GroupId!.Value).Distinct().ToList();
        var lids = list.Where(x => x.LocationId.HasValue).Select(x => x.LocationId!.Value).Distinct().ToList();
        var mids = list.Where(x => x.DepreciationMethodId.HasValue).Select(x => x.DepreciationMethodId!.Value).Distinct().ToList();
        var groups = gids.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.AstAssetGroups.AsNoTracking().Where(x => gids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var locs = lids.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.AstLocations.AsNoTracking().Where(x => lids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var methods = mids.Count == 0 ? new Dictionary<Guid, string>()
            : await _db.AstDepreciationMethods.AsNoTracking().Where(x => mids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        return list.Select(a => new AstAssetDto(
            a.Id, a.Code, a.Name, a.GroupId,
            a.GroupId is Guid g ? groups.GetValueOrDefault(g) : null,
            a.LocationId, a.LocationId is Guid l ? locs.GetValueOrDefault(l) : null,
            a.DepreciationMethodId, a.DepreciationMethodId is Guid m ? methods.GetValueOrDefault(m) : null,
            a.AssignedEmployeeId, a.AssignedEmployeeName,
            a.OriginalCost, a.CapitalizeDate, a.UsefulLifeMonths, a.DepreciationRatePercent,
            a.AccumulatedDepreciation, a.BookValue, a.Status, a.DisposedAt, a.DisposalAmount,
            a.PurchaseRef, a.Note)).ToList();
    }

    private async Task<string> NextAssetCodeAsync(Guid tenantId, CancellationToken ct)
    {
        var prefix = $"FA-{DateTime.UtcNow:yyyy}-";
        var last = await _db.AstAssets.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Code.StartsWith(prefix) && !x.IsDeleted)
            .OrderByDescending(x => x.Code).Select(x => x.Code).FirstOrDefaultAsync(ct);
        var n = 1;
        if (last is not null && int.TryParse(last.AsSpan(prefix.Length), out var parsed)) n = parsed + 1;
        return $"{prefix}{n:D4}";
    }

    private static AstDepreciationRunDto MapRun(AstDepreciationRun r) =>
        new(r.Id, r.Code, r.Year, r.Month, r.PeriodStart, r.PeriodEnd,
            r.Status, r.TotalAmount, r.LineCount, r.FinJournalId, r.PostedAt);

    private static async Task<T> RequireAsync<T>(
        DbSet<T> set, Guid tenantId, Guid id, string label, CancellationToken ct)
        where T : TenantEntity
        => await set.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
           ?? throw new AppException($"Không tìm thấy {label}.", 404);

    private static async Task EnsureCodeAsync<T>(DbSet<T> set, Guid tenantId, string code, CancellationToken ct)
        where T : TenantEntity
    {
        if (await set.AnyAsync(x => x.TenantId == tenantId && !x.IsDeleted && EF.Property<string>(x, "Code") == code, ct))
            throw new AppException("Mã đã tồn tại.");
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
}
