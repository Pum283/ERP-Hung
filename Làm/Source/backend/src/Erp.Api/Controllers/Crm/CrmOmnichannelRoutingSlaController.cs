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
[Route("api/crm/omnichannel-routing-sla")]
public sealed class CrmOmnichannelRoutingSlaController : ControllerBase
{
    private readonly ICrmOmnichannelRoutingSlaService _svc;

    public CrmOmnichannelRoutingSlaController(ICrmOmnichannelRoutingSlaService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_041: Phân phối hội thoại theo rule
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("rules")]
    [AuthorizePermission("crm.inbox.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmChatRoutingRuleDto>>>> GetRoutingRules(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmChatRoutingRuleDto>>.Ok(await _svc.GetRoutingRulesAsync(TenantId, ct)));

    [HttpPost("rules")]
    [AuthorizePermission("crm.inbox.write")]
    public async Task<ActionResult<ApiResponse<CrmChatRoutingRuleDto>>> CreateRoutingRule([FromBody] CrmCreateRoutingRuleRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmChatRoutingRuleDto>.Ok(await _svc.CreateRoutingRuleAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_042: Chuyển hội thoại giữa agent
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("transfer")]
    [AuthorizePermission("crm.inbox.write")]
    public async Task<ActionResult<ApiResponse<CrmConversationTransferResultDto>>> TransferConversation([FromBody] CrmTransferConversationRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmConversationTransferResultDto>.Ok(await _svc.TransferConversationAsync(TenantId, UserId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_043: SLA phản hồi & cảnh báo
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("sla-check")]
    [AuthorizePermission("crm.inbox.read")]
    public async Task<ActionResult<ApiResponse<CrmChatSlaAlertDto>>> CheckAndLogSla([FromBody] CrmCheckSlaBreachRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmChatSlaAlertDto>.Ok(await _svc.CheckAndLogSlaAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_044: Chatbot kịch bản
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("bot-flows")]
    [AuthorizePermission("crm.inbox.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmScriptedBotFlowDto>>>> GetBotFlows(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmScriptedBotFlowDto>>.Ok(await _svc.GetBotFlowsAsync(TenantId, ct)));

    [HttpPost("bot-flows")]
    [AuthorizePermission("crm.inbox.write")]
    public async Task<ActionResult<ApiResponse<CrmScriptedBotFlowDto>>> SaveBotFlow([FromBody] CrmSaveBotFlowRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmScriptedBotFlowDto>.Ok(await _svc.SaveBotFlowAsync(TenantId, req, ct)));
}
