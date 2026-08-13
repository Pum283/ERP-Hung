using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface ICrmOmnichannelRoutingSlaService
{
    // UC_CRM_041: Phân phối hội thoại theo rule
    Task<CrmChatRoutingRuleDto> CreateRoutingRuleAsync(Guid tenantId, CrmCreateRoutingRuleRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<CrmChatRoutingRuleDto>> GetRoutingRulesAsync(Guid tenantId, CancellationToken ct = default);

    // UC_CRM_042: Chuyển hội thoại giữa agent
    Task<CrmConversationTransferResultDto> TransferConversationAsync(Guid tenantId, Guid fromAgentId, CrmTransferConversationRequest req, CancellationToken ct = default);

    // UC_CRM_043: SLA phản hồi & cảnh báo
    Task<CrmChatSlaAlertDto> CheckAndLogSlaAsync(Guid tenantId, CrmCheckSlaBreachRequest req, CancellationToken ct = default);

    // UC_CRM_044: Chatbot kịch bản
    Task<CrmScriptedBotFlowDto> SaveBotFlowAsync(Guid tenantId, CrmSaveBotFlowRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<CrmScriptedBotFlowDto>> GetBotFlowsAsync(Guid tenantId, CancellationToken ct = default);
}
