using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IMfgRoutingStageShiftCapacityService
{
    // UC_MFG_004: Danh mục công đoạn
    Task<MfgRoutingStageDto> CreateRoutingStageAsync(Guid tenantId, MfgCreateRoutingStageRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<MfgRoutingStageDto>> GetRoutingStagesAsync(Guid tenantId, CancellationToken ct = default);

    // UC_MFG_005: Ca sản xuất / năng lực
    Task<MfgShiftCapacityDto> CreateShiftCapacityAsync(Guid tenantId, MfgCreateShiftCapacityRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<MfgShiftCapacityDto>> GetShiftCapacitiesAsync(Guid tenantId, CancellationToken ct = default);
}
