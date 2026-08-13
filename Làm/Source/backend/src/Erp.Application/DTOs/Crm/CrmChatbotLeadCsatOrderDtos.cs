namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_045: Chatbot thu thập lead
// ────────────────────────────────────────────────────────────────────────────

public record CrmCaptureBotLeadRequest(
    string CustomerName,
    string Phone,
    string Email,
    string Note
);

public record CrmCapturedBotLeadDto(
    Guid LeadId,
    string CustomerName,
    string Phone,
    string Email,
    string Status,
    DateTimeOffset CreatedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_046: Chuyển bot sang agent
// ────────────────────────────────────────────────────────────────────────────

public record CrmBotHandoffRequest(
    Guid ConversationId,
    Guid TargetAgentId,
    string Reason
);

public record CrmBotHandoffResultDto(
    Guid ConversationId,
    Guid AssignedAgentId,
    string AgentName,
    string HandoffStatus,
    DateTimeOffset HandedOverAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_048: Đánh giá CSAT
// ────────────────────────────────────────────────────────────────────────────

public record CrmSubmitCsatRequest(
    Guid ConversationId,
    Guid? AgentId,
    int Score,
    string FeedbackText
);

public record CrmCsatRatingDto(
    Guid Id,
    Guid ConversationId,
    Guid? AgentId,
    int Score,
    string FeedbackText,
    DateTimeOffset RatedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_CRM_080: Tiếp nhận đơn từ kênh online
// ────────────────────────────────────────────────────────────────────────────

public record CrmReceiveOnlineOrderRequest(
    string Channel,
    string ExternalOrderCode,
    string CustomerName,
    string Phone,
    decimal TotalAmount
);

public record CrmOnlineOrderIntakeDto(
    Guid Id,
    string Channel,
    string ExternalOrderCode,
    string CustomerName,
    string Phone,
    decimal TotalAmount,
    string Status,
    DateTimeOffset ReceivedAt
);
