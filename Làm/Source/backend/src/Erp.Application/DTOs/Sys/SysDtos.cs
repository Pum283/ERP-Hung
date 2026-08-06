using Erp.Domain.Enums.Sys;

namespace Erp.Application.DTOs.Sys;

public sealed record OrgUnitDto(Guid Id, string Code, string Name, Guid? ParentId, string UnitType, bool IsActive);
public sealed record OrgUnitUpsertRequest(Guid? Id, string Code, string Name, Guid? ParentId, string UnitType, bool IsActive);

public sealed record DepartmentDto(Guid Id, string Code, string Name, Guid? ParentId, Guid OrgUnitId, Guid? ManagerUserId, bool IsActive);
public sealed record DepartmentUpsertRequest(Guid? Id, string Code, string Name, Guid? ParentId, Guid OrgUnitId, Guid? ManagerUserId, bool IsActive);

public sealed record JobLevelDto(Guid Id, string Code, string Name, int LevelOrder, ScopeType DefaultScopeType, bool IsActive);
public sealed record JobLevelUpsertRequest(Guid? Id, string Code, string Name, int LevelOrder, ScopeType DefaultScopeType, bool IsActive);

public sealed record RoleDto(Guid Id, string Code, string Name, bool BypassDataScope, bool IsSystem, bool IsActive, IReadOnlyList<Guid> PermissionIds);
public sealed record RoleUpsertRequest(Guid? Id, string Code, string Name, string? Description, bool BypassDataScope, bool IsActive);

public sealed record PermissionDto(
    Guid Id, string ModuleCode, string Code, string Name, string Resource, string Action,
    string? Description, bool IsActive);

/// <summary>Catalog quyền chỉ xem — không tạo/sửa qua API (seed khi làm chức năng).</summary>
public sealed record PermissionUpsertRequest(
    Guid? Id, string ModuleCode, string Code, string Name, string Resource, string Action,
    string? Description, bool IsActive = true);

public sealed record UserDepartmentDto(
    Guid DepartmentId, string? DepartmentName, Guid? JobLevelId, string? JobLevelName, bool IsPrimary);

public sealed record UserDepartmentAssignRequest(Guid DepartmentId, Guid? JobLevelId, bool IsPrimary);

public sealed record UserDto(
    Guid Id, string Username, string? DisplayName, string? Email, UserStatus Status,
    Guid? PrimaryOrgUnitId, Guid? DepartmentId, Guid? JobLevelId, Guid? ManagerUserId,
    IReadOnlyList<Guid> RoleIds, IReadOnlyList<UserDepartmentDto> Departments);

public sealed record UserUpsertRequest(
    Guid? Id, string Username, string? DisplayName, string? Email, string? Phone,
    string? Password, UserStatus Status,
    Guid? PrimaryOrgUnitId, Guid? DepartmentId, Guid? JobLevelId, Guid? ManagerUserId,
    IReadOnlyList<UserDepartmentAssignRequest>? Departments = null);

public sealed record MenuItemDto(
    Guid Id, string Code, Guid? ParentId, string ModuleCode, string Title,
    string? RoutePath, string? PermissionCode, string? Icon, int SortOrder);

public sealed record SetLicenseModuleRequest(bool IsEnabled);
