namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_LOG_005: Cấu hình ca giao hàng
// ────────────────────────────────────────────────────────────────────────────

public record LogCreateDeliveryShiftRequest(
    string ShiftCode,
    string ShiftName,
    string StartTime,
    string EndTime,
    int MaxOrdersCapacity
);

public record LogDeliveryShiftDto(
    Guid Id,
    string ShiftCode,
    string ShiftName,
    string StartTime,
    string EndTime,
    int MaxOrdersCapacity,
    bool IsActive
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LOG_007: Gộp nhiều đơn thành chuyến
// ────────────────────────────────────────────────────────────────────────────

public record LogConsolidateTripRequest(
    Guid DriverVehicleId,
    string DriverName,
    string VehiclePlateNumber,
    IReadOnlyList<Guid> ConsolidatedOrderIds,
    decimal TotalWeightKg,
    DateTimeOffset ScheduledDepartureAt
);

public record LogDeliveryTripDto(
    Guid Id,
    string TripNumber,
    Guid DriverVehicleId,
    string DriverName,
    string VehiclePlateNumber,
    IReadOnlyList<Guid> ConsolidatedOrderIds,
    int TotalOrdersCount,
    decimal TotalWeightKg,
    string Status,
    DateTimeOffset ScheduledDepartureAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LOG_016: Chứng từ ký nhận (POD)
// ────────────────────────────────────────────────────────────────────────────

public record LogSubmitPodRequest(
    Guid DeliveryOrderId,
    string DeliveryOrderNumber,
    string RecipientName,
    string RecipientPhone,
    string SignatureImageUrl,
    string DeliveryPhotoUrl,
    string Notes
);

public record LogProofOfDeliveryDto(
    Guid Id,
    Guid DeliveryOrderId,
    string DeliveryOrderNumber,
    string RecipientName,
    string RecipientPhone,
    string SignatureImageUrl,
    string DeliveryPhotoUrl,
    string Notes,
    DateTimeOffset SignedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LOG_018: Hẹn giao lại
// ────────────────────────────────────────────────────────────────────────────

public record LogCreateRedeliveryRequest(
    Guid DeliveryOrderId,
    string OriginalOrderNumber,
    string FailedReason,
    DateTimeOffset RescheduledDeliveryDate,
    string PreferredShift
);

public record LogRedeliveryRequestDto(
    Guid Id,
    string RequestNumber,
    Guid DeliveryOrderId,
    string OriginalOrderNumber,
    string FailedReason,
    DateTimeOffset RescheduledDeliveryDate,
    string PreferredShift,
    string Status,
    DateTimeOffset RequestedAt
);
