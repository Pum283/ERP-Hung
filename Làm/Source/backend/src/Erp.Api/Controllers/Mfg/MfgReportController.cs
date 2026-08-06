using System.Security.Claims;
using System.Text;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Mfg;
using Erp.Application.Interfaces.Services.Mfg;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Mfg;

[ApiController]
[Authorize]
[Route("api/mfg/reports")]
public sealed class MfgReportController : ControllerBase
{
    private readonly IMfgReportService _svc;
    public MfgReportController(IMfgReportService svc) => _svc = svc;
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("wo-progress")]
    [AuthorizePermission("mfg.wo.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MfgWoProgressRowDto>>>> WoProgress(
        [FromQuery] string? status, [FromQuery] Guid? workshopId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<MfgWoProgressRowDto>>.Ok(
            await _svc.WoProgressAsync(TenantId, status, workshopId, ct)));

    [HttpGet("output")]
    [AuthorizePermission("mfg.wo.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MfgOutputRowDto>>>> Output(
        [FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to,
        [FromQuery] Guid? workshopId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<MfgOutputRowDto>>.Ok(
            await _svc.OutputByPeriodAsync(TenantId, from, to, workshopId, ct)));

    [HttpGet("material-variance")]
    [AuthorizePermission("mfg.wo.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MfgMaterialVarianceRowDto>>>> MaterialVariance(
        [FromQuery] Guid? workOrderId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<MfgMaterialVarianceRowDto>>.Ok(
            await _svc.MaterialVarianceAsync(TenantId, workOrderId, ct)));

    [HttpGet("dashboard")]
    [AuthorizePermission("mfg.wo.read")]
    public async Task<ActionResult<ApiResponse<MfgDashboardDto>>> Dashboard(
        [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, CancellationToken ct)
        => Ok(ApiResponse<MfgDashboardDto>.Ok(await _svc.DashboardAsync(TenantId, from, to, ct)));

    [HttpGet("export.csv")]
    [AuthorizePermission("mfg.wo.read")]
    public async Task<IActionResult> Export(
        [FromQuery] string report, [FromQuery] string? status = null, [FromQuery] Guid? workshopId = null,
        [FromQuery] Guid? workOrderId = null, [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null, CancellationToken ct = default)
    {
        var csv = await _svc.ExportCsvAsync(TenantId, report, status, workshopId, workOrderId, from, to, ct);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"mfg-{report}-{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
