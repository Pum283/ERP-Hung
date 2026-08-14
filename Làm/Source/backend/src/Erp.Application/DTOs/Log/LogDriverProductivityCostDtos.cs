namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_LOG_036: Năng suất tài xế / chuyến
// ────────────────────────────────────────────────────────────────────────────

public record LogDriverProductivityItemDto(
    Guid DriverVehicleId,
    string DriverName,
    int CompletedTripsCount,
    int DeliveredOrdersCount,
    decimal TotalWeightDeliveredKg,
    double OnTimeDeliveryRatePct
);

public record LogDriverProductivitySummaryDto(
    int TotalActiveDrivers,
    int TotalCompletedTrips,
    IReadOnlyList<LogDriverProductivityItemDto> Drivers
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LOG_037: Chi phí vận chuyển
// ────────────────────────────────────────────────────────────────────────────

public record LogCalculateTripCostRequest(
    string TripNumber,
    decimal TotalFuelCostVnd,
    decimal TotalTollFeeVnd,
    decimal DriverAllowanceVnd,
    int AllocatedOrdersCount
);

public record LogShippingCostAllocationDto(
    Guid Id,
    string CostAllocationNumber,
    string TripNumber,
    decimal TotalFuelCostVnd,
    decimal TotalTollFeeVnd,
    decimal DriverAllowanceVnd,
    decimal TotalTripCostVnd,
    int AllocatedOrdersCount,
    decimal AverageCostPerOrderVnd,
    DateTimeOffset CalculatedAt
);
