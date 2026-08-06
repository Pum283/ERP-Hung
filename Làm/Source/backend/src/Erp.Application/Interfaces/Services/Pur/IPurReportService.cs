using Erp.Application.DTOs.Pur;

namespace Erp.Application.Interfaces.Services.Pur;

public interface IPurReportService
{
    Task<IReadOnlyList<PurPurchaseByVendorRowDto>> PurchaseByVendorAsync(
        Guid tenantId, DateTimeOffset from, DateTimeOffset to, Guid? vendorId = null, CancellationToken ct = default);

    Task<IReadOnlyList<PurPurchaseByProductRowDto>> PurchaseByProductAsync(
        Guid tenantId, DateTimeOffset from, DateTimeOffset to, Guid? vendorId = null, CancellationToken ct = default);

    Task<IReadOnlyList<PurOpenPrAgingRowDto>> OpenPrAgingAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<PurOpenPoAgingRowDto>> OpenPoAgingAsync(Guid tenantId, Guid? vendorId = null, CancellationToken ct = default);

    Task<string> ExportCsvAsync(
        Guid tenantId, string report, DateTimeOffset? from = null, DateTimeOffset? to = null,
        Guid? vendorId = null, CancellationToken ct = default);
}
