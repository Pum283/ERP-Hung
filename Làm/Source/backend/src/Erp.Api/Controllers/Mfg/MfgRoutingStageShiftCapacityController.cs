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
[Route("api/mfg/routing-stage-shift-capacity")]
public sealed class MfgRoutingStageShiftCapacityController : ControllerBase
{
    private readonly IMfgRoutingStageShiftCapacityService _svc;

    public MfgRoutingStageShiftCapacityController(IMfgRoutingStageShiftCapacityService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_MFG_004: Danh mục công đoạn
    [HttpPost("routing-stages")]
    [AuthorizePermission("mfg.routing.write")]
    public async Task<ActionResult<ApiResponse<MfgRoutingStageDto>>> CreateRoutingStage([FromBody] MfgCreateRoutingStageRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgRoutingStageDto>.Ok(await _svc.CreateRoutingStageAsync(TenantId, req, ct)));

    [HttpGet("routing-stages")]
    [AuthorizePermission("mfg.routing.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MfgRoutingStageDto>>>> GetRoutingStages(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<MfgRoutingStageDto>>.Ok(await _svc.GetRoutingStagesAsync(TenantId, ct)));

    // UC_MFG_005: Ca sản xuất / năng lực
    [HttpPost("shift-capacities")]
    [AuthorizePermission("mfg.capacity.write")]
    public async Task<ActionResult<ApiResponse<MfgShiftCapacityDto>>> CreateShiftCapacity([FromBody] MfgCreateShiftCapacityRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgShiftCapacityDto>.Ok(await _svc.CreateShiftCapacityAsync(TenantId, req, ct)));

    [HttpGet("shift-capacities")]
    [AuthorizePermission("mfg.capacity.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MfgShiftCapacityDto>>>> GetShiftCapacities(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<MfgShiftCapacityDto>>.Ok(await _svc.GetShiftCapacitiesAsync(TenantId, ct)));
}
