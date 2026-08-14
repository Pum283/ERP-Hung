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
[Route("api/mfg/schedule-progress-rework-cost")]
public sealed class MfgScheduleProgressReworkCostController : ControllerBase
{
    private readonly IMfgScheduleProgressReworkCostService _svc;

    public MfgScheduleProgressReworkCostController(IMfgScheduleProgressReworkCostService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_MFG_016: Lịch SX theo xưởng/ca
    [HttpPost("schedules")]
    [AuthorizePermission("mfg.schedule.write")]
    public async Task<ActionResult<ApiResponse<MfgWorkshopShiftScheduleDto>>> CreateWorkshopShiftSchedule([FromBody] MfgCreateWorkshopShiftScheduleRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgWorkshopShiftScheduleDto>.Ok(await _svc.CreateWorkshopShiftScheduleAsync(TenantId, req, ct)));

    [HttpGet("schedules")]
    [AuthorizePermission("mfg.schedule.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MfgWorkshopShiftScheduleDto>>>> GetSchedules(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<MfgWorkshopShiftScheduleDto>>.Ok(await _svc.GetSchedulesAsync(TenantId, ct)));

    // UC_MFG_021: Ghi nhận tiến độ công đoạn
    [HttpPost("operation-progress")]
    [AuthorizePermission("mfg.progress.write")]
    public async Task<ActionResult<ApiResponse<MfgOperationProgressTrackingDto>>> LogOperationProgress([FromBody] MfgLogOperationProgressRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgOperationProgressTrackingDto>.Ok(await _svc.LogOperationProgressAsync(TenantId, req, ct)));

    // UC_MFG_026: Lệnh sản xuất lại
    [HttpPost("rework-work-orders")]
    [AuthorizePermission("mfg.rework.write")]
    public async Task<ActionResult<ApiResponse<MfgReworkWorkOrderDto>>> CreateReworkWorkOrder([FromBody] MfgCreateReworkWorkOrderRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgReworkWorkOrderDto>.Ok(await _svc.CreateReworkWorkOrderAsync(TenantId, req, ct)));

    // UC_MFG_028: Phân bổ nhân công / chi phí chung
    [HttpPost("overhead-cost-allocations")]
    [AuthorizePermission("mfg.cost.write")]
    public async Task<ActionResult<ApiResponse<MfgOverheadCostAllocationDto>>> AllocateOverheadCost([FromBody] MfgAllocateOverheadCostRequest req, CancellationToken ct)
        => Ok(ApiResponse<MfgOverheadCostAllocationDto>.Ok(await _svc.AllocateOverheadCostAsync(TenantId, req, ct)));
}
