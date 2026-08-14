namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_PJM_027: Checklist bàn giao
// ────────────────────────────────────────────────────────────────────────────

public record PjmCreateHandoverChecklistRequest(
    Guid ProjectId,
    string ProjectCode,
    string HandoverCriteriaName,
    bool IsSatisfied,
    string CustomerRepresentativeName
);

public record PjmHandoverChecklistItemDto(
    Guid Id,
    Guid ProjectId,
    string ProjectCode,
    string HandoverCriteriaName,
    bool IsSatisfied,
    string CustomerRepresentativeName,
    DateTimeOffset SignedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_PJM_028: Ghi nhận ảnh / biên bản
// ────────────────────────────────────────────────────────────────────────────

public record PjmUploadProtocolAttachmentRequest(
    Guid ProjectId,
    string ProjectCode,
    string AttachmentTitle,
    string AttachmentType,
    string FileUrl,
    long FileSizeBytes
);

public record PjmSiteProtocolAttachmentDto(
    Guid Id,
    Guid ProjectId,
    string ProjectCode,
    string AttachmentTitle,
    string AttachmentType,
    string FileUrl,
    long FileSizeBytes,
    DateTimeOffset UploadedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_PJM_029: Phát sinh change request
// ────────────────────────────────────────────────────────────────────────────

public record PjmCreateEcrRequest(
    Guid ProjectId,
    string ProjectCode,
    string EcrTitle,
    string ChangeReason,
    decimal EstimatedCostImpactVnd,
    int ScheduleImpactDays
);

public record PjmEngineeringChangeRequestDto(
    Guid Id,
    string EcrNumber,
    Guid ProjectId,
    string ProjectCode,
    string EcrTitle,
    string ChangeReason,
    decimal EstimatedCostImpactVnd,
    int ScheduleImpactDays,
    string Status,
    DateTimeOffset CreatedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_PJM_030: Duyệt change request
// ────────────────────────────────────────────────────────────────────────────

public record PjmApproveEcrRequest(
    Guid ChangeRequestId,
    bool IsApproved,
    decimal ApprovedCostAdjustmentVnd,
    int ApprovedScheduleAdjustmentDays,
    string ApproverName,
    string ApprovalComments
);

public record PjmChangeRequestApprovalDto(
    Guid Id,
    Guid ChangeRequestId,
    string EcrNumber,
    bool IsApproved,
    decimal ApprovedCostAdjustmentVnd,
    int ApprovedScheduleAdjustmentDays,
    string ApproverName,
    string ApprovalComments,
    DateTimeOffset ApprovedAt
);
