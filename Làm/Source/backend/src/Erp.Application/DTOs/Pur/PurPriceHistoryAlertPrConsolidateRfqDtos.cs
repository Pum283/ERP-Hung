namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_PUR_012: Lịch sử giá mua & UC_PUR_013: Cảnh báo tăng giá bất thường
// ────────────────────────────────────────────────────────────────────────────

public record PurPriceHistoryItemDto(
    Guid Id,
    Guid ProductId,
    string ProductCode,
    string ProductName,
    Guid SupplierId,
    string SupplierName,
    decimal UnitPriceVnd,
    decimal PreviousUnitPriceVnd,
    double ChangePercentage,
    bool IsAbnormalSpike, // True if change >= 10%
    DateTimeOffset EffectiveDate
);

// ────────────────────────────────────────────────────────────────────────────
// UC_PUR_016: Gộp nhiều nhu cầu thành PR
// ────────────────────────────────────────────────────────────────────────────

public record PurDemandLineDto(
    Guid DemandId,
    string DepartmentName,
    Guid ProductId,
    string ProductCode,
    string ProductName,
    int QuantityRequested
);

public record PurConsolidateDemandsToPrRequest(
    string PrTitle,
    IReadOnlyList<PurDemandLineDto> DemandLines
);

public record PurConsolidatedPrResultDto(
    Guid PrId,
    string PrNumber,
    string PrTitle,
    int TotalItemsCount,
    int TotalQuantity,
    DateTimeOffset ConsolidatedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_PUR_021: Tạo RFQ gửi nhiều nhà cung cấp
// ────────────────────────────────────────────────────────────────────────────

public record PurRfqItemDto(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    int Quantity
);

public record PurCreateMultiSupplierRfqRequest(
    string Title,
    IReadOnlyList<Guid> SupplierIds,
    IReadOnlyList<PurRfqItemDto> Items,
    DateTimeOffset DeadlineDate
);

public record PurMultiSupplierRfqDto(
    Guid Id,
    string RfqNumber,
    string Title,
    int TotalSuppliersCount,
    int TotalItemsCount,
    DateTimeOffset DeadlineDate,
    string Status,
    DateTimeOffset CreatedAt
);
