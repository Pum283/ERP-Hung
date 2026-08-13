namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_POS_025: Báo cáo khuyến mại POS
// ────────────────────────────────────────────────────────────────────────────

public record PosPromotionUsageSummaryDto(
    string RuleCode,
    string RuleName,
    int TotalTimesUsed,
    decimal TotalDiscountValueVnd,
    decimal RevenueGeneratedVnd
);

public record PosPromotionReportAnalyticsDto(
    int TotalPromotionsApplied,
    decimal TotalDiscountGrantedVnd,
    IReadOnlyList<PosPromotionUsageSummaryDto> UsageDetails
);

// ────────────────────────────────────────────────────────────────────────────
// UC_POS_028: Tách bill / gộp bill
// ────────────────────────────────────────────────────────────────────────────

public record PosSplitBillRequest(
    Guid SourceOrderId,
    IReadOnlyList<Guid> SplitItemIds,
    string Reason
);

public record PosMergeBillRequest(
    Guid PrimaryOrderId,
    IReadOnlyList<Guid> MergedOrderIds,
    string Reason
);

public record PosBillOperationResultDto(
    Guid OperationId,
    Guid PrimaryOrderId,
    string OperationType, // Split | Merge
    int TotalItemsAffected,
    DateTimeOffset OperationTime,
    string Message
);

// ────────────────────────────────────────────────────────────────────────────
// UC_POS_029: Chuyển đơn giữa quầy
// ────────────────────────────────────────────────────────────────────────────

public record PosTransferOrderRequest(
    Guid OrderId,
    string FromCounterCode,
    string ToCounterCode,
    string Notes
);

public record PosOrderTransferResultDto(
    Guid TransferId,
    Guid OrderId,
    string FromCounterCode,
    string ToCounterCode,
    DateTimeOffset TransferredAt,
    string Status
);

// ────────────────────────────────────────────────────────────────────────────
// UC_POS_030: Ghi chú đơn hàng & Bếp
// ────────────────────────────────────────────────────────────────────────────

public record PosUpdateOrderNotesRequest(
    Guid OrderId,
    string CustomerNotes,
    string KitchenSpecialInstructions
);

public record PosOrderNotesDto(
    Guid OrderId,
    string CustomerNotes,
    string KitchenSpecialInstructions,
    DateTimeOffset UpdatedAt
);
