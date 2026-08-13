using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface ICrmChatbotLeadCsatOrderService
{
    // UC_CRM_045: Chatbot thu thập lead
    Task<CrmCapturedBotLeadDto> CaptureBotLeadAsync(Guid tenantId, CrmCaptureBotLeadRequest req, CancellationToken ct = default);

    // UC_CRM_046: Chuyển bot sang agent
    Task<CrmBotHandoffResultDto> HandoffBotToAgentAsync(Guid tenantId, CrmBotHandoffRequest req, CancellationToken ct = default);

    // UC_CRM_048: Đánh giá CSAT
    Task<CrmCsatRatingDto> SubmitCsatAsync(Guid tenantId, CrmSubmitCsatRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<CrmCsatRatingDto>> GetCsatRatingsAsync(Guid tenantId, CancellationToken ct = default);

    // UC_CRM_080: Tiếp nhận đơn từ kênh online
    Task<CrmOnlineOrderIntakeDto> ReceiveOnlineOrderAsync(Guid tenantId, CrmReceiveOnlineOrderRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<CrmOnlineOrderIntakeDto>> GetOnlineOrdersAsync(Guid tenantId, CancellationToken ct = default);
}
