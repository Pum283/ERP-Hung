using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Hrm;
using Erp.Application.Interfaces.Services.Hrm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Hrm;

[ApiController]
[Authorize]
[Route("api/hrm/shifts")]
public sealed class HrmShiftController : ControllerBase
{
    private readonly IHrmShiftService _svc;

    public HrmShiftController(IHrmShiftService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("templates")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WorkShiftDto>>>> ListTemplates(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<WorkShiftDto>>.Ok(await _svc.ListTemplatesAsync(TenantId, ct)));

    [HttpPost("templates")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<WorkShiftDto>>> UpsertTemplate(
        [FromBody] WorkShiftUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<WorkShiftDto>.Ok(await _svc.UpsertTemplateAsync(TenantId, UserId, req, ct)));

    [HttpGet("assignments")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ShiftAssignmentDto>>>> ListAssignments(
        [FromQuery] Guid? orgUnitId, [FromQuery] Guid? employeeId,
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<ShiftAssignmentDto>>.Ok(
            await _svc.ListAssignmentsAsync(TenantId, orgUnitId, employeeId, from, to, ct)));

    [HttpGet("assignments/mine")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ShiftAssignmentDto>>>> MyAssignments(
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<ShiftAssignmentDto>>.Ok(
            await _svc.MyAssignmentsAsync(TenantId, UserId, from, to, ct)));

    [HttpPost("assignments")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<ShiftAssignmentDto>>> Assign(
        [FromBody] ShiftAssignRequest req, CancellationToken ct)
        => Ok(ApiResponse<ShiftAssignmentDto>.Ok(await _svc.AssignAsync(TenantId, UserId, req, ct)));

    [HttpPost("assignments/range")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ShiftAssignmentDto>>>> AssignRange(
        [FromBody] ShiftAssignRangeRequest req, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<ShiftAssignmentDto>>.Ok(
            await _svc.AssignRangeAsync(TenantId, UserId, req, ct)));

    [HttpPost("assignments/swap")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<object>>> Swap(
        [FromBody] ShiftSwapRequest req, CancellationToken ct)
    {
        await _svc.SwapAsync(TenantId, UserId, req, ct);
        return Ok(ApiResponse<object>.Ok(new { }));
    }

    [HttpPost("assignments/{id:guid}/cancel")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<object>>> Cancel(Guid id, CancellationToken ct)
    {
        await _svc.CancelAsync(TenantId, UserId, id, ct);
        return Ok(ApiResponse<object>.Ok(new { }));
    }

    [HttpPost("assignments/copy")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<object>>> Copy(
        [FromBody] ShiftCopyRequest req, CancellationToken ct)
    {
        var n = await _svc.CopyAsync(TenantId, UserId, req, ct);
        return Ok(ApiResponse<object>.Ok(new { copied = n }));
    }

    [HttpGet("locks")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ShiftPeriodLockDto>>>> ListLocks(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<ShiftPeriodLockDto>>.Ok(await _svc.ListLocksAsync(TenantId, ct)));

    [HttpPost("locks")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<ShiftPeriodLockDto>>> Lock(
        [FromBody] ShiftLockRequest req, CancellationToken ct)
        => Ok(ApiResponse<ShiftPeriodLockDto>.Ok(await _svc.LockPeriodAsync(TenantId, UserId, req, ct)));

    [HttpGet("export")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<IActionResult> Export(
        [FromQuery] Guid? orgUnitId, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
    {
        var csv = await _svc.ExportCsvAsync(TenantId, orgUnitId, from, to, ct);
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "shift-schedule.csv");
    }
}
