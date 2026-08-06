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
[Route("api/hrm/dashboard")]
public sealed class HrmDashboardController : ControllerBase
{
    private readonly IHrmDashboardService _svc;

    public HrmDashboardController(IHrmDashboardService svc) => _svc = svc;

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<HrmDashboardBundleDto>>> Bundle(
        [FromQuery] DateOnly? attFrom, [FromQuery] DateOnly? attTo,
        [FromQuery] int? leaveYear, [FromQuery] Guid? periodId, CancellationToken ct)
        => Ok(ApiResponse<HrmDashboardBundleDto>.Ok(
            await _svc.BundleAsync(TenantId, attFrom, attTo, leaveYear, periodId, ct)));

    [HttpGet("headcount")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<HrmDashboardHeadcountDto>>> Headcount(CancellationToken ct)
        => Ok(ApiResponse<HrmDashboardHeadcountDto>.Ok(await _svc.HeadcountAsync(TenantId, ct)));

    [HttpGet("attendance")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<HrmAttendanceReportRowDto>>>> Attendance(
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<HrmAttendanceReportRowDto>>.Ok(
            await _svc.AttendanceReportAsync(TenantId, from, to, ct)));

    [HttpGet("recruit-funnel")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<HrmRecruitFunnelRowDto>>>> Funnel(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<HrmRecruitFunnelRowDto>>.Ok(await _svc.RecruitFunnelAsync(TenantId, ct)));

    [HttpGet("leave")]
    [AuthorizePermission("hrm.leave.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<HrmLeaveSummaryRowDto>>>> Leave(
        [FromQuery] int? year, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<HrmLeaveSummaryRowDto>>.Ok(await _svc.LeaveSummaryAsync(TenantId, year, ct)));

    [HttpGet("cost")]
    [AuthorizePermission("hrm.payroll.read")]
    public async Task<ActionResult<ApiResponse<HrmCostSummaryDto>>> Cost(
        [FromQuery] Guid? periodId, CancellationToken ct)
        => Ok(ApiResponse<HrmCostSummaryDto>.Ok(await _svc.CostSummaryAsync(TenantId, periodId, ct)));

    [HttpGet("headcount-vs-plan")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<HeadcountCompareRowDto>>>> VsPlan(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<HeadcountCompareRowDto>>.Ok(await _svc.HeadcountVsPlanAsync(TenantId, ct)));
}
