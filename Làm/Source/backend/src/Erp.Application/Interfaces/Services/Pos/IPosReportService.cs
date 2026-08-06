using Erp.Application.DTOs.Pos;

namespace Erp.Application.Interfaces.Services.Pos;

public interface IPosReportService
{
    Task<IReadOnlyList<PosRevenueByTimeRowDto>> RevenueByTimeAsync(
        Guid tenantId, DateTimeOffset from, DateTimeOffset to, string grain,
        Guid? storeId = null, CancellationToken ct = default);

    Task<IReadOnlyList<PosRevenueByProductRowDto>> RevenueByProductAsync(
        Guid tenantId, DateTimeOffset from, DateTimeOffset to,
        Guid? storeId = null, CancellationToken ct = default);

    Task<IReadOnlyList<PosRevenueByCashierRowDto>> RevenueByCashierAsync(
        Guid tenantId, DateTimeOffset from, DateTimeOffset to,
        Guid? storeId = null, CancellationToken ct = default);

    Task<PosCancelDiscountReportDto> CancelDiscountRatesAsync(
        Guid tenantId, DateTimeOffset from, DateTimeOffset to,
        Guid? storeId = null, CancellationToken ct = default);

    /// <summary>UC_POS_066 — top SP bán chạy theo qty | revenue.</summary>
    Task<IReadOnlyList<PosTopProductRowDto>> TopProductsAsync(
        Guid tenantId, DateTimeOffset from, DateTimeOffset to,
        int top = 10, string by = "qty", Guid? storeId = null, CancellationToken ct = default);

    /// <summary>UC_POS_067 — so sánh doanh thu giữa các điểm bán.</summary>
    Task<IReadOnlyList<PosStoreCompareRowDto>> CompareStoresAsync(
        Guid tenantId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);

    /// <summary>UC_POS_069/072 — DT chuỗi realtime hôm nay + lũy kế tháng vs target.</summary>
    Task<PosChainLiveReportDto> ChainLiveAsync(
        Guid tenantId, DateTimeOffset? asOf = null, CancellationToken ct = default);

    /// <summary>UC_POS_065 — cost lý thuyết (BOM × StandardCost) vs thực tế (INV Issue POS).</summary>
    Task<PosCostVarianceReportDto> CostVarianceAsync(
        Guid tenantId, DateTimeOffset from, DateTimeOffset to,
        Guid? storeId = null, CancellationToken ct = default);

    Task<string> ExportCsvAsync(
        Guid tenantId, string report, DateTimeOffset from, DateTimeOffset to,
        string? grain = null, Guid? storeId = null, CancellationToken ct = default);
}
