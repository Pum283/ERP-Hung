using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Wf;
using Erp.Application.Interfaces.Services.Wf;
using Erp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Erp.Api.Controllers.Wf;

[ApiController]
[Authorize]
[Route("api/wf")]
public sealed class WfController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWfRuntimeService _wf;

    public WfController(AppDbContext db, IWfRuntimeService wf)
    {
        _db = db;
        _wf = wf;
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("definitions")]
    [AuthorizePermission("wf.task.read")]
    public async Task<ActionResult<ApiResponse<object>>> Definitions(CancellationToken ct)
    {
        var list = await _db.WfDefinitions.AsNoTracking()
            .Where(x => x.TenantId == TenantId && !x.IsDeleted)
            .Select(x => new { x.Id, x.Code, x.Name, x.ModuleCode, x.DocType, x.IsActive })
            .ToListAsync(ct);
        return Ok(ApiResponse<object>.Ok(list));
    }

    [HttpGet("tasks/my")]
    [AuthorizePermission("wf.task.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WfTaskDto>>>> MyTasks(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<WfTaskDto>>.Ok(await _wf.MyPendingTasksAsync(TenantId, UserId, ct)));

    [HttpPost("tasks/{taskId:guid}/act")]
    [AuthorizePermission("wf.task.act")]
    public async Task<ActionResult<ApiResponse<object>>> Act(Guid taskId, [FromBody] WfActRequest req, CancellationToken ct)
    {
        await _wf.ActAsync(TenantId, taskId, UserId, req, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    [HttpGet("delegations")]
    [AuthorizePermission("wf.task.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WfDelegationDto>>>> Delegations(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<WfDelegationDto>>.Ok(
            await _wf.ListDelegationsAsync(TenantId, UserId, ct)));

    [HttpPost("delegations")]
    [AuthorizePermission("wf.task.act")]
    public async Task<ActionResult<ApiResponse<WfDelegationDto>>> UpsertDelegation(
        [FromBody] WfDelegationUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<WfDelegationDto>.Ok(await _wf.UpsertDelegationAsync(TenantId, UserId, req, ct)));

    [HttpPost("delegations/{id:guid}/deactivate")]
    [AuthorizePermission("wf.task.act")]
    public async Task<ActionResult<ApiResponse<object>>> DeactivateDelegation(Guid id, CancellationToken ct)
    {
        await _wf.DeactivateDelegationAsync(TenantId, UserId, id, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    [HttpGet("dashboard")]
    [AuthorizePermission("wf.task.read")]
    public async Task<ActionResult<ApiResponse<WfDashboardDto>>> Dashboard(CancellationToken ct)
        => Ok(ApiResponse<WfDashboardDto>.Ok(await _wf.DashboardAsync(TenantId, ct)));
}
