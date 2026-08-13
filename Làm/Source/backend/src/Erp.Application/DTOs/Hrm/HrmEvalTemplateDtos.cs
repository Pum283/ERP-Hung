namespace Erp.Application.DTOs.Hrm;

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_177: Mẫu đánh giá KPI / năng lực
// ────────────────────────────────────────────────────────────────────────────

public record HrmKpiTemplateDto(
    Guid Id,
    string Code,
    string Title,
    string? TargetRole,
    string CriteriaDescription,
    decimal MaxScore,
    decimal WeightPercentage,
    DateTimeOffset CreatedAt
);

public record HrmKpiTemplateUpsertRequest(
    string Code,
    string Title,
    string? TargetRole = null,
    string CriteriaDescription = "",
    decimal MaxScore = 100m,
    decimal WeightPercentage = 100m
);

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_178: Tạo kỳ đánh giá
// ────────────────────────────────────────────────────────────────────────────

public record HrmEvaluationCycleDto(
    Guid Id,
    string CycleName,
    string PeriodKey,
    DateOnly StartDate,
    DateOnly EndDate,
    Guid? KpiTemplateId,
    string? KpiTemplateTitle,
    string Status,
    DateTimeOffset CreatedAt
);

public record HrmEvaluationCycleUpsertRequest(
    string CycleName,
    string PeriodKey,
    DateOnly StartDate,
    DateOnly EndDate,
    Guid? KpiTemplateId = null,
    string Status = "Draft"
);

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_179: Quản lý đánh giá nhân viên
// ────────────────────────────────────────────────────────────────────────────

public record HrmManagerEvaluationDto(
    Guid Id,
    Guid EvaluationCycleId,
    string? EvaluationCycleName,
    Guid EmployeeId,
    string? EmployeeName,
    Guid EvaluatorId,
    string? EvaluatorName,
    decimal KpiScore,
    decimal CompetencyScore,
    string FinalGrade,
    string? ManagerComments,
    string Status,
    DateTimeOffset CreatedAt
);

public record HrmManagerEvaluationUpsertRequest(
    Guid EvaluationCycleId,
    Guid EmployeeId,
    Guid EvaluatorId,
    decimal KpiScore = 0m,
    decimal CompetencyScore = 0m,
    string FinalGrade = "B",
    string? ManagerComments = null,
    string Status = "Pending"
);

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_180: Nhân viên tự đánh giá
// ────────────────────────────────────────────────────────────────────────────

public record HrmSelfEvaluationDto(
    Guid Id,
    Guid EmployeeId,
    string? EmployeeName,
    string AppraisalPeriod,
    string KeyAchievements,
    string AreasForImprovement,
    int SelfRating,
    string Status,
    DateTimeOffset CreatedAt
);

public record HrmSelfEvaluationUpsertRequest(
    Guid EmployeeId,
    string AppraisalPeriod,
    string KeyAchievements = "",
    string AreasForImprovement = "",
    int SelfRating = 5,
    string Status = "Draft"
);
