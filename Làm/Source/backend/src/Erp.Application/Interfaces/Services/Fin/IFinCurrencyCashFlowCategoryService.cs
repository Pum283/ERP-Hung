using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IFinCurrencyCashFlowCategoryService
{
    // UC_FIN_005: Đồng tiền hạch toán & tỷ giá
    Task<FinCurrencyExchangeRateDto> CreateExchangeRateAsync(Guid tenantId, FinCreateExchangeRateRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<FinCurrencyExchangeRateDto>> GetExchangeRatesAsync(Guid tenantId, CancellationToken ct = default);

    // UC_FIN_007: Khoản mục thu/chi
    Task<FinCashFlowCategoryDto> CreateCashFlowCategoryAsync(Guid tenantId, FinCreateCashFlowCategoryRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<FinCashFlowCategoryDto>> GetCashFlowCategoriesAsync(Guid tenantId, CancellationToken ct = default);
}
