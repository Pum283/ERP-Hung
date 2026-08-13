namespace Erp.Application.DTOs.Hrm;

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_005: Quản lý bộ phận trong đơn vị
// ────────────────────────────────────────────────────────────────────────────

public record HrmDepartmentDto(
    Guid Id,
    string Code,
    string Name,
    Guid? ParentId,
    string? ParentName,
    Guid OrgUnitId,
    string? OrgUnitName,
    Guid? ManagerUserId,
    string? ManagerName,
    string Path,
    int SortOrder,
    bool IsActive,
    DateTimeOffset CreatedAt
);

public record HrmDepartmentUpsertRequest(
    string Code,
    string Name,
    Guid? ParentId,
    Guid OrgUnitId,
    Guid? ManagerUserId,
    int SortOrder = 0,
    bool IsActive = true
);

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_008: Quản lý vị trí công việc
// ────────────────────────────────────────────────────────────────────────────

public record JobPositionDto(
    Guid Id,
    string Code,
    string Name,
    Guid? DefaultJobLevelId,
    string? DefaultJobLevelName,
    int SortOrder,
    bool IsActive,
    DateTimeOffset CreatedAt
);

public record JobPositionUpsertRequest(
    string Code,
    string Name,
    Guid? DefaultJobLevelId,
    int SortOrder = 0,
    bool IsActive = true
);

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_011: Định nghĩa trung tâm chi phí NS
// ────────────────────────────────────────────────────────────────────────────

public record HrmCostCenterDto(
    Guid Id,
    string Code,
    string Name,
    Guid? OrgUnitId,
    string? OrgUnitName,
    decimal AllocationPercentage,
    bool IsActive,
    DateTimeOffset CreatedAt
);

public record HrmCostCenterUpsertRequest(
    string Code,
    string Name,
    Guid? OrgUnitId,
    decimal AllocationPercentage = 100m,
    bool IsActive = true
);

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_023: Quản lý người thân / liên hệ khẩn
// ────────────────────────────────────────────────────────────────────────────

public record EmployeeRelativeDto(
    Guid Id,
    Guid EmployeeId,
    string? EmployeeName,
    string FullName,
    string Relationship,
    string? Phone,
    string? Address,
    bool IsEmergencyContact,
    bool IsTaxDependent,
    string? IdNumber,
    DateTimeOffset CreatedAt
);

public record EmployeeRelativeUpsertRequest(
    Guid EmployeeId,
    string FullName,
    string Relationship = "Spouse",
    string? Phone = null,
    string? Address = null,
    bool IsEmergencyContact = true,
    bool IsTaxDependent = false,
    string? IdNumber = null
);
