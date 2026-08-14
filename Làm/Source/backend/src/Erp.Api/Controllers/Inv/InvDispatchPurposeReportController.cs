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
[Route("api/inv/dispatch-purpose-report")]
public sealed class InvDispatchPurposeReportController : ControllerBase
{
    private readonly IInvDispatchPurposeReportService _svc;

    public InvDispatchPurposeReportController(IInvDispatchPurposeReportService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_INV_068: Báo cáo xuất theo mục đích
    [HttpGet("summary")]
    [AuthorizePermission("inv.report.read")]
    public async Task<ActionResult<ApiResponse<InvDispatchPurposeReportSummaryDto>>> GetDispatchPurposeSummaryReport(CancellationToken ct)
        => Ok(ApiResponse<InvDispatchPurposeReportSummaryDto>.Ok(await _svc.GetDispatchPurposeSummaryReportAsync(TenantId, ct)));
}
