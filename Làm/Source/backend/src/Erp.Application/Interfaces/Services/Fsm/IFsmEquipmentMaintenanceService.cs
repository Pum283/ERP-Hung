using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IFsmEquipmentMaintenanceService
{
    // UC_FSM_033: Lịch bảo trì theo thiết bị
    Task<FsmEquipmentMaintenanceScheduleDto> CreateMaintenanceScheduleAsync(Guid tenantId, FsmCreateMaintenanceScheduleRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<FsmEquipmentMaintenanceScheduleDto>> GetMaintenanceSchedulesAsync(Guid tenantId, CancellationToken ct = default);

    // UC_FSM_034: Tự tạo ticket bảo trì đến hạn
    Task<FsmAutoDueMaintenanceTicketDto> GenerateDueTicketAsync(Guid tenantId, Guid scheduleId, CancellationToken ct = default);

    // UC_FSM_035: Checklist bảo trì chuẩn
    Task<FsmStandardMaintenanceChecklistDto> CreateStandardChecklistAsync(Guid tenantId, FsmCreateStandardChecklistItemRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<FsmStandardMaintenanceChecklistDto>> GetStandardChecklistsAsync(Guid tenantId, string category, CancellationToken ct = default);

    // UC_FSM_036: Báo cáo thực hiện bảo trì
    Task<FsmMaintenanceExecutionReportDto> GetMaintenanceExecutionReportAsync(Guid tenantId, CancellationToken ct = default);
}
