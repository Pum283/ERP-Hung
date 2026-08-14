using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IMfgScheduleProgressReworkCostService
{
    // UC_MFG_016: Lịch SX theo xưởng/ca
    Task<MfgWorkshopShiftScheduleDto> CreateWorkshopShiftScheduleAsync(Guid tenantId, MfgCreateWorkshopShiftScheduleRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<MfgWorkshopShiftScheduleDto>> GetSchedulesAsync(Guid tenantId, CancellationToken ct = default);

    // UC_MFG_021: Ghi nhận tiến độ công đoạn
    Task<MfgOperationProgressTrackingDto> LogOperationProgressAsync(Guid tenantId, MfgLogOperationProgressRequest req, CancellationToken ct = default);

    // UC_MFG_026: Lệnh sản xuất lại
    Task<MfgReworkWorkOrderDto> CreateReworkWorkOrderAsync(Guid tenantId, MfgCreateReworkWorkOrderRequest req, CancellationToken ct = default);

    // UC_MFG_028: Phân bổ nhân công / chi phí chung
    Task<MfgOverheadCostAllocationDto> AllocateOverheadCostAsync(Guid tenantId, MfgAllocateOverheadCostRequest req, CancellationToken ct = default);
}
