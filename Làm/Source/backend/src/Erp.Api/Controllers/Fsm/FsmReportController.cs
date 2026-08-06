using System.Security.Claims;
using System.Text;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Fsm;
using Erp.Application.Interfaces.Services.Fsm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Fsm;

[ApiController]
[Authorize]
[Route("api/fsm/reports")]
public sealed class FsmReportController : ControllerBase
{
    private readonly IFsmReportService _svc;
    public FsmReportController(IFsmReportService svc) => _svc = svc;
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("dashboard")]
    [AuthorizePermission("fsm.ticket.read")]
    public async Task<ActionResult<ApiResponse<FsmDashboardDto>>> Dashboard(CancellationToken ct)
        => Ok(ApiResponse<FsmDashboardDto>.Ok(await _svc.DashboardAsync(TenantId, ct)));

    [HttpGet("sla")]
    [AuthorizePermission("fsm.ticket.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FsmSlaComplianceRowDto>>>> Sla(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FsmSlaComplianceRowDto>>.Ok(await _svc.SlaComplianceAsync(TenantId, ct)));

    [HttpGet("productivity")]
    [AuthorizePermission("fsm.ticket.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FsmTechProductivityRowDto>>>> Productivity(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FsmTechProductivityRowDto>>.Ok(await _svc.TechProductivityAsync(TenantId, ct)));

    [HttpGet("parts")]
    [AuthorizePermission("fsm.ticket.read")]
    public async Task<ActionResult<ApiResponse<FsmPartCostSummaryDto>>> Parts(CancellationToken ct)
        => Ok(ApiResponse<FsmPartCostSummaryDto>.Ok(await _svc.PartCostAsync(TenantId, ct)));

    [HttpGet("export.csv")]
    [AuthorizePermission("fsm.ticket.read")]
    public async Task<IActionResult> Export([FromQuery] string report, CancellationToken ct)
    {
        var csv = await _svc.ExportCsvAsync(TenantId, report, ct);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"fsm-{report}-{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
