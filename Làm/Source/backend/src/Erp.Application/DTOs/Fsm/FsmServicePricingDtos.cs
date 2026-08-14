namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_FSM_004: Bảng giá dịch vụ
// ────────────────────────────────────────────────────────────────────────────

public record FsmCreateServicePriceRateRequest(
    string ServiceCode,
    string ServiceName,
    string ServiceCategory,
    decimal BaseHourlyRateVnd,
    decimal StandardTravelFeeVnd,
    decimal EmergencySurchargePct
);

public record FsmServicePriceRateDto(
    Guid Id,
    string ServiceCode,
    string ServiceName,
    string ServiceCategory,
    decimal BaseHourlyRateVnd,
    decimal StandardTravelFeeVnd,
    decimal EmergencySurchargePct,
    bool IsActive
);
