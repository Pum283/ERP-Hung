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
[Route("api/fsm/equipment-maintenance")]
public sealed class FsmEquipmentMaintenanceController : ControllerBase
{
    private readonly IFsmEquipmentMaintenanceService _svc;

    public FsmEquipmentMaintenanceController(IFsmEquipmentMaintenanceService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_FSM_033: Lịch bảo trì theo thiết bị
    [HttpPost("schedules")]
    [AuthorizePermission("fsm.maint.write")]
    public async Task<ActionResult<ApiResponse<FsmEquipmentMaintenanceScheduleDto>>> CreateMaintenanceSchedule([FromBody] FsmCreateMaintenanceScheduleRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmEquipmentMaintenanceScheduleDto>.Ok(await _svc.CreateMaintenanceScheduleAsync(TenantId, req, ct)));

    [HttpGet("schedules")]
    [AuthorizePermission("fsm.maint.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FsmEquipmentMaintenanceScheduleDto>>>> GetMaintenanceSchedules(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FsmEquipmentMaintenanceScheduleDto>>.Ok(await _svc.GetMaintenanceSchedulesAsync(TenantId, ct)));

    // UC_FSM_034: Tự tạo ticket bảo trì đến hạn
    [HttpPost("generate-due-ticket/{scheduleId:guid}")]
    [AuthorizePermission("fsm.maint.write")]
    public async Task<ActionResult<ApiResponse<FsmAutoDueMaintenanceTicketDto>>> GenerateDueTicket(Guid scheduleId, CancellationToken ct)
        => Ok(ApiResponse<FsmAutoDueMaintenanceTicketDto>.Ok(await _svc.GenerateDueTicketAsync(TenantId, scheduleId, ct)));

    // UC_FSM_035: Checklist bảo trì chuẩn
    [HttpPost("standard-checklists")]
    [AuthorizePermission("fsm.maint.write")]
    public async Task<ActionResult<ApiResponse<FsmStandardMaintenanceChecklistDto>>> CreateStandardChecklist([FromBody] FsmCreateStandardChecklistItemRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmStandardMaintenanceChecklistDto>.Ok(await _svc.CreateStandardChecklistAsync(TenantId, req, ct)));

    [HttpGet("standard-checklists")]
    [AuthorizePermission("fsm.maint.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FsmStandardMaintenanceChecklistDto>>>> GetStandardChecklists([FromQuery] string? category, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FsmStandardMaintenanceChecklistDto>>.Ok(await _svc.GetStandardChecklistsAsync(TenantId, category ?? "", ct)));

    // UC_FSM_036: Báo cáo thực hiện bảo trì
    [HttpGet("execution-report")]
    [AuthorizePermission("fsm.maint.read")]
    public async Task<ActionResult<ApiResponse<FsmMaintenanceExecutionReportDto>>> GetMaintenanceExecutionReport(CancellationToken ct)
        => Ok(ApiResponse<FsmMaintenanceExecutionReportDto>.Ok(await _svc.GetMaintenanceExecutionReportAsync(TenantId, ct)));
}
