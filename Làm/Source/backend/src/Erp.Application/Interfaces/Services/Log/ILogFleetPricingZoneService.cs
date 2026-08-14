using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface ILogFleetPricingZoneService
{
    // UC_LOG_002: Danh mục tài xế / xe
    Task<LogDriverVehicleDto> CreateDriverVehicleAsync(Guid tenantId, LogCreateDriverVehicleRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<LogDriverVehicleDto>> GetDriverVehiclesAsync(Guid tenantId, CancellationToken ct = default);

    // UC_LOG_003: Bảng giá cước vận chuyển
    Task<LogFreightPricingRateDto> CreateFreightPricingRateAsync(Guid tenantId, LogCreateFreightPricingRateRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<LogFreightPricingRateDto>> GetFreightPricingRatesAsync(Guid tenantId, CancellationToken ct = default);

    // UC_LOG_004: Cấu hình khu vực giao
    Task<LogDeliveryZoneConfigDto> CreateDeliveryZoneConfigAsync(Guid tenantId, LogCreateDeliveryZoneConfigRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<LogDeliveryZoneConfigDto>> GetDeliveryZoneConfigsAsync(Guid tenantId, CancellationToken ct = default);
}
