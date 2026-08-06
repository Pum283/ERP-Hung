using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Mod;
using Erp.Application.Interfaces.Services.Wf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Wf;

[ApiController]
[Authorize]
[Route("api/wf")]
public sealed class WorkOpsController : ControllerBase
{
    private readonly IWorkOpsService _svc;

    public WorkOpsController(IWorkOpsService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("work-types")]
    [AuthorizePermission("wf.task.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WorkTypeDto>>>> Types(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<WorkTypeDto>>.Ok(await _svc.ListTypesAsync(TenantId, ct)));

    [HttpPost("work-types")]
    [AuthorizePermission("wf.task.act")]
    public async Task<ActionResult<ApiResponse<WorkTypeDto>>> UpsertType([FromBody] WorkTypeDto req, CancellationToken ct)
        => Ok(ApiResponse<WorkTypeDto>.Ok(await _svc.UpsertTypeAsync(TenantId, UserId, req, ct)));

    [HttpGet("work-projects")]
    [AuthorizePermission("wf.task.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WorkProjectDto>>>> Projects(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<WorkProjectDto>>.Ok(await _svc.ListProjectsAsync(TenantId, ct)));

    [HttpPost("work-projects")]
    [AuthorizePermission("wf.task.act")]
    public async Task<ActionResult<ApiResponse<WorkProjectDto>>> UpsertProject([FromBody] WorkProjectDto req, CancellationToken ct)
        => Ok(ApiResponse<WorkProjectDto>.Ok(await _svc.UpsertProjectAsync(TenantId, UserId, req, ct)));

    [HttpGet("work-items")]
    [AuthorizePermission("wf.task.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WorkItemDto>>>> Items(
        [FromQuery] string? status, [FromQuery] Guid? assigneeId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<WorkItemDto>>.Ok(await _svc.ListItemsAsync(TenantId, UserId, status, assigneeId, ct)));

    [HttpPost("work-items")]
    [AuthorizePermission("wf.task.act")]
    public async Task<ActionResult<ApiResponse<WorkItemDto>>> UpsertItem([FromBody] WorkItemUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<WorkItemDto>.Ok(await _svc.UpsertItemAsync(TenantId, UserId, req, ct)));

    [HttpGet("workload")]
    [AuthorizePermission("wf.task.read")]
    public async Task<ActionResult<ApiResponse<object>>> Workload(CancellationToken ct)
        => Ok(ApiResponse<object>.Ok(await _svc.OpenWorkloadAsync(TenantId, ct)));
}
