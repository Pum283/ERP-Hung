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
[Route("api/crm/lead-campaign-inbox")]
public sealed class CrmLeadCampaignInboxController : ControllerBase
{
    private readonly ICrmLeadCampaignInboxService _svc;

    public CrmLeadCampaignInboxController(ICrmLeadCampaignInboxService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_007: Đánh giá tiềm năng
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("potential-scores")]
    [AuthorizePermission("crm.lead.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmPotentialScoreDto>>>> GetPotentialScores(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmPotentialScoreDto>>.Ok(await _svc.GetPotentialScoresAsync(TenantId, ct)));

    [HttpPost("potential-scores/evaluate")]
    [AuthorizePermission("crm.lead.write")]
    public async Task<ActionResult<ApiResponse<CrmPotentialScoreDto>>> EvaluateLeadPotential([FromBody] CrmEvaluatePotentialRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmPotentialScoreDto>.Ok(await _svc.EvaluateLeadPotentialAsync(TenantId, UserId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_022: Nhân bản campaign
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("campaigns/duplicate")]
    [AuthorizePermission("crm.campaign.write")]
    public async Task<ActionResult<ApiResponse<CrmCampaignDuplicateResultDto>>> DuplicateCampaign([FromBody] CrmDuplicateCampaignRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmCampaignDuplicateResultDto>.Ok(await _svc.DuplicateCampaignAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_039: Hộp thư tập trung đa kênh
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("conversations")]
    [AuthorizePermission("crm.inbox.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmOmnichannelConversationDto>>>> GetConversations([FromQuery] string? channel, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmOmnichannelConversationDto>>.Ok(await _svc.GetConversationsAsync(TenantId, channel, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_040: Tiếp nhận hội thoại mới
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("conversations/assign")]
    [AuthorizePermission("crm.inbox.write")]
    public async Task<ActionResult<ApiResponse<CrmConversationAssignResultDto>>> ReceiveAndAssignConversation([FromBody] CrmReceiveConversationRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmConversationAssignResultDto>.Ok(await _svc.ReceiveAndAssignConversationAsync(TenantId, req, ct)));
}
