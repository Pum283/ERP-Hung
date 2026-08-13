namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_007: Đánh giá tiềm năng
// ────────────────────────────────────────────────────────────────────────────

public record CrmEvaluatePotentialRequest(
    Guid CustomerId,
    int Score,
    string? Notes
);

public record CrmPotentialScoreDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    int Score,
    string PriorityTier,
    Guid? EvaluatorId,
    string EvaluatorName,
    string Notes,
    DateTimeOffset EvaluatedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_022: Nhân bản campaign
// ────────────────────────────────────────────────────────────────────────────

public record CrmDuplicateCampaignRequest(
    Guid OriginalCampaignId,
    string NewCampaignName,
    DateTimeOffset? NewStartDate,
    DateTimeOffset? NewEndDate
);

public record CrmCampaignDuplicateResultDto(
    Guid NewCampaignId,
    string NewCampaignName,
    string Status,
    DateTimeOffset CreatedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_039: Hộp thư tập trung đa kênh
// ────────────────────────────────────────────────────────────────────────────

public record CrmOmnichannelConversationDto(
    Guid Id,
    string Channel,
    string ExternalId,
    string CustomerName,
    string CustomerPhone,
    Guid? AssignedAgentId,
    string AgentName,
    string Status,
    string LastMessageSnippet,
    DateTimeOffset LastMessageAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_040: Tiếp nhận hội thoại mới
// ────────────────────────────────────────────────────────────────────────────

public record CrmReceiveConversationRequest(
    Guid ConversationId,
    Guid TargetAgentId
);

public record CrmConversationAssignResultDto(
    Guid ConversationId,
    Guid AssignedAgentId,
    string AgentName,
    string Status,
    DateTimeOffset AssignedAt
);
