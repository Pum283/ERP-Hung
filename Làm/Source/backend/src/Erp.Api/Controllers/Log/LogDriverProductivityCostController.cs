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
[Route("api/log/driver-productivity-cost")]
public sealed class LogDriverProductivityCostController : ControllerBase
{
    private readonly ILogDriverProductivityCostService _svc;

    public LogDriverProductivityCostController(ILogDriverProductivityCostService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_LOG_036: Năng suất tài xế / chuyến
    [HttpGet("productivity-reports")]
    [AuthorizePermission("log.report.read")]
    public async Task<ActionResult<ApiResponse<LogDriverProductivitySummaryDto>>> GetDriverProductivityReport(CancellationToken ct)
        => Ok(ApiResponse<LogDriverProductivitySummaryDto>.Ok(await _svc.GetDriverProductivityReportAsync(TenantId, ct)));

    // UC_LOG_037: Chi phí vận chuyển
    [HttpPost("trip-costs/calculate")]
    [AuthorizePermission("log.cost.write")]
    public async Task<ActionResult<ApiResponse<LogShippingCostAllocationDto>>> CalculateTripCost([FromBody] LogCalculateTripCostRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogShippingCostAllocationDto>.Ok(await _svc.CalculateTripCostAsync(TenantId, req, ct)));
}
