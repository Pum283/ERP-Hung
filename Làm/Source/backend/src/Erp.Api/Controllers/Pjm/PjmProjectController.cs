using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Pjm;
using Erp.Application.Interfaces.Services.Pjm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Pjm;

[ApiController]
[Authorize]
[Route("api/pjm/types")]
public sealed class PjmTypeController : ControllerBase
{
    private readonly IPjmProjectService _svc;
    public PjmTypeController(IPjmProjectService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("pjm.master.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PjmProjectTypeDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PjmProjectTypeDto>>.Ok(await _svc.ListTypesAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("pjm.master.manage")]
    public async Task<ActionResult<ApiResponse<PjmProjectTypeDto>>> Upsert(
        [FromBody] PjmProjectTypeUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PjmProjectTypeDto>.Ok(await _svc.UpsertTypeAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/pjm/statuses")]
public sealed class PjmStatusController : ControllerBase
{
    private readonly IPjmProjectService _svc;
    public PjmStatusController(IPjmProjectService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("pjm.master.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PjmProjectStatusDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PjmProjectStatusDto>>.Ok(await _svc.ListStatusesAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("pjm.master.manage")]
    public async Task<ActionResult<ApiResponse<PjmProjectStatusDto>>> Upsert(
        [FromBody] PjmProjectStatusUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PjmProjectStatusDto>.Ok(await _svc.UpsertStatusAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/pjm/wbs-templates")]
public sealed class PjmWbsTemplateController : ControllerBase
{
    private readonly IPjmProjectService _svc;
    public PjmWbsTemplateController(IPjmProjectService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("pjm.master.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PjmWbsTemplateDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PjmWbsTemplateDto>>.Ok(await _svc.ListTemplatesAsync(TenantId, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("pjm.master.read")]
    public async Task<ActionResult<ApiResponse<PjmWbsTemplateDetailDto>>> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PjmWbsTemplateDetailDto>.Ok(await _svc.GetTemplateDetailAsync(TenantId, id, ct)));

    [HttpPost]
    [AuthorizePermission("pjm.master.manage")]
    public async Task<ActionResult<ApiResponse<PjmWbsTemplateDto>>> Upsert(
        [FromBody] PjmWbsTemplateUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PjmWbsTemplateDto>.Ok(await _svc.UpsertTemplateAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/items")]
    [AuthorizePermission("pjm.master.manage")]
    public async Task<ActionResult<ApiResponse<PjmWbsTemplateItemDto>>> UpsertItem(
        Guid id, [FromBody] PjmWbsTemplateItemUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PjmWbsTemplateItemDto>.Ok(await _svc.UpsertTemplateItemAsync(TenantId, UserId, id, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/pjm/projects")]
public sealed class PjmProjectController : ControllerBase
{
    private readonly IPjmProjectService _svc;
    private readonly IPjmCostCloseService _cost;
    public PjmProjectController(IPjmProjectService svc, IPjmCostCloseService cost)
    {
        _svc = svc;
        _cost = cost;
    }
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("pjm.project.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PjmProjectDto>>>> List(
        [FromQuery] string? q, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PjmProjectDto>>.Ok(await _svc.ListProjectsAsync(TenantId, q, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("pjm.project.read")]
    public async Task<ActionResult<ApiResponse<PjmProjectDetailDto>>> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PjmProjectDetailDto>.Ok(await _svc.GetProjectDetailAsync(TenantId, id, ct)));

    [HttpPost]
    [AuthorizePermission("pjm.project.manage")]
    public async Task<ActionResult<ApiResponse<PjmProjectDto>>> Upsert(
        [FromBody] PjmProjectUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PjmProjectDto>.Ok(await _svc.UpsertProjectAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/members")]
    [AuthorizePermission("pjm.project.manage")]
    public async Task<ActionResult<ApiResponse<PjmProjectMemberDto>>> UpsertMember(
        Guid id, [FromBody] PjmProjectMemberUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PjmProjectMemberDto>.Ok(await _svc.UpsertMemberAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/wbs")]
    [AuthorizePermission("pjm.project.manage")]
    public async Task<ActionResult<ApiResponse<PjmWbsItemDto>>> UpsertWbs(
        Guid id, [FromBody] PjmWbsItemUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PjmWbsItemDto>.Ok(await _svc.UpsertWbsItemAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/expenses")]
    [AuthorizePermission("pjm.project.manage")]
    public async Task<ActionResult<ApiResponse<PjmExpenseDto>>> UpsertExpense(
        Guid id, [FromBody] PjmExpenseUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PjmExpenseDto>.Ok(await _cost.UpsertExpenseAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/material-issues")]
    [AuthorizePermission("pjm.project.manage")]
    public async Task<ActionResult<ApiResponse<PjmMaterialIssueDto>>> CreateMaterialIssue(
        Guid id, [FromBody] PjmMaterialIssueCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<PjmMaterialIssueDto>.Ok(await _cost.CreateMaterialIssueAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/acceptances")]
    [AuthorizePermission("pjm.project.manage")]
    public async Task<ActionResult<ApiResponse<PjmAcceptanceDto>>> CreateAcceptance(
        Guid id, [FromBody] PjmAcceptanceCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<PjmAcceptanceDto>.Ok(await _cost.CreateAcceptanceAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/acceptances/{acceptanceId:guid}/sign")]
    [AuthorizePermission("pjm.project.manage")]
    public async Task<ActionResult<ApiResponse<PjmAcceptanceDto>>> SignAcceptance(
        Guid id, Guid acceptanceId, [FromBody] PjmAcceptanceSignRequest req, CancellationToken ct)
        => Ok(ApiResponse<PjmAcceptanceDto>.Ok(
            await _cost.SignAcceptanceAsync(TenantId, UserId, id, acceptanceId, req, ct)));

    [HttpPost("{id:guid}/revenue")]
    [AuthorizePermission("pjm.project.manage")]
    public async Task<ActionResult<ApiResponse<PjmProjectDto>>> RecognizeRevenue(
        Guid id, [FromBody] PjmRecognizeRevenueRequest req, CancellationToken ct)
        => Ok(ApiResponse<PjmProjectDto>.Ok(await _cost.RecognizeRevenueAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/close")]
    [AuthorizePermission("pjm.project.manage")]
    public async Task<ActionResult<ApiResponse<PjmProjectDto>>> Close(
        Guid id, [FromBody] PjmCloseProjectRequest req, CancellationToken ct)
        => Ok(ApiResponse<PjmProjectDto>.Ok(await _cost.CloseProjectAsync(TenantId, UserId, id, req, ct)));
}
