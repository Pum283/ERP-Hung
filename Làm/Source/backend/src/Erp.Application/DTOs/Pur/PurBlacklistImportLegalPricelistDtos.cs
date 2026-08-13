namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_PUR_006: Blacklist / ngưng dùng nhà cung cấp
// ────────────────────────────────────────────────────────────────────────────

public record PurBlacklistSupplierRequest(
    Guid SupplierId,
    string Reason,
    string SuspensionPeriodMonths
);

public record PurSupplierBlacklistStatusDto(
    Guid SupplierId,
    bool IsBlacklisted,
    string Reason,
    DateTimeOffset BlacklistedAt,
    string Status
);

// ────────────────────────────────────────────────────────────────────────────
// UC_PUR_007: Import danh sách nhà cung cấp
// ────────────────────────────────────────────────────────────────────────────

public record PurImportSupplierRowDto(
    string SupplierCode,
    string SupplierName,
    string TaxCode,
    string Phone,
    string Email,
    string CategoryCode
);

public record PurBatchImportSuppliersRequest(
    IReadOnlyList<PurImportSupplierRowDto> Suppliers
);

public record PurBatchImportSuppliersResultDto(
    int TotalProcessed,
    int TotalSuccess,
    int TotalFailed,
    IReadOnlyList<string> ErrorMessages,
    DateTimeOffset ImportedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_PUR_008: Hồ sơ pháp lý nhà cung cấp
// ────────────────────────────────────────────────────────────────────────────

public record PurSaveSupplierLegalDocumentRequest(
    Guid SupplierId,
    string DocumentType, // BusinessLicense | TaxRegistration | FoodSafetyCert | ISO
    string DocumentNumber,
    DateTimeOffset IssuedDate,
    DateTimeOffset? ExpirationDate,
    string FileUrl
);

public record PurSupplierLegalDocumentDto(
    Guid Id,
    Guid SupplierId,
    string DocumentType,
    string DocumentNumber,
    DateTimeOffset IssuedDate,
    DateTimeOffset? ExpirationDate,
    string FileUrl,
    string Status // Valid | ExpiringSoon | Expired
);

// ────────────────────────────────────────────────────────────────────────────
// UC_PUR_011: Hiệu lực bảng giá mua
// ────────────────────────────────────────────────────────────────────────────

public record PurPricelistItemDto(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    decimal PurchaseUnitPriceVnd
);

public record PurSavePurchasePricelistValidityRequest(
    Guid SupplierId,
    string PricelistCode,
    string PricelistName,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset EffectiveTo,
    IReadOnlyList<PurPricelistItemDto> Items
);

public record PurPurchasePricelistValidityDto(
    Guid Id,
    Guid SupplierId,
    string PricelistCode,
    string PricelistName,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset EffectiveTo,
    bool IsActive,
    IReadOnlyList<PurPricelistItemDto> Items
);
