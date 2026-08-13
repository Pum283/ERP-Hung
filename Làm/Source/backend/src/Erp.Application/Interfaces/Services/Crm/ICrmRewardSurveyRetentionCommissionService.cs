using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface ICrmRewardSurveyRetentionCommissionService
{
    // UC_CRM_117: Tích điểm / đổi quà
    Task<CrmRewardRedemptionDto> RedeemRewardAsync(Guid tenantId, CrmRedeemRewardRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<CrmRewardRedemptionDto>> GetRedemptionsAsync(Guid tenantId, Guid? customerId = null, CancellationToken ct = default);

    // UC_CRM_118: Khảo sát hài lòng
    Task<CrmCustomerSurveyResponseDto> SubmitSurveyResponseAsync(Guid tenantId, CrmSubmitSurveyResponseRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<CrmCustomerSurveyResponseDto>> GetSurveyResponsesAsync(Guid tenantId, CancellationToken ct = default);

    // UC_CRM_119: Báo cáo retention / tái mua
    Task<CrmCustomerRetentionReportDto> GetRetentionReportAsync(Guid tenantId, CancellationToken ct = default);

    // UC_CRM_120: Cấu hình rule hoa hồng
    Task<CrmCommissionRuleDto> ConfigureCommissionRuleAsync(Guid tenantId, CrmConfigureCommissionRuleRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<CrmCommissionRuleDto>> GetCommissionRulesAsync(Guid tenantId, CancellationToken ct = default);
}
