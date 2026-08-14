namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_PJM_003: Mẫu checklist nghiệm thu
// ────────────────────────────────────────────────────────────────────────────

public record PjmCreateAcceptanceTemplateRequest(
    string TemplateCode,
    string TemplateName,
    string ProjectCategory,
    string ChecklistItemContent,
    int SequenceOrder,
    bool IsMandatory
);

public record PjmAcceptanceChecklistTemplateDto(
    Guid Id,
    string TemplateCode,
    string TemplateName,
    string ProjectCategory,
    string ChecklistItemContent,
    int SequenceOrder,
    bool IsMandatory
);

// ────────────────────────────────────────────────────────────────────────────
// UC_PJM_016: Gantt / timeline tiến độ
// ────────────────────────────────────────────────────────────────────────────

public record PjmCreateMilestoneRequest(
    Guid ProjectId,
    string MilestoneCode,
    string MilestoneName,
    DateTimeOffset PlannedStartDate,
    DateTimeOffset PlannedEndDate,
    double CompletionProgressPct,
    string PredecessorMilestoneCode,
    string Status
);

public record PjmGanttTimelineMilestoneDto(
    Guid Id,
    Guid ProjectId,
    string MilestoneCode,
    string MilestoneName,
    DateTimeOffset PlannedStartDate,
    DateTimeOffset PlannedEndDate,
    double CompletionProgressPct,
    string PredecessorMilestoneCode,
    string Status
);

// ────────────────────────────────────────────────────────────────────────────
// UC_PJM_018: Nhật ký thay đổi kế hoạch
// ────────────────────────────────────────────────────────────────────────────

public record PjmLogPlanChangeRequest(
    Guid ProjectId,
    string ProjectCode,
    string ChangeTitle,
    string ReasonForChange,
    string RequestedBy
);

public record PjmPlanChangeAuditLogDto(
    Guid Id,
    Guid ProjectId,
    string ProjectCode,
    string ChangeTitle,
    string ReasonForChange,
    string RequestedBy,
    string ApprovalStatus,
    DateTimeOffset RequestedAt
);
