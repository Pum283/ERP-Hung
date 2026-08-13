namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_PUR_022: Nhập báo giá từ nhà cung cấp
// ────────────────────────────────────────────────────────────────────────────

public record PurQuotationLineItemDto(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    int Quantity,
    decimal UnitPriceVnd
);

public record PurSubmitVendorQuotationRequest(
    Guid RfqId,
    Guid SupplierId,
    string QuotationNumber,
    int DeliveryLeadTimeDays,
    string PaymentTerms,
    IReadOnlyList<PurQuotationLineItemDto> Items
);

public record PurVendorQuotationDto(
    Guid Id,
    Guid RfqId,
    Guid SupplierId,
    string SupplierName,
    string QuotationNumber,
    decimal TotalAmountVnd,
    int DeliveryLeadTimeDays,
    string PaymentTerms,
    bool IsAwardedWinner,
    IReadOnlyList<PurQuotationLineItemDto> Items,
    DateTimeOffset ReceivedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_PUR_023: So sánh giá / điều kiện & UC_PUR_024: Chọn nhà cung cấp thắng
// ────────────────────────────────────────────────────────────────────────────

public record PurAwardQuotationWinnerRequest(
    Guid QuotationId,
    string AwardReason
);

public record PurAwardQuotationWinnerResultDto(
    Guid QuotationId,
    Guid RfqId,
    Guid SupplierId,
    bool IsWinner,
    string Status,
    DateTimeOffset AwardedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_PUR_029: Xác nhận PO từ nhà cung cấp
// ────────────────────────────────────────────────────────────────────────────

public record PurConfirmVendorPoRequest(
    Guid PurchaseOrderId,
    string PoNumber,
    Guid SupplierId,
    string ConfirmationStatus, // Confirmed | ConfirmedWithChanges | Rejected
    DateTimeOffset PromisedDeliveryDate,
    string VendorComments
);

public record PurVendorPoConfirmationDto(
    Guid Id,
    Guid PurchaseOrderId,
    string PoNumber,
    Guid SupplierId,
    string ConfirmationStatus,
    DateTimeOffset PromisedDeliveryDate,
    string VendorComments,
    DateTimeOffset ConfirmedAt
);
