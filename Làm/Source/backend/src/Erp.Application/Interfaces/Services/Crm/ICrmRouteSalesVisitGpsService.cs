using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface ICrmRouteSalesVisitGpsService
{
    // UC_CRM_089: Phân vùng / tuyến bán hàng
    Task<CrmTerritoryDto> CreateTerritoryAsync(Guid tenantId, CrmCreateTerritoryRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<CrmTerritoryDto>> GetTerritoriesAsync(Guid tenantId, CancellationToken ct = default);

    // UC_CRM_090: Phân loại tần suất visit
    Task<CrmVisitFrequencyDto> ClassifyFrequencyAsync(Guid tenantId, CrmClassifyFrequencyRequest req, CancellationToken ct = default);

    // UC_CRM_091: Lập kế hoạch visit
    Task<CrmVisitPlanDto> CreateVisitPlanAsync(Guid tenantId, CrmCreateVisitPlanRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<CrmVisitPlanDto>> GetVisitPlansAsync(Guid tenantId, DateTime? date = null, CancellationToken ct = default);

    // UC_CRM_092: Check-in / check-out GPS
    Task<CrmGpsCheckResultDto> CheckInGpsAsync(Guid tenantId, CrmGpsCheckInRequest req, CancellationToken ct = default);
    Task<CrmGpsCheckResultDto> CheckOutGpsAsync(Guid tenantId, CrmGpsCheckOutRequest req, CancellationToken ct = default);
}
