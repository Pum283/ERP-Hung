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
[Route("api/mfg/cost-variance-qc-inspection")]
public sealed class MfgCostVarianceQcInspectionController : ControllerBase
{
    private readonly IMfgCostVarianceQcInspectionService _svc;

    public MfgCostVarianceQcInspectionController(IMfgCostVarianceQcInspectionService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_MFG_030: Đối chiếu lý thuyết vs thực tế
    [HttpPost("cost-variance-analyses")]
    [AuthorizePermission("mfg.cost.write")]
    public async Task<ActionResult<ApiResponse<MfgCostVarianceAnalysisDto>>> AnalyzeCostVariance([FromBody] MfgAnalyzeCostVarianceRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgCostVarianceAnalysisDto>.Ok(await _svc.AnalyzeCostVarianceAsync(TenantId, req, ct)));

    // UC_MFG_032: Tiêu chí QC đầu vào
    [HttpPost("incoming-qc-criteria")]
    [AuthorizePermission("mfg.qc.write")]
    public async Task<ActionResult<ApiResponse<MfgIncomingQcCriterionDto>>> CreateIncomingQcCriterion([FromBody] MfgCreateIncomingQcCriterionRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgIncomingQcCriterionDto>.Ok(await _svc.CreateIncomingQcCriterionAsync(TenantId, req, ct)));

    [HttpGet("incoming-qc-criteria")]
    [AuthorizePermission("mfg.qc.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MfgIncomingQcCriterionDto>>>> GetIncomingQcCriteria(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<MfgIncomingQcCriterionDto>>.Ok(await _svc.GetIncomingQcCriteriaAsync(TenantId, ct)));

    // UC_MFG_033: QC thành phẩm
    [HttpPost("finished-goods-qc")]
    [AuthorizePermission("mfg.qc.write")]
    public async Task<ActionResult<ApiResponse<MfgFinishedGoodsQcCheckDto>>> PerformFinishedGoodsQc([FromBody] MfgPerformFinishedGoodsQcRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgFinishedGoodsQcCheckDto>.Ok(await _svc.PerformFinishedGoodsQcAsync(TenantId, req, ct)));

    // UC_MFG_034: Ghi nhận lô đạt / không đạt
    [HttpPost("inspection-lot-dispositions")]
    [AuthorizePermission("mfg.qc.write")]
    public async Task<ActionResult<ApiResponse<MfgInspectionLotDispositionDto>>> DecideLotDisposition([FromBody] MfgDecideLotDispositionRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgInspectionLotDispositionDto>.Ok(await _svc.DecideLotDispositionAsync(TenantId, req, ct)));
}
