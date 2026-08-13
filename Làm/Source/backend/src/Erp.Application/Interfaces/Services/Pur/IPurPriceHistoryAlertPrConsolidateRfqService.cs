using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IPurPriceHistoryAlertPrConsolidateRfqService
{
    // UC_PUR_012 & UC_PUR_013: Lịch sử giá mua & Cảnh báo tăng giá
    Task<IReadOnlyList<PurPriceHistoryItemDto>> GetPurchasePriceHistoryAsync(Guid tenantId, Guid? productId, Guid? supplierId, CancellationToken ct = default);

    // UC_PUR_016: Gộp nhiều nhu cầu thành PR
    Task<PurConsolidatedPrResultDto> ConsolidateDemandsToPrAsync(Guid tenantId, Guid userId, PurConsolidateDemandsToPrRequest req, CancellationToken ct = default);

    // UC_PUR_021: Tạo RFQ gửi nhiều nhà cung cấp
    Task<PurMultiSupplierRfqDto> CreateMultiSupplierRfqAsync(Guid tenantId, Guid userId, PurCreateMultiSupplierRfqRequest req, CancellationToken ct = default);
}
