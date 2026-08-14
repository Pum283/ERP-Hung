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
[Route("api/pjm/warranty-productivity")]
public sealed class PjmWarrantyProductivityController : ControllerBase
{
    private readonly IPjmWarrantyProductivityService _svc;

    public PjmWarrantyProductivityController(IPjmWarrantyProductivityService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_PJM_037: Bảo hành sau dự án
    [HttpPost("warranty-coverages")]
    [AuthorizePermission("pjm.warranty.write")]
    public async Task<ActionResult<ApiResponse<PjmPostProjectWarrantyCoverageDto>>> CreateWarrantyCoverage([FromBody] PjmCreateWarrantyCoverageRequest req, CancellationToken ct)
        => Ok(ApiResponse<PjmPostProjectWarrantyCoverageDto>.Ok(await _svc.CreateWarrantyCoverageAsync(TenantId, req, ct)));

    [HttpGet("warranty-coverages")]
    [AuthorizePermission("pjm.warranty.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PjmPostProjectWarrantyCoverageDto>>>> GetWarrantyCoverages(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PjmPostProjectWarrantyCoverageDto>>.Ok(await _svc.GetWarrantyCoveragesAsync(TenantId, ct)));

    // UC_PJM_041: Năng suất nguồn lực
    [HttpGet("resource-productivity")]
    [AuthorizePermission("pjm.report.read")]
    public async Task<ActionResult<ApiResponse<PjmResourceProductivityReportDto>>> GetResourceProductivityReport(CancellationToken ct)
        => Ok(ApiResponse<PjmResourceProductivityReportDto>.Ok(await _svc.GetResourceProductivityReportAsync(TenantId, ct)));
}
