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
[Route("api/hrm/attendance")]
public sealed class HrmAttendanceController : ControllerBase
{
    private readonly IHrmAttendanceService _svc;

    public HrmAttendanceController(IHrmAttendanceService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("policy")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<AttendancePolicyDto>>> GetPolicy(CancellationToken ct)
        => Ok(ApiResponse<AttendancePolicyDto>.Ok(await _svc.GetPolicyAsync(TenantId, ct)));

    [HttpPut("policy")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<AttendancePolicyDto>>> UpsertPolicy(
        [FromBody] AttendancePolicyUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<AttendancePolicyDto>.Ok(await _svc.UpsertPolicyAsync(TenantId, UserId, req, ct)));

    [HttpGet("devices")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AttendanceDeviceDto>>>> Devices(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<AttendanceDeviceDto>>.Ok(await _svc.ListDevicesAsync(TenantId, ct)));

    [HttpPost("devices")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<AttendanceDeviceDto>>> UpsertDevice(
        [FromBody] AttendanceDeviceUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<AttendanceDeviceDto>.Ok(await _svc.UpsertDeviceAsync(TenantId, UserId, req, ct)));

    [HttpGet("geofences")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AttendanceGeoFenceDto>>>> GeoFences(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<AttendanceGeoFenceDto>>.Ok(await _svc.ListGeoFencesAsync(TenantId, ct)));

    [HttpPost("geofences")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<AttendanceGeoFenceDto>>> UpsertGeo(
        [FromBody] AttendanceGeoFenceUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<AttendanceGeoFenceDto>.Ok(await _svc.UpsertGeoFenceAsync(TenantId, UserId, req, ct)));

    [HttpPost("check-in")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<AttendanceRecordDto>>> CheckIn(
        [FromBody] AttendancePunchRequest req, CancellationToken ct)
        => Ok(ApiResponse<AttendanceRecordDto>.Ok(await _svc.CheckInAsync(TenantId, UserId, req, ct)));

    [HttpPost("check-out")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<AttendanceRecordDto>>> CheckOut(
        [FromBody] AttendancePunchRequest req, CancellationToken ct)
        => Ok(ApiResponse<AttendanceRecordDto>.Ok(await _svc.CheckOutAsync(TenantId, UserId, req, ct)));

    [HttpGet("mine")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AttendanceRecordDto>>>> Mine(
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<AttendanceRecordDto>>.Ok(
            await _svc.MyHistoryAsync(TenantId, UserId, from, to, ct)));

    [HttpGet("board")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AttendanceRecordDto>>>> Board(
        [FromQuery] Guid? orgUnitId, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<AttendanceRecordDto>>.Ok(
            await _svc.BoardAsync(TenantId, orgUnitId, from, to, ct)));

    [HttpGet("alerts")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AttendanceMissingAlertDto>>>> Alerts(
        [FromQuery] DateOnly? date, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<AttendanceMissingAlertDto>>.Ok(
            await _svc.MissingAlertsAsync(TenantId, date, ct)));

    [HttpPost("mark-missing")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<object>>> MarkMissing([FromQuery] DateOnly date, CancellationToken ct)
    {
        var n = await _svc.MarkMissingAsync(TenantId, UserId, date, ct);
        return Ok(ApiResponse<object>.Ok(new { marked = n }));
    }

    [HttpPost("sync-device")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<object>>> SyncDevice(
        [FromBody] AttendanceDeviceSyncRequest req, CancellationToken ct)
    {
        var n = await _svc.SyncDeviceAsync(TenantId, UserId, req, ct);
        return Ok(ApiResponse<object>.Ok(new { synced = n }));
    }

    [HttpPost("recalc-ot")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<object>>> RecalcOt(
        [FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct)
    {
        var n = await _svc.RecalcOtAsync(TenantId, UserId, from, to, ct);
        return Ok(ApiResponse<object>.Ok(new { recalculated = n }));
    }

    [HttpGet("adjusts")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AttendanceAdjustDto>>>> Adjusts(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<AttendanceAdjustDto>>.Ok(await _svc.ListAdjustsAsync(TenantId, ct)));

    [HttpPost("adjusts")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<AttendanceAdjustDto>>> CreateAdjust(
        [FromBody] AttendanceAdjustCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<AttendanceAdjustDto>.Ok(await _svc.CreateAdjustAsync(TenantId, UserId, req, ct)));

    [HttpPost("adjusts/{id:guid}/approve")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<AttendanceAdjustDto>>> ApproveAdjust(Guid id, CancellationToken ct)
        => Ok(ApiResponse<AttendanceAdjustDto>.Ok(await _svc.DecideAdjustAsync(TenantId, UserId, id, true, ct)));

    [HttpPost("adjusts/{id:guid}/reject")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<AttendanceAdjustDto>>> RejectAdjust(Guid id, CancellationToken ct)
        => Ok(ApiResponse<AttendanceAdjustDto>.Ok(await _svc.DecideAdjustAsync(TenantId, UserId, id, false, ct)));

    [HttpGet("locks")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AttendancePeriodLockDto>>>> Locks(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<AttendancePeriodLockDto>>.Ok(await _svc.ListLocksAsync(TenantId, ct)));

    [HttpPost("locks")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<AttendancePeriodLockDto>>> Lock(
        [FromBody] AttendanceLockRequest req, CancellationToken ct)
        => Ok(ApiResponse<AttendancePeriodLockDto>.Ok(await _svc.LockPeriodAsync(TenantId, UserId, req, ct)));

    [HttpPost("locks/{periodKey}/unlock")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<AttendancePeriodLockDto>>> Unlock(string periodKey, CancellationToken ct)
        => Ok(ApiResponse<AttendancePeriodLockDto>.Ok(await _svc.UnlockPeriodAsync(TenantId, UserId, periodKey, ct)));

    [HttpPost("records/{id:guid}/confirm")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<object>>> Confirm(Guid id, CancellationToken ct)
    {
        await _svc.ConfirmRecordAsync(TenantId, UserId, id, ct);
        return Ok(ApiResponse<object>.Ok(new { }));
    }
}
