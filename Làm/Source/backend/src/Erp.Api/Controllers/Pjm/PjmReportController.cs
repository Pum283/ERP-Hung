using System.Security.Claims;
using System.Text;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Pjm;
using Erp.Application.Interfaces.Services.Pjm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Pjm;

[ApiController]
[Authorize]
[Route("api/pjm/reports")]
public sealed class PjmReportController : ControllerBase
{
    private readonly IPjmReportService _svc;
    public PjmReportController(IPjmReportService svc) => _svc = svc;
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("dashboard")]
    [AuthorizePermission("pjm.project.read")]
    public async Task<ActionResult<ApiResponse<PjmDashboardDto>>> Dashboard(CancellationToken ct)
        => Ok(ApiResponse<PjmDashboardDto>.Ok(await _svc.DashboardAsync(TenantId, ct)));

    [HttpGet("portfolio")]
    [AuthorizePermission("pjm.project.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PjmPortfolioRowDto>>>> Portfolio(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PjmPortfolioRowDto>>.Ok(await _svc.PortfolioAsync(TenantId, ct)));

    [HttpGet("progress")]
    [AuthorizePermission("pjm.project.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PjmProgressHealthRowDto>>>> Progress(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PjmProgressHealthRowDto>>.Ok(await _svc.ProgressHealthAsync(TenantId, ct)));

    [HttpGet("overdue")]
    [AuthorizePermission("pjm.project.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PjmOverdueRowDto>>>> Overdue(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PjmOverdueRowDto>>.Ok(await _svc.OverdueAsync(TenantId, ct)));

    [HttpGet("profit")]
    [AuthorizePermission("pjm.project.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PjmProfitRowDto>>>> Profit(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PjmProfitRowDto>>.Ok(await _svc.ProfitAsync(TenantId, ct)));

    [HttpGet("export.csv")]
    [AuthorizePermission("pjm.project.read")]
    public async Task<IActionResult> Export([FromQuery] string report, CancellationToken ct)
    {
        var csv = await _svc.ExportCsvAsync(TenantId, report, ct);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"pjm-{report}-{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
