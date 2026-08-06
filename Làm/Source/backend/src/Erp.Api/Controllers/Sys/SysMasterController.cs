using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Exceptions;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Sys;
using Erp.Application.Interfaces.Services.Auth;
using Erp.Application.Interfaces.Services.Sys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IAppAuthz = Erp.Application.Interfaces.Services.Auth.IAuthorizationService;

namespace Erp.Api.Controllers.Sys;

[ApiController]
[Authorize]
[Route("api/sys")]
public sealed class SysMasterController : ControllerBase
{
    private readonly ISysMasterService _svc;
    private readonly IAppAuthz _authz;

    public SysMasterController(ISysMasterService svc, IAppAuthz authz)
    {
        _svc = svc;
        _authz = authz;
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    private async Task EnsureAnyAsync(CancellationToken ct, params string[] codes)
    {
        foreach (var c in codes)
            if (await _authz.HasPermissionAsync(UserId, c, ct)) return;
        throw new ForbiddenException($"Thiếu quyền ({string.Join(" | ", codes)}).");
    }

    [HttpGet("org-units")]
    [AuthorizePermission("sys.user.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<OrgUnitDto>>>> OrgUnits(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<OrgUnitDto>>.Ok(await _svc.ListOrgUnitsAsync(TenantId, ct)));

    [HttpPost("org-units")]
    [AuthorizePermission("sys.user.manage")]
    public async Task<ActionResult<ApiResponse<OrgUnitDto>>> UpsertOrg([FromBody] OrgUnitUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<OrgUnitDto>.Ok(await _svc.UpsertOrgUnitAsync(TenantId, UserId, req, ct)));

    [HttpGet("departments")]
    [AuthorizePermission("sys.user.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DepartmentDto>>>> Departments(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<DepartmentDto>>.Ok(await _svc.ListDepartmentsAsync(TenantId, ct)));

    [HttpPost("departments")]
    [AuthorizePermission("sys.user.manage")]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> UpsertDept([FromBody] DepartmentUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<DepartmentDto>.Ok(await _svc.UpsertDepartmentAsync(TenantId, UserId, req, ct)));

    [HttpGet("job-levels")]
    [AuthorizePermission("sys.user.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<JobLevelDto>>>> JobLevels(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<JobLevelDto>>.Ok(await _svc.ListJobLevelsAsync(TenantId, ct)));

    [HttpPost("job-levels")]
    [AuthorizePermission("sys.user.manage")]
    public async Task<ActionResult<ApiResponse<JobLevelDto>>> UpsertJl([FromBody] JobLevelUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<JobLevelDto>.Ok(await _svc.UpsertJobLevelAsync(TenantId, UserId, req, ct)));

    [HttpGet("roles")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RoleDto>>>> Roles(CancellationToken ct)
    {
        // Digi: xem role khi quản trị role HOẶC khi gán role cho user
        await EnsureAnyAsync(ct, "sys.role.read", "sys.role.manage", "sys.user.manage", "sys.role.assign");
        return Ok(ApiResponse<IReadOnlyList<RoleDto>>.Ok(await _svc.ListRolesAsync(TenantId, ct)));
    }

    [HttpPost("roles")]
    [AuthorizePermission("sys.role.update")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> UpsertRole([FromBody] RoleUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<RoleDto>.Ok(await _svc.UpsertRoleAsync(TenantId, UserId, req, ct)));

    [HttpPut("roles/{roleId:guid}/permissions")]
    [AuthorizePermission("sys.role.assign")]
    public async Task<ActionResult<ApiResponse<object>>> SetRolePerms(Guid roleId, [FromBody] List<Guid> permissionIds, CancellationToken ct)
    {
        await _svc.SetRolePermissionsAsync(TenantId, roleId, permissionIds, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    [HttpGet("permissions")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PermissionDto>>>> Permissions(
        [FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        // Catalog chỉ đọc — quyền sinh bằng seed khi làm chức năng; gán vào role qua PUT roles/{id}/permissions
        await EnsureAnyAsync(ct, "sys.permission.read", "sys.role.read", "sys.role.assign", "sys.role.manage");
        return Ok(ApiResponse<IReadOnlyList<PermissionDto>>.Ok(await _svc.ListPermissionsAsync(includeInactive, ct)));
    }

    [HttpGet("users")]
    [AuthorizePermission("sys.user.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserDto>>>> Users(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<UserDto>>.Ok(await _svc.ListUsersAsync(TenantId, UserId, ct)));

    [HttpPost("users")]
    [AuthorizePermission("sys.user.manage")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpsertUser([FromBody] UserUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<UserDto>.Ok(await _svc.UpsertUserAsync(TenantId, UserId, req, ct)));

    [HttpPut("users/{userId:guid}/roles")]
    [AuthorizePermission("sys.user.manage")]
    public async Task<ActionResult<ApiResponse<object>>> SetUserRoles(Guid userId, [FromBody] List<Guid> roleIds, CancellationToken ct)
    {
        await _svc.SetUserRolesAsync(TenantId, userId, roleIds, UserId, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    [HttpDelete("users/{userId:guid}")]
    [AuthorizePermission("sys.user.manage")]
    public async Task<ActionResult<ApiResponse<object>>> SoftDeleteUser(Guid userId, CancellationToken ct)
    {
        await _svc.SoftDeleteUserAsync(TenantId, userId, UserId, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    [HttpPost("users/{userId:guid}/reset-password")]
    [AuthorizePermission("sys.user.manage")]
    public async Task<ActionResult<ApiResponse<ResetPasswordResultDto>>> ResetPassword(Guid userId, CancellationToken ct)
        => Ok(ApiResponse<ResetPasswordResultDto>.Ok(await _svc.AdminResetPasswordAsync(TenantId, userId, UserId, ct)));

    [HttpPost("roles/{roleId:guid}/copy")]
    [AuthorizePermission("sys.role.update")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> CopyRole(Guid roleId, [FromBody] RoleUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<RoleDto>.Ok(await _svc.CopyRoleAsync(TenantId, roleId, UserId, req.Code, req.Name, ct)));

    [HttpGet("menu")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MenuItemDto>>>> Menu(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<MenuItemDto>>.Ok(await _svc.GetMyMenuAsync(TenantId, UserId, ct)));
}
