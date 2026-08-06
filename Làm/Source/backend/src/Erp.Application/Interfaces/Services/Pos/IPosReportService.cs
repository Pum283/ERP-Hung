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

    Task<string> ExportCsvAsync(
        Guid tenantId, string report, DateTimeOffset from, DateTimeOffset to,
        string? grain = null, Guid? storeId = null, CancellationToken ct = default);
}
