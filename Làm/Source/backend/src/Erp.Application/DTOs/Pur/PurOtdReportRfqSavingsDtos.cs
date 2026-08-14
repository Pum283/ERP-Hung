namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_PUR_049: Báo cáo đúng hạn giao hàng (OTD Performance)
// ────────────────────────────────────────────────────────────────────────────

public record PurVendorOtdPerformanceDto(
    Guid SupplierId,
    string SupplierName,
    int TotalOrdersCount,
    int OnTimeOrdersCount,
    int LateOrdersCount,
    double OnTimeDeliveryPercentage,
    string PerformanceRating // Excellent (>=95%) | Good (>=85%) | Poor (<85%)
);

// ────────────────────────────────────────────────────────────────────────────
// UC_PUR_050: Báo cáo tiết kiệm chi phí từ RFQ
// ────────────────────────────────────────────────────────────────────────────

public record PurRfqSavingsItemDto(
    Guid RfqId,
    string RfqNumber,
    string Title,
    decimal InitialBudgetVnd,
    decimal AwardedAmountVnd,
    decimal SavingsAmountVnd,
    double SavingsPercentage,
    DateTimeOffset CalculatedAt
);

public record PurRfqSavingsSummaryDto(
    decimal TotalBudgetVnd,
    decimal TotalAwardedVnd,
    decimal TotalSavingsVnd,
    double OverallSavingsPercentage,
    IReadOnlyList<PurRfqSavingsItemDto> SavingsList
);
