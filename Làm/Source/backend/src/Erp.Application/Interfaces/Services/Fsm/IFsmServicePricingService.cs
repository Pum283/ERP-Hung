using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IFsmServicePricingService
{
    // UC_FSM_004: Bảng giá dịch vụ
    Task<FsmServicePriceRateDto> CreateServicePriceRateAsync(Guid tenantId, FsmCreateServicePriceRateRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<FsmServicePriceRateDto>> GetServicePriceRatesAsync(Guid tenantId, CancellationToken ct = default);
}
