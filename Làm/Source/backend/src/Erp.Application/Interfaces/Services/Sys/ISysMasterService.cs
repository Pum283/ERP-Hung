using Erp.Application.DTOs.Sys;
using Erp.Domain.Enums.Sys;

namespace Erp.Application.Interfaces.Services.Sys;

public interface ISysMasterService
{
    Task<IReadOnlyList<OrgUnitDto>> ListOrgUnitsAsync(Guid tenantId, CancellationToken ct = default);
    Task<OrgUnitDto> UpsertOrgUnitAsync(Guid tenantId, Guid? userId, OrgUnitUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<DepartmentDto>> ListDepartmentsAsync(Guid tenantId, CancellationToken ct = default);
    Task<DepartmentDto> UpsertDepartmentAsync(Guid tenantId, Guid? userId, DepartmentUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<JobLevelDto>> ListJobLevelsAsync(Guid tenantId, CancellationToken ct = default);
    Task<JobLevelDto> UpsertJobLevelAsync(Guid tenantId, Guid? userId, JobLevelUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<RoleDto>> ListRolesAsync(Guid tenantId, CancellationToken ct = default);
    Task<RoleDto> UpsertRoleAsync(Guid tenantId, Guid? userId, RoleUpsertRequest req, CancellationToken ct = default);
    Task SetRolePermissionsAsync(Guid tenantId, Guid roleId, IReadOnlyList<Guid> permissionIds, CancellationToken ct = default);

    Task<IReadOnlyList<PermissionDto>> ListPermissionsAsync(bool includeInactive = false, CancellationToken ct = default);

    Task<IReadOnlyList<UserDto>> ListUsersAsync(Guid tenantId, Guid currentUserId, CancellationToken ct = default);
    Task<UserDto> UpsertUserAsync(Guid tenantId, Guid? actorId, UserUpsertRequest req, CancellationToken ct = default);
    Task SetUserRolesAsync(Guid tenantId, Guid userId, IReadOnlyList<Guid> roleIds, Guid? actorId, CancellationToken ct = default);
    Task SoftDeleteUserAsync(Guid tenantId, Guid userId, Guid actorId, CancellationToken ct = default);
    Task<ResetPasswordResultDto> AdminResetPasswordAsync(Guid tenantId, Guid userId, Guid actorId, CancellationToken ct = default);
    /// <summary>UC_SYS_019 — mời user: tạo TK + OTP + gửi Email/SMS qua channel stub.</summary>
    Task<InviteUserResultDto> InviteUserAsync(Guid tenantId, Guid actorId, InviteUserRequest req, CancellationToken ct = default);
    Task<RoleDto> CopyRoleAsync(Guid tenantId, Guid roleId, Guid? actorId, string newCode, string newName, CancellationToken ct = default);

    Task<IReadOnlyList<MenuItemDto>> GetMyMenuAsync(Guid tenantId, Guid userId, CancellationToken ct = default);
}
