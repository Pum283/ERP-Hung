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
[Route("api/hrm")]
public sealed class HrmLeaveController : ControllerBase
{
    private readonly IHrmLeaveService _svc;

    public HrmLeaveController(IHrmLeaveService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("leave-balances")]
    [AuthorizePermission("hrm.leave.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LeaveBalanceDto>>>> Balances([FromQuery] Guid? employeeId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LeaveBalanceDto>>.Ok(await _svc.ListBalancesAsync(TenantId, UserId, employeeId, ct)));

    [HttpPost("leave-balances/adjust")]
    [AuthorizePermission("hrm.leave.manage")]
    public async Task<ActionResult<ApiResponse<LeaveBalanceDto>>> AdjustBalance(
        [FromBody] LeaveBalanceAdjustRequest req, CancellationToken ct)
        => Ok(ApiResponse<LeaveBalanceDto>.Ok(await _svc.AdjustBalanceAsync(TenantId, UserId, req, ct)));

    [HttpPost("leave-balances/allocate")]
    [AuthorizePermission("hrm.leave.manage")]
    public async Task<ActionResult<ApiResponse<object>>> Allocate(
        [FromBody] LeaveAllocateYearRequest req, CancellationToken ct)
    {
        var n = await _svc.AllocateYearAsync(TenantId, UserId, req, ct);
        return Ok(ApiResponse<object>.Ok(new { allocated = n }));
    }

    [HttpGet("leave-requests")]
    [AuthorizePermission("hrm.leave.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LeaveRequestDto>>>> Requests([FromQuery] Guid? employeeId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LeaveRequestDto>>.Ok(await _svc.ListRequestsAsync(TenantId, UserId, employeeId, ct)));

    [HttpPost("leave-requests")]
    [AuthorizePermission("hrm.leave.manage")]
    public async Task<ActionResult<ApiResponse<LeaveRequestDto>>> Create([FromBody] LeaveRequestCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<LeaveRequestDto>.Ok(await _svc.CreateAndOptionallySubmitAsync(TenantId, UserId, req, ct)));

    [HttpPost("leave-requests/{id:guid}/cancel")]
    [AuthorizePermission("hrm.leave.manage")]
    public async Task<ActionResult<ApiResponse<LeaveRequestDto>>> Cancel(Guid id, CancellationToken ct)
        => Ok(ApiResponse<LeaveRequestDto>.Ok(await _svc.CancelRequestAsync(TenantId, UserId, id, ct)));

    [HttpGet("leave-entitlements")]
    [AuthorizePermission("hrm.leave.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LeaveEntitlementRuleDto>>>> Entitlements(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LeaveEntitlementRuleDto>>.Ok(await _svc.ListEntitlementRulesAsync(TenantId, ct)));

    [HttpPost("leave-entitlements")]
    [AuthorizePermission("hrm.leave.manage")]
    public async Task<ActionResult<ApiResponse<LeaveEntitlementRuleDto>>> UpsertEntitlement(
        [FromBody] LeaveEntitlementRuleUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<LeaveEntitlementRuleDto>.Ok(await _svc.UpsertEntitlementRuleAsync(TenantId, UserId, req, ct)));

    [HttpGet("leave-calendar")]
    [AuthorizePermission("hrm.leave.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LeaveCalendarItemDto>>>> Calendar(
        [FromQuery] Guid? orgUnitId, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LeaveCalendarItemDto>>.Ok(
            await _svc.CalendarAsync(TenantId, orgUnitId, from, to, ct)));

    [HttpGet("holidays")]
    [AuthorizePermission("hrm.leave.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<HolidayDto>>>> Holidays([FromQuery] int? year, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<HolidayDto>>.Ok(await _svc.ListHolidaysAsync(TenantId, year, ct)));

    [HttpPost("holidays")]
    [AuthorizePermission("hrm.leave.manage")]
    public async Task<ActionResult<ApiResponse<HolidayDto>>> UpsertHoliday(
        [FromBody] HolidayUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<HolidayDto>.Ok(await _svc.UpsertHolidayAsync(TenantId, UserId, req, ct)));

    [HttpPost("holidays/import")]
    [AuthorizePermission("hrm.leave.manage")]
    public async Task<ActionResult<ApiResponse<object>>> ImportHolidays(
        [FromBody] IReadOnlyList<HolidayImportItem> items, CancellationToken ct)
    {
        var n = await _svc.ImportHolidaysAsync(TenantId, UserId, items, ct);
        return Ok(ApiResponse<object>.Ok(new { imported = n }));
    }

    [HttpGet("leave-report")]
    [AuthorizePermission("hrm.leave.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LeaveReportRowDto>>>> Report(
        [FromQuery] int? year, [FromQuery] Guid? orgUnitId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LeaveReportRowDto>>.Ok(
            await _svc.ReportAsync(TenantId, year ?? DateTime.UtcNow.Year, orgUnitId, ct)));
}
