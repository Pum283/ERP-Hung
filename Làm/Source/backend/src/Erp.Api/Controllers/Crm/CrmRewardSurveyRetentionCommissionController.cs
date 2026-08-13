using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/crm/reward-survey-retention-commission")]
public sealed class CrmRewardSurveyRetentionCommissionController : ControllerBase
{
    private readonly ICrmRewardSurveyRetentionCommissionService _svc;

    public CrmRewardSurveyRetentionCommissionController(ICrmRewardSurveyRetentionCommissionService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_117: Tích điểm / đổi quà
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("redemptions")]
    [AuthorizePermission("crm.loyalty.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmRewardRedemptionDto>>>> GetRedemptions([FromQuery] Guid? customerId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmRewardRedemptionDto>>.Ok(await _svc.GetRedemptionsAsync(TenantId, customerId, ct)));

    [HttpPost("redemptions")]
    [AuthorizePermission("crm.loyalty.write")]
    public async Task<ActionResult<ApiResponse<CrmRewardRedemptionDto>>> RedeemReward([FromBody] CrmRedeemRewardRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmRewardRedemptionDto>.Ok(await _svc.RedeemRewardAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_118: Khảo sát hài lòng
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("surveys")]
    [AuthorizePermission("crm.survey.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmCustomerSurveyResponseDto>>>> GetSurveyResponses(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmCustomerSurveyResponseDto>>.Ok(await _svc.GetSurveyResponsesAsync(TenantId, ct)));

    [HttpPost("surveys")]
    [AuthorizePermission("crm.survey.write")]
    public async Task<ActionResult<ApiResponse<CrmCustomerSurveyResponseDto>>> SubmitSurveyResponse([FromBody] CrmSubmitSurveyResponseRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmCustomerSurveyResponseDto>.Ok(await _svc.SubmitSurveyResponseAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_119: Báo cáo retention / tái mua
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("retention-report")]
    [AuthorizePermission("crm.report.read")]
    public async Task<ActionResult<ApiResponse<CrmCustomerRetentionReportDto>>> GetRetentionReport(CancellationToken ct)
        => Ok(ApiResponse<CrmCustomerRetentionReportDto>.Ok(await _svc.GetRetentionReportAsync(TenantId, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_120: Cấu hình rule hoa hồng
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("commission-rules")]
    [AuthorizePermission("crm.commission.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmCommissionRuleDto>>>> GetCommissionRules(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmCommissionRuleDto>>.Ok(await _svc.GetCommissionRulesAsync(TenantId, ct)));

    [HttpPost("commission-rules")]
    [AuthorizePermission("crm.commission.write")]
    public async Task<ActionResult<ApiResponse<CrmCommissionRuleDto>>> ConfigureCommissionRule([FromBody] CrmConfigureCommissionRuleRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmCommissionRuleDto>.Ok(await _svc.ConfigureCommissionRuleAsync(TenantId, req, ct)));
}
