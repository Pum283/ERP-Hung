using Erp.Application.Interfaces.Services.Auth;
using Erp.Domain.Enums.Sys;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Auth;

public sealed class DataScopeService : IDataScopeService
{
    private readonly AppDbContext _db;

    public DataScopeService(AppDbContext db) => _db = db;

    public async Task<UserScopeContext> GetUserScopeContextAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, ct)
            ?? throw new InvalidOperationException("User không tồn tại.");

        var now = DateTimeOffset.UtcNow;
        var bypass = await (
            from ur in _db.UserRoles.AsNoTracking()
            join r in _db.Roles.AsNoTracking() on ur.RoleId equals r.Id
            where ur.UserId == userId
                  && ur.IsActive && !ur.IsDeleted && ur.RevokedAt == null
                  && (ur.ValidFrom == null || ur.ValidFrom <= now)
                  && (ur.ValidTo == null || ur.ValidTo >= now)
                  && r.IsActive && !r.IsDeleted && r.BypassDataScope
            select r.Id
        ).AnyAsync(ct);

        // Memberships: 1 primary + peers; JobLevel theo từng phòng ban.
        var memberships = await _db.UserDepartments.AsNoTracking()
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .Select(x => new { x.DepartmentId, x.JobLevelId, x.IsPrimary })
            .ToListAsync(ct);

        var primaryDeptId = memberships.FirstOrDefault(x => x.IsPrimary)?.DepartmentId
                            ?? memberships.FirstOrDefault()?.DepartmentId
                            ?? user.DepartmentId;
        var primaryJlId = memberships.FirstOrDefault(x => x.IsPrimary)?.JobLevelId
                          ?? memberships.FirstOrDefault()?.JobLevelId
                          ?? user.JobLevelId;

        if (bypass)
        {
            var allSalesPoints = await _db.SalesPoints.AsNoTracking()
                .Where(sp => sp.TenantId == user.TenantId && !sp.IsDeleted)
                .Select(sp => sp.Id)
                .ToListAsync(ct);
            return new UserScopeContext(ScopeType.All, true, userId, primaryDeptId, Array.Empty<Guid>(), allSalesPoints);
        }

        var scope = ScopeType.Own;
        if (primaryJlId is Guid jlId)
        {
            var jl = await _db.JobLevels.AsNoTracking().FirstOrDefaultAsync(x => x.Id == jlId && !x.IsDeleted, ct);
            if (jl is not null) scope = jl.DefaultScopeType;
        }

        var deptIds = new List<Guid>();
        if (scope == ScopeType.Department)
        {
            var roots = memberships.Select(x => x.DepartmentId).Distinct().ToList();
            if (roots.Count == 0 && primaryDeptId is Guid only)
                roots.Add(only);

            if (roots.Count > 0)
            {
                var rootDepts = await _db.Departments.AsNoTracking()
                    .Where(d => d.TenantId == user.TenantId && !d.IsDeleted && roots.Contains(d.Id))
                    .Select(d => new { d.Id, d.Path })
                    .ToListAsync(ct);
                var paths = rootDepts.Select(d => d.Path).Where(p => !string.IsNullOrEmpty(p)).ToList();
                var all = await _db.Departments.AsNoTracking()
                    .Where(d => d.TenantId == user.TenantId && !d.IsDeleted && d.IsActive
                                && (roots.Contains(d.Id) || paths.Any(p => d.Path.StartsWith(p))))
                    .Select(d => d.Id)
                    .ToListAsync(ct);
                deptIds = all.Distinct().ToList();
            }
        }

        // UC_SYS_029: Resolve SalesPoint scope from UserDataScopes
        var salesPointScopes = await _db.UserDataScopes.AsNoTracking()
            .Where(x => x.UserId == userId && x.TenantId == user.TenantId && !x.IsDeleted
                        && (x.Dimension == "SalesPoint" || x.Dimension == "OrgUnit"))
            .Select(x => x.ScopeId)
            .Distinct()
            .ToListAsync(ct);

        return new UserScopeContext(scope, false, userId, primaryDeptId, deptIds, salesPointScopes);
    }
}
