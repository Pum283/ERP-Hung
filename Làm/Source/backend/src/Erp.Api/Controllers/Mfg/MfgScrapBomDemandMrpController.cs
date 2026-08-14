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
[Route("api/mfg/scrap-bom-demand-mrp")]
public sealed class MfgScrapBomDemandMrpController : ControllerBase
{
    private readonly IMfgScrapBomDemandMrpService _svc;

    public MfgScrapBomDemandMrpController(IMfgScrapBomDemandMrpService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_MFG_009: Định mức hao hụt
    [HttpPost("scrap-allowances")]
    [AuthorizePermission("mfg.bom.write")]
    public async Task<ActionResult<ApiResponse<MfgBomScrapAllowanceDto>>> SetBomScrapAllowance([FromBody] MfgSetBomScrapAllowanceRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgBomScrapAllowanceDto>.Ok(await _svc.SetBomScrapAllowanceAsync(TenantId, req, ct)));

    // UC_MFG_011: Sao chép BOM
    [HttpPost("bom-copies")]
    [AuthorizePermission("mfg.bom.write")]
    public async Task<ActionResult<ApiResponse<MfgBomCopyLogDto>>> CopyBom([FromBody] MfgCopyBomRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgBomCopyLogDto>.Ok(await _svc.CopyBomAsync(TenantId, req, ct)));

    // UC_MFG_012: Kế hoạch SX theo nhu cầu (MPS)
    [HttpPost("demand-plans")]
    [AuthorizePermission("mfg.plan.write")]
    public async Task<ActionResult<ApiResponse<MfgDemandProductionPlanDto>>> CreateDemandProductionPlan([FromBody] MfgCreateDemandProductionPlanRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgDemandProductionPlanDto>.Ok(await _svc.CreateDemandProductionPlanAsync(TenantId, req, ct)));

    // UC_MFG_014: Tính nhu cầu nguyên vật liệu (MRP)
    [HttpPost("mrp-runs")]
    [AuthorizePermission("mfg.plan.write")]
    public async Task<ActionResult<ApiResponse<MfgMaterialRequirementPlanningDto>>> RunMrpCalculation([FromBody] MfgRunMrpCalculationRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgMaterialRequirementPlanningDto>.Ok(await _svc.RunMrpCalculationAsync(TenantId, req, ct)));
}
