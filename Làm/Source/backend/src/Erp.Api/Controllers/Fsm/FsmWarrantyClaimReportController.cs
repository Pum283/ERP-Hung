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
[Route("api/fsm/warranty-claim-report")]
public sealed class FsmWarrantyClaimReportController : ControllerBase
{
    private readonly IFsmWarrantyClaimReportService _svc;

    public FsmWarrantyClaimReportController(IFsmWarrantyClaimReportService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_FSM_049: Báo cáo bảo hành
    [HttpGet]
    [AuthorizePermission("fsm.report.read")]
    public async Task<ActionResult<ApiResponse<FsmWarrantyClaimSummaryReportDto>>> GetWarrantyClaimReport(CancellationToken ct)
        => Ok(ApiResponse<FsmWarrantyClaimSummaryReportDto>.Ok(await _svc.GetWarrantyClaimReportAsync(TenantId, ct)));
}
