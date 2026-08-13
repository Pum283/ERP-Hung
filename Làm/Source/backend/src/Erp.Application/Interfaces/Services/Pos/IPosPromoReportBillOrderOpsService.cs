using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IPosPromoReportBillOrderOpsService
{
    // UC_POS_025: Báo cáo khuyến mại
    Task<PosPromotionReportAnalyticsDto> GetPromotionReportAnalyticsAsync(Guid tenantId, CancellationToken ct = default);

    // UC_POS_028: Tách bill / gộp bill
    Task<PosBillOperationResultDto> SplitBillAsync(Guid tenantId, Guid userId, PosSplitBillRequest req, CancellationToken ct = default);
    Task<PosBillOperationResultDto> MergeBillAsync(Guid tenantId, Guid userId, PosMergeBillRequest req, CancellationToken ct = default);

    // UC_POS_029: Chuyển đơn giữa quầy
    Task<PosOrderTransferResultDto> TransferOrderCounterAsync(Guid tenantId, Guid userId, PosTransferOrderRequest req, CancellationToken ct = default);

    // UC_POS_030: Ghi chú đơn hàng & Bếp
    Task<PosOrderNotesDto> UpdateOrderNotesAsync(Guid tenantId, PosUpdateOrderNotesRequest req, CancellationToken ct = default);
}
