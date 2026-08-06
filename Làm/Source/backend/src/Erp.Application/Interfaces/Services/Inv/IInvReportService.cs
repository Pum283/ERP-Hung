using Erp.Application.DTOs.Inv;

namespace Erp.Application.Interfaces.Services.Inv;

public interface IInvReportService
{
    Task<IReadOnlyList<InvStockValueRowDto>> StockValueAsync(
        Guid tenantId, Guid? warehouseId = null, CancellationToken ct = default);

    Task<IReadOnlyList<InvMovementPeriodRowDto>> MovementByPeriodAsync(
        Guid tenantId, DateTimeOffset from, DateTimeOffset to,
        Guid? warehouseId = null, CancellationToken ct = default);

    Task<IReadOnlyList<InvSkuCardLineDto>> SkuCardAsync(
        Guid tenantId, Guid skuId, Guid? warehouseId = null,
        DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken ct = default);

    Task<IReadOnlyList<InvMinMaxAlertRowDto>> MinMaxAlertsAsync(
        Guid tenantId, Guid? warehouseId = null, CancellationToken ct = default);

    Task<IReadOnlyList<InvStocktakeReportRowDto>> StocktakeResultAsync(
        Guid tenantId, Guid? stocktakeId = null, Guid? warehouseId = null, CancellationToken ct = default);

    Task<InvDashboardDto> DashboardAsync(Guid tenantId, Guid? warehouseId = null, CancellationToken ct = default);

    Task<IReadOnlyList<InvNearExpiryRowDto>> NearExpiryAsync(
        Guid tenantId, int withinDays = 30, Guid? warehouseId = null, CancellationToken ct = default);

    Task<string> ExportCsvAsync(
        Guid tenantId, string report, Guid? warehouseId = null, Guid? skuId = null,
        Guid? stocktakeId = null, DateTimeOffset? from = null, DateTimeOffset? to = null,
        int? withinDays = null, CancellationToken ct = default);
}
