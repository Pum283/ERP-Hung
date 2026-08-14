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
[Route("api/mfg/quarantine-yield-batch-param")]
public sealed class MfgQuarantineYieldBatchParamController : ControllerBase
{
    private readonly IMfgQuarantineYieldBatchParamService _svc;

    public MfgQuarantineYieldBatchParamController(IMfgQuarantineYieldBatchParamService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_MFG_035: Cách ly hàng lỗi
    [HttpPost("quarantine-holds")]
    [AuthorizePermission("mfg.qc.write")]
    public async Task<ActionResult<ApiResponse<MfgDefectiveQuarantineHoldDto>>> CreateQuarantineHold([FromBody] MfgCreateQuarantineHoldRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgDefectiveQuarantineHoldDto>.Ok(await _svc.CreateQuarantineHoldAsync(TenantId, req, ct)));

    // UC_MFG_036: Báo cáo tỷ lệ đạt QC
    [HttpGet("yield-summary")]
    [AuthorizePermission("mfg.qc.read")]
    public async Task<ActionResult<ApiResponse<MfgQualityYieldSummaryDto>>> GetQualityYieldSummary(CancellationToken ct)
        => Ok(ApiResponse<MfgQualityYieldSummaryDto>.Ok(await _svc.GetQualityYieldSummaryAsync(TenantId, ct)));

    // UC_MFG_037: Lô/mẻ sản xuất
    [HttpPost("batch-lots")]
    [AuthorizePermission("mfg.batch.write")]
    public async Task<ActionResult<ApiResponse<MfgProductionBatchLotDto>>> CreateBatchLot([FromBody] MfgCreateBatchLotRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgProductionBatchLotDto>.Ok(await _svc.CreateBatchLotAsync(TenantId, req, ct)));

    // UC_MFG_038: Ghi nhận thông số mẻ
    [HttpPost("batch-parameters")]
    [AuthorizePermission("mfg.batch.write")]
    public async Task<ActionResult<ApiResponse<MfgBatchProcessParameterDto>>> LogBatchParameter([FromBody] MfgLogBatchParameterRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgBatchProcessParameterDto>.Ok(await _svc.LogBatchParameterAsync(TenantId, req, ct)));
}
