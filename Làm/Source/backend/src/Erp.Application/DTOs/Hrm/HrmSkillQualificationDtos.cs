namespace Erp.Application.DTOs.Hrm;

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_024: Quản lý trình độ / kỹ năng
// ────────────────────────────────────────────────────────────────────────────

public record HrmEmployeeSkillDto(
    Guid Id,
    Guid EmployeeId,
    string? EmployeeName,
    string SkillName,
    string ProficiencyLevel,
    string? CertificateRef,
    DateTimeOffset CreatedAt
);

public record HrmEmployeeSkillUpsertRequest(
    Guid EmployeeId,
    string SkillName,
    string ProficiencyLevel = "Intermediate",
    string? CertificateRef = null
);

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_037: Báo cáo biến động nhân sự
// ────────────────────────────────────────────────────────────────────────────

public record DepartmentHeadcountStatDto(
    Guid DepartmentId,
    string DepartmentName,
    int TotalCount,
    int ActiveCount
);

public record HrmPersonnelMovementReportDto(
    int TotalEmployees,
    int ActiveCount,
    int OnLeaveCount,
    int TerminatedCount,
    int JoinersInPeriod,
    int LeaversInPeriod,
    decimal TurnoverRatePercentage,
    IReadOnlyList<DepartmentHeadcountStatDto> DepartmentBreakdown,
    DateOnly FromDate,
    DateOnly ToDate
);

public record HrmPersonnelMovementFilter(
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    Guid? OrgUnitId = null
);

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_044: In / xuất mẫu hợp đồng
// ────────────────────────────────────────────────────────────────────────────

public record HrmContractTemplatePrintDto(
    Guid ContractId,
    string ContractNo,
    string EmployeeCode,
    string EmployeeName,
    string ContractType,
    DateOnly StartDate,
    DateOnly? EndDate,
    decimal? BaseSalary,
    string FormattedTemplateText,
    DateTimeOffset GeneratedAt
);

public record HrmContractExportRequest(
    Guid ContractId,
    string TemplateFormat = "Standard"
);

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_058: Import ứng viên hàng loạt
// ────────────────────────────────────────────────────────────────────────────

public record HrmBulkCandidateImportItem(
    string FullName,
    string? Email,
    string? Phone,
    Guid JobPostingId
);

public record HrmBulkCandidateImportError(
    int RowIndex,
    string FullName,
    string ErrorMessage
);

public record HrmBulkCandidateImportResult(
    int TotalProcessed,
    int SuccessCount,
    int FailedCount,
    IReadOnlyList<Guid> ImportedCandidateIds,
    IReadOnlyList<HrmBulkCandidateImportError> Errors
);
