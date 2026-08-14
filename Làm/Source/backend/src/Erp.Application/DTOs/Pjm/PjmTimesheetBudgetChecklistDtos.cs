namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_PJM_020: Timesheet theo dự án
// ────────────────────────────────────────────────────────────────────────────

public record PjmCreateTimesheetRequest(
    Guid ProjectId,
    string ProjectCode,
    Guid EmployeeUserId,
    string EmployeeName,
    string TaskDescription,
    decimal HoursSpent,
    decimal OvertimeHours,
    DateTimeOffset WorkDate
);

public record PjmProjectTimesheetEntryDto(
    Guid Id,
    Guid ProjectId,
    string ProjectCode,
    Guid EmployeeUserId,
    string EmployeeName,
    string TaskDescription,
    decimal HoursSpent,
    decimal OvertimeHours,
    string Status,
    DateTimeOffset WorkDate
);

// ────────────────────────────────────────────────────────────────────────────
// UC_PJM_024: Cảnh báo vượt ngân sách
// ────────────────────────────────────────────────────────────────────────────

public record PjmBudgetOverrunWarningDto(
    Guid Id,
    Guid ProjectId,
    string ProjectCode,
    string ProjectName,
    decimal ApprovedBudgetVnd,
    decimal ActualCommittedCostVnd,
    decimal OverrunAmountVnd,
    double OverrunPercent,
    string WarningSeverity,
    DateTimeOffset GeneratedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_PJM_025: Checklist khảo sát
// ────────────────────────────────────────────────────────────────────────────

public record PjmCreateSurveyChecklistRequest(
    Guid ProjectId,
    string ProjectCode,
    string SurveyItemTitle,
    string TechnicalStandard,
    bool IsSatisfied,
    string InspectorNotes
);

public record PjmSurveyChecklistItemDto(
    Guid Id,
    Guid ProjectId,
    string ProjectCode,
    string SurveyItemTitle,
    string TechnicalStandard,
    bool IsSatisfied,
    string InspectorNotes,
    DateTimeOffset CheckedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_PJM_026: Checklist lắp đặt
// ────────────────────────────────────────────────────────────────────────────

public record PjmCreateInstallationChecklistRequest(
    Guid ProjectId,
    string ProjectCode,
    string InstallationStepTitle,
    string EquipmentTag,
    bool IsCompleted,
    string TechnicianSigner
);

public record PjmInstallationChecklistItemDto(
    Guid Id,
    Guid ProjectId,
    string ProjectCode,
    string InstallationStepTitle,
    string EquipmentTag,
    bool IsCompleted,
    string TechnicianSigner,
    DateTimeOffset InstalledAt
);
