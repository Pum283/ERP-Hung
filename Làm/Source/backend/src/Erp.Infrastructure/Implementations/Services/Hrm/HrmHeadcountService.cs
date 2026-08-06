using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Hrm;
using Erp.Application.Interfaces.Services.Hrm;
using Erp.Domain.Entities.Hrm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Hrm;

public sealed class HrmHeadcountService : IHrmHeadcountService
{
    private static readonly HashSet<string> Scopes = new(StringComparer.OrdinalIgnoreCase)
        { "OrgUnit", "Department", "Shift" };

    private readonly AppDbContext _db;

    public HrmHeadcountService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<HeadcountPlanDto>> ListAsync(Guid tenantId, CancellationToken ct = default)
    {
        var rows = await _db.HeadcountPlans.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
        return await MapManyAsync(rows, ct);
    }

    public async Task<HeadcountPlanDto> UpsertAsync(
        Guid tenantId, Guid userId, HeadcountPlanUpsertRequest req, CancellationToken ct = default)
    {
        var scope = (req.ScopeType ?? "").Trim();
        if (!Scopes.Contains(scope)) throw new AppException("ScopeType phải là OrgUnit|Department|Shift.");
        if (req.PlannedHeadcount < 0 || req.PlannedHeadcount > 100000)
            throw new AppException("Định biên không hợp lệ.");
        if (!await _db.OrgUnits.AnyAsync(x => x.Id == req.OrgUnitId && x.TenantId == tenantId && !x.IsDeleted, ct))
            throw new AppException("Đơn vị không hợp lệ.", 404);

        if (string.Equals(scope, "Department", StringComparison.OrdinalIgnoreCase))
        {
            if (req.DepartmentId is not Guid did)
                throw new AppException("Cần DepartmentId khi định biên theo bộ phận.");
            if (!await _db.Departments.AnyAsync(
                    x => x.Id == did && x.TenantId == tenantId && x.OrgUnitId == req.OrgUnitId && !x.IsDeleted, ct))
                throw new AppException("Bộ phận không hợp lệ.", 404);
        }

        string? shift = null;
        if (string.Equals(scope, "Shift", StringComparison.OrdinalIgnoreCase))
        {
            shift = (req.ShiftCode ?? "").Trim();
            if (shift.Length == 0) throw new AppException("Cần mã ca khi định biên theo ca.");
            if (shift.Length > 40) throw new AppException("Mã ca tối đa 40 ký tự.");
        }

        HeadcountPlan entity;
        if (req.Id is Guid id)
        {
            entity = await _db.HeadcountPlans.FirstOrDefaultAsync(
                x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Định biên không tồn tại.", 404);
            if (entity.Status is not ("Draft" or "Rejected"))
                throw new AppException("Chỉ sửa phiếu Draft/Rejected.");
        }
        else
        {
            entity = new HeadcountPlan
            {
                TenantId = tenantId,
                RequestedByUserId = userId,
                CreatedBy = userId,
                Status = "Draft"
            };
            _db.HeadcountPlans.Add(entity);
        }

        entity.ScopeType = scope;
        entity.OrgUnitId = req.OrgUnitId;
        entity.DepartmentId = string.Equals(scope, "Department", StringComparison.OrdinalIgnoreCase) ? req.DepartmentId : null;
        entity.ShiftCode = shift;
        entity.PlannedHeadcount = req.PlannedHeadcount;
        entity.EffectiveFrom = req.EffectiveFrom;
        entity.EffectiveTo = req.EffectiveTo;
        entity.Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim();
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        if (req.Submit)
            await SubmitInternalAsync(tenantId, userId, entity, ct);

        return (await MapManyAsync(new[] { entity }, ct))[0];
    }

    public async Task<HeadcountPlanDto> SubmitAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default)
    {
        var entity = await _db.HeadcountPlans.FirstOrDefaultAsync(
            x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Định biên không tồn tại.", 404);
        await SubmitInternalAsync(tenantId, userId, entity, ct);
        return (await MapManyAsync(new[] { entity }, ct))[0];
    }

    public async Task<HeadcountPlanDto> DecideAsync(
        Guid tenantId, Guid userId, Guid id, bool approve, CancellationToken ct = default)
    {
        var entity = await _db.HeadcountPlans.FirstOrDefaultAsync(
            x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
            ?? throw new AppException("Định biên không tồn tại.", 404);
        if (entity.Status != "Pending")
            throw new AppException("Chỉ duyệt phiếu đang Pending.");
        entity.Status = approve ? "Approved" : "Rejected";
        entity.DecidedByUserId = userId;
        entity.DecidedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return (await MapManyAsync(new[] { entity }, ct))[0];
    }

    public async Task<IReadOnlyList<HeadcountCompareRowDto>> CompareAsync(Guid tenantId, CancellationToken ct = default)
        => await BuildCompareAsync(tenantId, shortageOnly: false, ct);

    public async Task<IReadOnlyList<HeadcountCompareRowDto>> ShortagesAsync(Guid tenantId, CancellationToken ct = default)
        => await BuildCompareAsync(tenantId, shortageOnly: true, ct);

    private async Task SubmitInternalAsync(Guid tenantId, Guid userId, HeadcountPlan entity, CancellationToken ct)
    {
        if (entity.Status is not ("Draft" or "Rejected"))
            throw new AppException("Chỉ gửi duyệt phiếu Draft/Rejected.");
        entity.Status = "Pending";
        entity.DecidedByUserId = null;
        entity.DecidedAt = null;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
    }

    private async Task<IReadOnlyList<HeadcountCompareRowDto>> BuildCompareAsync(
        Guid tenantId, bool shortageOnly, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var plans = await _db.HeadcountPlans.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Status == "Approved"
                        && x.EffectiveFrom <= today
                        && (x.EffectiveTo == null || x.EffectiveTo >= today))
            .ToListAsync(ct);

        var emps = await _db.Employees.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted
                        && (x.Status == "Active" || x.Status == "Probation"))
            .Select(x => new { x.OrgUnitId, x.DepartmentId })
            .ToListAsync(ct);

        var orgNames = await _db.OrgUnits.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var deptNames = await _db.Departments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        var rows = new List<HeadcountCompareRowDto>();
        foreach (var p in plans)
        {
            int actual;
            if (string.Equals(p.ScopeType, "Department", StringComparison.OrdinalIgnoreCase) && p.DepartmentId is Guid did)
                actual = emps.Count(e => e.DepartmentId == did);
            else
                // OrgUnit + Shift: đếm theo đơn vị (ca chưa gắn NV → dùng cùng actual org)
                actual = emps.Count(e => e.OrgUnitId == p.OrgUnitId);

            var gap = p.PlannedHeadcount - actual;
            var shortage = gap > 0;
            if (shortageOnly && !shortage) continue;

            rows.Add(new HeadcountCompareRowDto(
                p.ScopeType, p.OrgUnitId, orgNames.GetValueOrDefault(p.OrgUnitId, "?"),
                p.DepartmentId,
                p.DepartmentId is Guid d ? deptNames.GetValueOrDefault(d) : null,
                p.ShiftCode, p.PlannedHeadcount, actual, gap, shortage));
        }

        return rows
            .OrderByDescending(x => x.Shortage)
            .ThenByDescending(x => x.Gap)
            .ThenBy(x => x.OrgUnitName)
            .ToList();
    }

    private async Task<IReadOnlyList<HeadcountPlanDto>> MapManyAsync(IReadOnlyList<HeadcountPlan> rows, CancellationToken ct)
    {
        if (rows.Count == 0) return Array.Empty<HeadcountPlanDto>();
        var ouIds = rows.Select(r => r.OrgUnitId).Distinct().ToList();
        var deptIds = rows.Where(r => r.DepartmentId is not null).Select(r => r.DepartmentId!.Value).Distinct().ToList();
        var userIds = rows.Select(r => r.RequestedByUserId)
            .Concat(rows.Where(r => r.DecidedByUserId is not null).Select(r => r.DecidedByUserId!.Value))
            .Distinct().ToList();

        var orgs = await _db.OrgUnits.AsNoTracking().Where(x => ouIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var depts = deptIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.Departments.AsNoTracking().Where(x => deptIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var users = await _db.Users.AsNoTracking().Where(x => userIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.Username, ct);

        return rows.Select(r => new HeadcountPlanDto(
            r.Id, r.ScopeType, r.OrgUnitId, orgs.GetValueOrDefault(r.OrgUnitId, "?"),
            r.DepartmentId, r.DepartmentId is Guid d ? depts.GetValueOrDefault(d) : null,
            r.ShiftCode, r.PlannedHeadcount, r.Status, r.EffectiveFrom, r.EffectiveTo, r.Note,
            r.RequestedByUserId, users.GetValueOrDefault(r.RequestedByUserId, "?"),
            r.DecidedByUserId,
            r.DecidedByUserId is Guid du ? users.GetValueOrDefault(du) : null,
            r.DecidedAt, r.CreatedAt
        )).ToList();
    }
}
