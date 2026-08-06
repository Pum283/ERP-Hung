using Erp.Application.DTOs.Pos;

namespace Erp.Application.Interfaces.Services.Pos;

public interface IPosConfigService
{
    Task<IReadOnlyList<PosStoreDto>> ListStoresAsync(Guid tenantId, CancellationToken ct = default);
    Task<PosStoreDto> UpsertStoreAsync(Guid tenantId, Guid userId, PosStoreUpsertRequest req, CancellationToken ct = default);
    Task<PosStoreDetailDto> GetStoreDetailAsync(Guid tenantId, Guid storeId, CancellationToken ct = default);
    Task<PosTerminalDto> UpsertTerminalAsync(Guid tenantId, Guid userId, Guid storeId, PosTerminalUpsertRequest req, CancellationToken ct = default);
    Task<PosPrinterDto> UpsertPrinterAsync(Guid tenantId, Guid userId, Guid storeId, PosPrinterUpsertRequest req, CancellationToken ct = default);
    Task<PosCashierDto> UpsertCashierAsync(Guid tenantId, Guid userId, Guid storeId, PosCashierUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<PosCategoryDto>> ListCategoriesAsync(Guid tenantId, CancellationToken ct = default);
    Task<PosCategoryDto> UpsertCategoryAsync(Guid tenantId, Guid userId, PosCategoryUpsertRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PosProductDto>> ListProductsAsync(Guid tenantId, string? q, CancellationToken ct = default);
    Task<PosProductDto> UpsertProductAsync(Guid tenantId, Guid userId, PosProductUpsertRequest req, CancellationToken ct = default);
    Task<PosProductDto> SetProductStatusAsync(Guid tenantId, Guid userId, Guid productId, string status, CancellationToken ct = default);
    Task<IReadOnlyList<PosBomLineDto>> ListBomAsync(Guid tenantId, Guid productId, CancellationToken ct = default);
    Task<PosBomLineDto> UpsertBomAsync(Guid tenantId, Guid userId, Guid productId, PosBomLineUpsertRequest req, CancellationToken ct = default);
    Task<PosSyncResult> SyncCatalogAsync(Guid tenantId, Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<PosTaxRateDto>> ListTaxRatesAsync(Guid tenantId, CancellationToken ct = default);
    Task<PosTaxRateDto> UpsertTaxRateAsync(Guid tenantId, Guid userId, PosTaxRateUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<PosPriceListDto>> ListPriceListsAsync(Guid tenantId, CancellationToken ct = default);
    Task<PosPriceListDto> UpsertPriceListAsync(Guid tenantId, Guid userId, PosPriceListUpsertRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PosPriceItemDto>> ListPriceItemsAsync(Guid tenantId, Guid priceListId, CancellationToken ct = default);
    Task<PosPriceItemDto> UpsertPriceItemAsync(Guid tenantId, Guid userId, Guid priceListId, PosPriceItemUpsertRequest req, CancellationToken ct = default);
}
