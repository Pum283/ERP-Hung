namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_MFG_004: Danh mục công đoạn
// ────────────────────────────────────────────────────────────────────────────

public record MfgCreateRoutingStageRequest(
    string StageCode,
    string StageName,
    string WorkCenterCode,
    decimal StandardCycleTimeMinutes,
    decimal StandardSetupTimeMinutes,
    bool IsOutsourced
);

public record MfgRoutingStageDto(
    Guid Id,
    string StageCode,
    string StageName,
    string WorkCenterCode,
    decimal StandardCycleTimeMinutes,
    decimal StandardSetupTimeMinutes,
    bool IsOutsourced,
    bool IsActive
);

// ────────────────────────────────────────────────────────────────────────────
// UC_MFG_005: Ca sản xuất / năng lực
// ────────────────────────────────────────────────────────────────────────────

public record MfgCreateShiftCapacityRequest(
    string ShiftCode,
    string ShiftName,
    string WorkCenterCode,
    decimal AvailableHoursPerShift,
    decimal EfficiencyFactorPct,
    decimal MaxCapacityOutputUnits
);

public record MfgShiftCapacityDto(
    Guid Id,
    string ShiftCode,
    string ShiftName,
    string WorkCenterCode,
    decimal AvailableHoursPerShift,
    decimal EfficiencyFactorPct,
    decimal MaxCapacityOutputUnits,
    bool IsActive
);
