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
[Route("api/crm/chatbot-lead-csat-order")]
public sealed class CrmChatbotLeadCsatOrderController : ControllerBase
{
    private readonly ICrmChatbotLeadCsatOrderService _svc;

    public CrmChatbotLeadCsatOrderController(ICrmChatbotLeadCsatOrderService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_045: Chatbot thu thập lead
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("bot-leads/capture")]
    [AuthorizePermission("crm.lead.write")]
    public async Task<ActionResult<ApiResponse<CrmCapturedBotLeadDto>>> CaptureBotLead([FromBody] CrmCaptureBotLeadRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmCapturedBotLeadDto>.Ok(await _svc.CaptureBotLeadAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_046: Chuyển bot sang agent
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("bot-handoff")]
    [AuthorizePermission("crm.inbox.write")]
    public async Task<ActionResult<ApiResponse<CrmBotHandoffResultDto>>> HandoffBotToAgent([FromBody] CrmBotHandoffRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmBotHandoffResultDto>.Ok(await _svc.HandoffBotToAgentAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_048: Đánh giá CSAT
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("csat-ratings")]
    [AuthorizePermission("crm.inbox.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmCsatRatingDto>>>> GetCsatRatings(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmCsatRatingDto>>.Ok(await _svc.GetCsatRatingsAsync(TenantId, ct)));

    [HttpPost("csat-ratings")]
    [AuthorizePermission("crm.inbox.write")]
    public async Task<ActionResult<ApiResponse<CrmCsatRatingDto>>> SubmitCsat([FromBody] CrmSubmitCsatRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmCsatRatingDto>.Ok(await _svc.SubmitCsatAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_080: Tiếp nhận đơn từ kênh online
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("online-orders")]
    [AuthorizePermission("crm.order.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmOnlineOrderIntakeDto>>>> GetOnlineOrders(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmOnlineOrderIntakeDto>>.Ok(await _svc.GetOnlineOrdersAsync(TenantId, ct)));

    [HttpPost("online-orders")]
    [AuthorizePermission("crm.order.write")]
    public async Task<ActionResult<ApiResponse<CrmOnlineOrderIntakeDto>>> ReceiveOnlineOrder([FromBody] CrmReceiveOnlineOrderRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmOnlineOrderIntakeDto>.Ok(await _svc.ReceiveOnlineOrderAsync(TenantId, req, ct)));
}
