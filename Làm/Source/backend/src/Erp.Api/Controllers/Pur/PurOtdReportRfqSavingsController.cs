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
[Route("api/pur/otd-report-rfq-savings")]
public sealed class PurOtdReportRfqSavingsController : ControllerBase
{
    private readonly IPurOtdReportRfqSavingsService _svc;

    public PurOtdReportRfqSavingsController(IPurOtdReportRfqSavingsService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_PUR_049: Báo cáo đúng hạn giao hàng (OTD)
    [HttpGet("vendor-otd-performance")]
    [AuthorizePermission("pur.report.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PurVendorOtdPerformanceDto>>>> GetVendorOtdPerformanceReport(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PurVendorOtdPerformanceDto>>.Ok(await _svc.GetVendorOtdPerformanceReportAsync(TenantId, ct)));

    // UC_PUR_050: Báo cáo tiết kiệm chi phí từ RFQ
    [HttpGet("rfq-savings-summary")]
    [AuthorizePermission("pur.report.read")]
    public async Task<ActionResult<ApiResponse<PurRfqSavingsSummaryDto>>> GetRfqSavingsSummaryReport(CancellationToken ct)
        => Ok(ApiResponse<PurRfqSavingsSummaryDto>.Ok(await _svc.GetRfqSavingsSummaryReportAsync(TenantId, ct)));
}
