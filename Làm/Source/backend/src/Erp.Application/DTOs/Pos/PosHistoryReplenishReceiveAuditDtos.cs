namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_POS_053: Tra cứu lịch sử mua khách hàng
// ────────────────────────────────────────────────────────────────────────────

public record PosCustomerPurchaseHistoryItemDto(
    Guid OrderId,
    string OrderCode,
    DateTimeOffset PurchasedAt,
    decimal TotalVnd,
    int TotalItemsCount,
    string Status
);

// ────────────────────────────────────────────────────────────────────────────
// UC_POS_056: Tạo đề nghị nhập hàng tại quầy POS
// ────────────────────────────────────────────────────────────────────────────

public record PosReplenishmentLineItemDto(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    int QuantityRequested
);

public record PosCreateReplenishmentRequest(
    string StoreCode,
    string Priority, // Normal | Urgent
    IReadOnlyList<PosReplenishmentLineItemDto> Items
);

public record PosReplenishmentRequestDto(
    Guid Id,
    string RequestNumber,
    string StoreCode,
    string Priority,
    IReadOnlyList<PosReplenishmentLineItemDto> Items,
    string Status,
    DateTimeOffset RequestedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_POS_057: Nhận hàng từ kho trung tâm
// ────────────────────────────────────────────────────────────────────────────

public record PosReceiveTransferShipmentRequest(
    string TransferCode,
    string StoreCode,
    IReadOnlyList<PosReplenishmentLineItemDto> ReceivedItems,
    string Notes
);

public record PosReceiveTransferResultDto(
    Guid ReceiveId,
    string TransferCode,
    string StoreCode,
    int TotalItemsReceived,
    DateTimeOffset ReceivedAt,
    string Status
);

// ────────────────────────────────────────────────────────────────────────────
// UC_POS_058: Kiểm kê nhanh tại cửa hàng
// ────────────────────────────────────────────────────────────────────────────

public record PosQuickAuditLineItemDto(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    int SystemStockQuantity,
    int ActualStockQuantity
);

public record PosSubmitQuickAuditRequest(
    string StoreCode,
    IReadOnlyList<PosQuickAuditLineItemDto> AuditLines
);

public record PosQuickAuditResultDto(
    Guid AuditId,
    string AuditCode,
    string StoreCode,
    int TotalItemsAudited,
    int DiscrepancyCount,
    DateTimeOffset AuditedAt
);
