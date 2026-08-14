namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_PUR_044: Tạm ứng nhà cung cấp
// ────────────────────────────────────────────────────────────────────────────

public record PurCreateVendorAdvancePaymentRequest(
    Guid PurchaseOrderId,
    Guid SupplierId,
    decimal AdvanceAmountVnd,
    string PaymentReason
);

public record PurVendorAdvancePaymentDto(
    Guid Id,
    Guid PurchaseOrderId,
    string RequestNumber,
    Guid SupplierId,
    string SupplierName,
    decimal AdvanceAmountVnd,
    string PaymentReason,
    string Status,
    DateTimeOffset RequestedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_PUR_045, UC_PUR_046, UC_PUR_047: Hợp đồng khung, theo dõi giá trị còn lại & Cảnh báo hết hạn
// ────────────────────────────────────────────────────────────────────────────

public record PurCreateBlanketContractRequest(
    string ContractNumber,
    string ContractTitle,
    Guid SupplierId,
    decimal TotalContractValueVnd,
    int TotalContractQty,
    DateTimeOffset StartDate,
    DateTimeOffset ExpirationDate
);

public record PurBlanketContractDto(
    Guid Id,
    string ContractNumber,
    string ContractTitle,
    Guid SupplierId,
    string SupplierName,
    decimal TotalContractValueVnd,
    decimal ConsumedValueVnd,
    decimal RemainingValueVnd,
    int TotalContractQty,
    int ConsumedQty,
    int RemainingQty,
    double ConsumedPercentage,
    DateTimeOffset StartDate,
    DateTimeOffset ExpirationDate,
    int DaysUntilExpiration,
    bool IsExpiringSoon,
    string Status
);
