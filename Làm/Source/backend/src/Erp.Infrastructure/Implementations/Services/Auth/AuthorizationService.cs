using Erp.Application.Common.Exceptions;
using Erp.Application.Interfaces.Services.Auth;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Auth;

/// <summary>
/// RBAC Digi-style: User có 1..N Role; kiểm tra quyền = hợp (union) RolePermission —
/// có quyền từ bất kỳ role nào là đủ (ưu tiên quyền cao hơn = cộng dồn, BypassDataScope = full).
/// </summary>
public sealed class AuthorizationService : IAuthorizationService
{
    private readonly AppDbContext _db;

    public AuthorizationService(AppDbContext db) => _db = db;

    public async Task EnsurePermissionAsync(Guid userId, string permissionCode, CancellationToken ct = default)
    {
        if (!await HasPermissionAsync(userId, permissionCode, ct))
            throw new ForbiddenException($"Thiếu quyền `{permissionCode}`.");
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permissionCode, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var code = permissionCode.Trim().ToLowerInvariant();

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

        if (bypass) return true;

        return await (
            from ur in _db.UserRoles.AsNoTracking()
            join rp in _db.RolePermissions.AsNoTracking() on ur.RoleId equals rp.RoleId
            join p in _db.Permissions.AsNoTracking() on rp.PermissionId equals p.Id
            where ur.UserId == userId
                  && ur.IsActive && !ur.IsDeleted && ur.RevokedAt == null
                  && (ur.ValidFrom == null || ur.ValidFrom <= now)
                  && (ur.ValidTo == null || ur.ValidTo >= now)
                  && !rp.IsDeleted
                  && p.IsActive && !p.IsDeleted
                  && p.Code == code
            select p.Id
        ).AnyAsync(ct);
    }
}
