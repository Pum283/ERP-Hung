namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_POS_017 & UC_POS_018: Giá theo khung giờ & ngày trong tuần
// ────────────────────────────────────────────────────────────────────────────

public record PosSaveTimeSlotPriceRuleRequest(
    string RuleName,
    Guid ProductId,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string DaysOfWeek, // Monday,Tuesday,Wednesday,Thursday,Friday,Saturday,Sunday
    decimal SpecialPriceVnd,
    double DiscountPercent
);

public record PosTimeSlotPriceRuleDto(
    Guid Id,
    string RuleName,
    Guid ProductId,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string DaysOfWeek,
    decimal SpecialPriceVnd,
    double DiscountPercent,
    bool IsActive
);

// ────────────────────────────────────────────────────────────────────────────
// UC_POS_020: Làm tròn tiền thanh toán
// ────────────────────────────────────────────────────────────────────────────

public record PosCashRoundingCalculationDto(
    decimal OriginalTotalVnd,
    int RoundingInterval, // 500 | 1000
    decimal RoundedTotalVnd,
    decimal RoundingDifferenceVnd
);

// ────────────────────────────────────────────────────────────────────────────
// UC_POS_023: Khuyến mại theo combo
// ────────────────────────────────────────────────────────────────────────────

public record PosSaveComboPromotionRuleRequest(
    string ComboCode,
    string ComboName,
    IReadOnlyList<Guid> ProductIds,
    decimal FixedComboPriceVnd,
    DateTime StartDate,
    DateTime EndDate
);

public record PosComboPromotionRuleDto(
    Guid Id,
    string ComboCode,
    string ComboName,
    IReadOnlyList<Guid> ProductIds,
    decimal FixedComboPriceVnd,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive
);
