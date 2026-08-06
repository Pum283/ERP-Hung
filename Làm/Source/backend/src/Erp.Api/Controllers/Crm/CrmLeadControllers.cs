using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Crm;
using Erp.Application.Interfaces.Services.Crm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Crm;

[ApiController]
[Authorize]
[Route("api/crm/lead-sources")]
public sealed class CrmLeadSourceController : ControllerBase
{
    private readonly ICrmLeadService _svc;
    public CrmLeadSourceController(ICrmLeadService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("crm.lead.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmLeadSourceDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmLeadSourceDto>>.Ok(await _svc.ListSourcesAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("crm.lead.manage")]
    public async Task<ActionResult<ApiResponse<CrmLeadSourceDto>>> Upsert(
        [FromBody] CrmLeadSourceUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmLeadSourceDto>.Ok(await _svc.UpsertSourceAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/crm/leads")]
public sealed class CrmLeadController : ControllerBase
{
    private readonly ICrmLeadService _svc;
    public CrmLeadController(ICrmLeadService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("crm.lead.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmLeadDto>>>> List(
        [FromQuery] string? q, [FromQuery] string? status, [FromQuery] Guid? ownerUserId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmLeadDto>>.Ok(
            await _svc.ListLeadsAsync(TenantId, q, status, ownerUserId, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("crm.lead.read")]
    public async Task<ActionResult<ApiResponse<CrmLeadDetailDto>>> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponse<CrmLeadDetailDto>.Ok(await _svc.GetLeadDetailAsync(TenantId, id, ct)));

    [HttpPost]
    [AuthorizePermission("crm.lead.manage")]
    public async Task<ActionResult<ApiResponse<CrmLeadDto>>> Upsert(
        [FromBody] CrmLeadUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmLeadDto>.Ok(await _svc.UpsertLeadAsync(TenantId, UserId, req, ct)));

    [HttpPost("auto-intake")]
    [AuthorizePermission("crm.lead.manage")]
    public async Task<ActionResult<ApiResponse<CrmLeadDto>>> AutoIntake(
        [FromBody] CrmLeadAutoIntakeRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmLeadDto>.Ok(await _svc.AutoIntakeAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/assign")]
    [AuthorizePermission("crm.lead.manage")]
    public async Task<ActionResult<ApiResponse<CrmLeadDto>>> Assign(
        Guid id, [FromBody] CrmLeadAssignRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmLeadDto>.Ok(await _svc.AssignAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/status")]
    [AuthorizePermission("crm.lead.manage")]
    public async Task<ActionResult<ApiResponse<CrmLeadDto>>> SetStatus(
        Guid id, [FromBody] CrmLeadStatusRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmLeadDto>.Ok(await _svc.SetStatusAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/mark-lost")]
    [AuthorizePermission("crm.lead.manage")]
    public async Task<ActionResult<ApiResponse<CrmLeadDto>>> MarkLost(
        Guid id, [FromBody] CrmLeadLostRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmLeadDto>.Ok(await _svc.MarkLostAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/convert")]
    [AuthorizePermission("crm.lead.manage")]
    public async Task<ActionResult<ApiResponse<CrmOpportunityDto>>> Convert(Guid id, CancellationToken ct)
        => Ok(ApiResponse<CrmOpportunityDto>.Ok(await _svc.ConvertToOpportunityAsync(TenantId, UserId, id, ct)));

    [HttpPost("tasks")]
    [AuthorizePermission("crm.lead.manage")]
    public async Task<ActionResult<ApiResponse<CrmLeadTaskDto>>> UpsertTask(
        [FromBody] CrmLeadTaskUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmLeadTaskDto>.Ok(await _svc.UpsertTaskAsync(TenantId, UserId, req, ct)));

    [HttpPost("activities")]
    [AuthorizePermission("crm.lead.manage")]
    public async Task<ActionResult<ApiResponse<CrmLeadActivityDto>>> AddActivity(
        [FromBody] CrmLeadActivityUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmLeadActivityDto>.Ok(await _svc.AddActivityAsync(TenantId, UserId, req, ct)));

    [HttpPost("import")]
    [AuthorizePermission("crm.lead.manage")]
    public async Task<ActionResult<ApiResponse<CrmLeadImportResult>>> Import(
        [FromBody] CrmLeadImportRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmLeadImportResult>.Ok(await _svc.ImportCsvAsync(TenantId, UserId, req, ct)));

    [HttpGet("conversion-report")]
    [AuthorizePermission("crm.lead.read")]
    public async Task<ActionResult<ApiResponse<CrmLeadConversionReportDto>>> ConversionReport(CancellationToken ct)
        => Ok(ApiResponse<CrmLeadConversionReportDto>.Ok(await _svc.GetConversionReportAsync(TenantId, ct)));
}

[ApiController]
[Authorize]
[Route("api/crm/opportunities")]
public sealed class CrmOpportunityController : ControllerBase
{
    private readonly ICrmLeadService _svc;
    public CrmOpportunityController(ICrmLeadService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("crm.opportunity.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmOpportunityDto>>>> List(
        [FromQuery] string? q, [FromQuery] string? stage, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmOpportunityDto>>.Ok(
            await _svc.ListOpportunitiesAsync(TenantId, q, stage, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("crm.opportunity.read")]
    public async Task<ActionResult<ApiResponse<CrmOpportunityDetailDto>>> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponse<CrmOpportunityDetailDto>.Ok(await _svc.GetOpportunityDetailAsync(TenantId, id, ct)));

    [HttpPost]
    [AuthorizePermission("crm.opportunity.manage")]
    public async Task<ActionResult<ApiResponse<CrmOpportunityDto>>> Upsert(
        [FromBody] CrmOpportunityUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmOpportunityDto>.Ok(await _svc.UpsertOpportunityAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/lines")]
    [AuthorizePermission("crm.opportunity.manage")]
    public async Task<ActionResult<ApiResponse<CrmOpportunityLineDto>>> UpsertLine(
        Guid id, [FromBody] CrmOpportunityLineUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmOpportunityLineDto>.Ok(
            await _svc.UpsertOpportunityLineAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/stage")]
    [AuthorizePermission("crm.opportunity.manage")]
    public async Task<ActionResult<ApiResponse<CrmOpportunityDto>>> SetStage(
        Guid id, [FromBody] CrmOpportunityStageRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmOpportunityDto>.Ok(
            await _svc.SetOpportunityStageAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/create-quote")]
    [AuthorizePermission("crm.opportunity.manage")]
    public async Task<ActionResult<ApiResponse<CrmQuoteDto>>> CreateQuote(Guid id, CancellationToken ct)
        => Ok(ApiResponse<CrmQuoteDto>.Ok(await _svc.CreateQuoteFromOpportunityAsync(TenantId, UserId, id, ct)));
}
