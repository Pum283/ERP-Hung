using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface ICrmLeadCampaignInboxService
{
    // UC_CRM_007: Đánh giá tiềm năng
    Task<CrmPotentialScoreDto> EvaluateLeadPotentialAsync(Guid tenantId, Guid evaluatorId, CrmEvaluatePotentialRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<CrmPotentialScoreDto>> GetPotentialScoresAsync(Guid tenantId, CancellationToken ct = default);

    // UC_CRM_022: Nhân bản campaign
    Task<CrmCampaignDuplicateResultDto> DuplicateCampaignAsync(Guid tenantId, CrmDuplicateCampaignRequest req, CancellationToken ct = default);

    // UC_CRM_039: Hộp thư tập trung đa kênh
    Task<IReadOnlyList<CrmOmnichannelConversationDto>> GetConversationsAsync(Guid tenantId, string? channel = null, CancellationToken ct = default);

    // UC_CRM_040: Tiếp nhận hội thoại mới
    Task<CrmConversationAssignResultDto> ReceiveAndAssignConversationAsync(Guid tenantId, CrmReceiveConversationRequest req, CancellationToken ct = default);
}
