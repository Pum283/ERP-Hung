using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Crm;
using Erp.Application.Interfaces.Services.Crm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Crm;

/// <summary>Campaign marketing (UC_CRM_016, 019, 023, 026, 029, 031).</summary>
[ApiController]
[Authorize]
[Route("api/crm/campaigns")]
public sealed class CrmCampaignController : ControllerBase
{
    private readonly ICrmCampaignService _svc;
    public CrmCampaignController(ICrmCampaignService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("crm.campaign.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmCampaignDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmCampaignDto>>.Ok(await _svc.ListAsync(TenantId, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("crm.campaign.read")]
    public async Task<ActionResult<ApiResponse<CrmCampaignDto>>> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponse<CrmCampaignDto>.Ok(await _svc.GetAsync(TenantId, id, ct)));

    [HttpPost]
    [AuthorizePermission("crm.campaign.manage")]
    public async Task<ActionResult<ApiResponse<CrmCampaignDto>>> Upsert(
        [FromBody] CrmCampaignUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmCampaignDto>.Ok(await _svc.UpsertAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/close")]
    [AuthorizePermission("crm.campaign.manage")]
    public async Task<ActionResult<ApiResponse<CrmCampaignDto>>> Close(
        Guid id, [FromBody] CrmCampaignCloseRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmCampaignDto>.Ok(await _svc.CloseAsync(TenantId, UserId, id, req, ct)));

    // ── Expenses (UC_CRM_019) ──
    [HttpGet("{id:guid}/expenses")]
    [AuthorizePermission("crm.campaign.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmCampaignExpenseDto>>>> ListExpenses(
        Guid id, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmCampaignExpenseDto>>.Ok(await _svc.ListExpensesAsync(TenantId, id, ct)));

    [HttpPost("{id:guid}/expenses")]
    [AuthorizePermission("crm.campaign.manage")]
    public async Task<ActionResult<ApiResponse<CrmCampaignExpenseDto>>> UpsertExpense(
        Guid id, [FromBody] CrmCampaignExpenseUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmCampaignExpenseDto>.Ok(await _svc.UpsertExpenseAsync(TenantId, UserId, id, req, ct)));

    // ── Web Lead Sync (UC_CRM_026) ──
    [HttpPost("web-leads/sync")]
    [AuthorizePermission("crm.campaign.manage")]
    public async Task<ActionResult<ApiResponse<CrmWebLeadDto>>> SyncWebLead(
        [FromBody] CrmWebLeadSyncRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmWebLeadDto>.Ok(await _svc.SyncWebLeadAsync(TenantId, req, ct)));

    [HttpGet("web-leads")]
    [AuthorizePermission("crm.campaign.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmWebLeadDto>>>> ListWebLeads(
        [FromQuery] string? syncStatus, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmWebLeadDto>>.Ok(await _svc.ListWebLeadsAsync(TenantId, syncStatus, ct)));

    // ── Metrics & Dashboard (UC_CRM_029, 031) ──
    [HttpGet("{id:guid}/metrics")]
    [AuthorizePermission("crm.campaign.read")]
    public async Task<ActionResult<ApiResponse<CrmMarketingMetricsDto>>> GetMetrics(Guid id, CancellationToken ct)
        => Ok(ApiResponse<CrmMarketingMetricsDto>.Ok(await _svc.GetMetricsAsync(TenantId, id, ct)));

    [HttpGet("dashboard")]
    [AuthorizePermission("crm.campaign.read")]
    public async Task<ActionResult<ApiResponse<CrmMarketingDashboardDto>>> GetDashboard(CancellationToken ct)
        => Ok(ApiResponse<CrmMarketingDashboardDto>.Ok(await _svc.GetDashboardAsync(TenantId, ct)));
}
