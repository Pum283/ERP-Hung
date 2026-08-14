namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_INV_028: Xuất cho dự án
// ────────────────────────────────────────────────────────────────────────────

public record InvCreateProjectDispatchRequest(
    Guid ProjectId,
    string ProjectName,
    Guid WarehouseId,
    decimal TotalAllocatedValueVnd,
    string ProjectPhase
);

public record InvProjectDispatchDto(
    Guid Id,
    string DispatchNumber,
    Guid ProjectId,
    string ProjectName,
    Guid WarehouseId,
    decimal TotalAllocatedValueVnd,
    string ProjectPhase,
    DateTimeOffset DispatchedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_INV_032: Duyệt chuyển kho
// ────────────────────────────────────────────────────────────────────────────

public record InvCreateTransferApprovalRequest(
    string TransferRequestNumber,
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId
);

public record InvDecideTransferApprovalRequest(
    Guid ApprovalId,
    bool IsApproved,
    string ApproverName,
    string Comments
);

public record InvTransferApprovalDto(
    Guid Id,
    string TransferRequestNumber,
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    string ApprovalStatus,
    string ApproverName,
    string ApprovalComments,
    DateTimeOffset? DecisionAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_INV_034: Chuyển kho một bước
// ────────────────────────────────────────────────────────────────────────────

public record InvExecuteOneStepTransferRequest(
    Guid FromWarehouseId,
    Guid ToWarehouseId,
    Guid ProductId,
    decimal Quantity,
    string TransferReason
);

public record InvOneStepTransferDto(
    Guid Id,
    string TransferNumber,
    Guid FromWarehouseId,
    Guid ToWarehouseId,
    Guid ProductId,
    decimal Quantity,
    string TransferReason,
    DateTimeOffset ExecutedAt
);

// ────────────────────────────────────────────────────────────────────────────
// UC_INV_046: Theo dõi serial
// ────────────────────────────────────────────────────────────────────────────

public record InvRecordSerialEventRequest(
    Guid ProductId,
    string ProductCode,
    string SerialNumber,
    string EventType,
    string CurrentLocation,
    string DocumentReference
);

public record InvSerialTrackingHistoryDto(
    Guid Id,
    Guid ProductId,
    string ProductCode,
    string SerialNumber,
    string EventType,
    string CurrentLocation,
    string DocumentReference,
    DateTimeOffset Timestamp
);
