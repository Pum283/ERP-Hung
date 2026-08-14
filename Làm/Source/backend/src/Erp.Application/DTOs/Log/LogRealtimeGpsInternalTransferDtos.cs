namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_LOG_019: Theo dõi realtime trên bản đồ
// ────────────────────────────────────────────────────────────────────────────

public record LogPingGpsLocationRequest(
    Guid DriverVehicleId,
    string VehiclePlateNumber,
    double Latitude,
    double Longitude,
    double CurrentSpeedKmh,
    string CurrentAddress
);

public record LogRealtimeGpsPingDto(
    Guid Id,
    Guid DriverVehicleId,
    string VehiclePlateNumber,
    double Latitude,
    double Longitude,
    double CurrentSpeedKmh,
    string CurrentAddress,
    DateTimeOffset PingedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LOG_031 & UC_LOG_032: Lệnh giao nội bộ & Xác nhận nhận hàng
// ────────────────────────────────────────────────────────────────────────────

public record LogCreateInternalTransferDeliveryRequest(
    Guid FromWarehouseId,
    string FromWarehouseName,
    Guid ToWarehouseId,
    string ToWarehouseName,
    string DriverName,
    string VehiclePlateNumber,
    decimal DispatchedQuantity
);

public record LogConfirmInternalReceiptRequest(
    Guid InternalDeliveryId,
    decimal ReceivedQuantity,
    string ReceiverStaffName
);

public record LogInternalTransferDeliveryDto(
    Guid Id,
    string InternalDeliveryNumber,
    Guid FromWarehouseId,
    string FromWarehouseName,
    Guid ToWarehouseId,
    string ToWarehouseName,
    string DriverName,
    string VehiclePlateNumber,
    decimal DispatchedQuantity,
    decimal ReceivedQuantity,
    string Status,
    string ReceiverStaffName,
    DateTimeOffset DispatchedAt,
    DateTimeOffset? ReceivedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LOG_033: Đối soát giao nội bộ
// ────────────────────────────────────────────────────────────────────────────

public record LogCreateInternalReconciliationRequest(
    Guid InternalTransferDeliveryId,
    decimal DiscrepancyCostVnd,
    string RootCause
);

public record LogInternalDeliveryReconciliationDto(
    Guid Id,
    string ReconciliationNumber,
    Guid InternalTransferDeliveryId,
    string InternalDeliveryNumber,
    decimal DispatchedTotalQty,
    decimal ReceivedTotalQty,
    decimal DiscrepancyQty,
    decimal DiscrepancyCostVnd,
    string RootCause,
    string ResolutionStatus,
    DateTimeOffset ReconciledAt
);
