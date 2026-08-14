namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_INV_068: Báo cáo xuất theo mục đích
// ────────────────────────────────────────────────────────────────────────────

public record InvDispatchPurposeCategoryDto(
    string PurposeCategory,
    int DispatchCount,
    decimal TotalDispatchedValueVnd,
    double ValuePercentage
);

public record InvDispatchPurposeReportSummaryDto(
    int TotalDispatchCount,
    decimal TotalDispatchedValueVnd,
    IReadOnlyList<InvDispatchPurposeCategoryDto> Categories
);
