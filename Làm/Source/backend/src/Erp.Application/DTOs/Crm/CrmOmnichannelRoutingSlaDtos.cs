namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_041: Phân phối hội thoại theo rule
// ────────────────────────────────────────────────────────────────────────────

public record CrmCreateRoutingRuleRequest(
    string RuleName,
    string Strategy,
    string TargetSkillGroup,
    int Priority
);

public record CrmChatRoutingRuleDto(
    Guid Id,
    string RuleName,
    string Strategy,
    string TargetSkillGroup,
    bool IsActive,
    int Priority
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_042: Chuyển hội thoại giữa agent
// ────────────────────────────────────────────────────────────────────────────

public record CrmTransferConversationRequest(
    Guid ConversationId,
    Guid TargetAgentId,
    string TransferNote
);

public record CrmConversationTransferResultDto(
    Guid ConversationId,
    Guid FromAgentId,
    Guid ToAgentId,
    string ToAgentName,
    string Status,
    string TransferNote,
    DateTimeOffset TransferredAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_043: SLA phản hồi & cảnh báo
// ────────────────────────────────────────────────────────────────────────────

public record CrmCheckSlaBreachRequest(
    Guid ConversationId,
    int MaxResponseMinutes,
    int ActualResponseMinutes
);

public record CrmChatSlaAlertDto(
    Guid Id,
    Guid ConversationId,
    int MaxResponseMinutes,
    int ActualResponseMinutes,
    bool IsBreached,
    string AlertStatus,
    DateTimeOffset BreachedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_044: Chatbot kịch bản
// ────────────────────────────────────────────────────────────────────────────

public record CrmSaveBotFlowRequest(
    string FlowName,
    string TriggerKeyword,
    string StepsJson
);

public record CrmScriptedBotFlowDto(
    Guid Id,
    string FlowName,
    string TriggerKeyword,
    string StepsJson,
    bool IsActive,
    DateTimeOffset UpdatedAt
);
