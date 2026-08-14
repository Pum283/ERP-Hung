namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_FIN_005: Đồng tiền hạch toán & tỷ giá
// ────────────────────────────────────────────────────────────────────────────

public record FinCreateExchangeRateRequest(
    string CurrencyCode,
    string CurrencyName,
    decimal ExchangeRateToVnd,
    string RateSource,
    bool IsBaseCurrency,
    DateTimeOffset EffectiveDate
);

public record FinCurrencyExchangeRateDto(
    Guid Id,
    string CurrencyCode,
    string CurrencyName,
    decimal ExchangeRateToVnd,
    string RateSource,
    bool IsBaseCurrency,
    DateTimeOffset EffectiveDate
);

// ────────────────────────────────────────────────────────────────────────────
// UC_FIN_007: Khoản mục thu/chi
// ────────────────────────────────────────────────────────────────────────────

public record FinCreateCashFlowCategoryRequest(
    string CategoryCode,
    string CategoryName,
    string CashFlowType,
    string SectionCode,
    bool IsActive
);

public record FinCashFlowCategoryDto(
    Guid Id,
    string CategoryCode,
    string CategoryName,
    string CashFlowType,
    string SectionCode,
    bool IsActive
);
