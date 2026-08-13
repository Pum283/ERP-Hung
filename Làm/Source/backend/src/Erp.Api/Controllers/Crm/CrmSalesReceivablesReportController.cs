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
[Route("api/crm/sales-receivables-report")]
public sealed class CrmSalesReceivablesReportController : ControllerBase
{
    private readonly ICrmSalesReceivablesReportService _svc;

    public CrmSalesReceivablesReportController(ICrmSalesReceivablesReportService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_CRM_130: Báo cáo công nợ bán
    [HttpGet("receivables-aging")]
    [AuthorizePermission("crm.report.read")]
    public async Task<ActionResult<ApiResponse<CrmSalesReceivablesAgingSummaryDto>>> GetReceivablesAgingReport(CancellationToken ct)
        => Ok(ApiResponse<CrmSalesReceivablesAgingSummaryDto>.Ok(await _svc.GetReceivablesAgingReportAsync(TenantId, ct)));

    // UC_CRM_131: Xuất báo cáo định kỳ
    [HttpGet("scheduled-exports")]
    [AuthorizePermission("crm.report.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CrmScheduledReportExportDto>>>> GetScheduledReportExports(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CrmScheduledReportExportDto>>.Ok(await _svc.GetScheduledReportExportsAsync(TenantId, ct)));

    [HttpPost("scheduled-exports")]
    [AuthorizePermission("crm.report.export")]
    public async Task<ActionResult<ApiResponse<CrmScheduledReportExportDto>>> ScheduleReportExport([FromBody] CrmScheduleReportExportRequest req, CancellationToken ct)
        => Ok(ApiResponse<CrmScheduledReportExportDto>.Ok(await _svc.ScheduleReportExportAsync(TenantId, req, ct)));
}
