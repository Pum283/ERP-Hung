namespace Erp.Application.DTOs.Log;

public sealed record LogCarrierDto(
    Guid Id, string Code, string Name, string? Phone, string? ContactName, string? Note, string Status);

public sealed record LogCarrierUpsertRequest(
    Guid? Id, string Code, string Name, string? Phone, string? ContactName, string? Note, string? Status);

public sealed record LogDeliveryOrderDto(
    Guid Id, string Code, string SourceOrderCode, string CustomerName, string? ShipAddress, string? Phone,
    string Status, Guid? CarrierId, string? CarrierName, Guid? DriverUserId, string? DriverName,
    Guid? ParentOrderId, int BatchNo, string? Note, string? FailureReason,
    string? WaybillNo, DateTimeOffset? WaybillPrintedAt,
    DateTimeOffset? PickedAt, DateTimeOffset? DispatchedAt, DateTimeOffset? DeliveredAt,
    DateTimeOffset? PromisedAt, bool? OnTime,
    bool IsCod, decimal CodAmount, string CodStatus,
    DateTimeOffset? CodDueAt, DateTimeOffset? CodCollectedAt, Guid? CodHandoverId, bool CodOverdue,
    int LineCount);

public sealed record LogDeliveryLineDto(
    Guid Id, Guid DeliveryOrderId, string ProductCode, string ProductName,
    decimal Qty, decimal QtyPicked, string Unit, string? Note);

public sealed record LogShipmentEventDto(
    Guid Id, Guid DeliveryOrderId, string Status, string? Note, Guid ActorUserId, string? ActorName, DateTimeOffset OccurredAt);

public sealed record LogDeliveryDetailDto(
    LogDeliveryOrderDto Order,
    IReadOnlyList<LogDeliveryLineDto> Lines,
    IReadOnlyList<LogShipmentEventDto> Events,
    IReadOnlyList<LogDeliveryOrderDto> ChildBatches);

public sealed record LogDeliveryUpsertRequest(
    Guid? Id, string? Code, string SourceOrderCode, string CustomerName,
    string? ShipAddress, string? Phone, string? Note, DateTimeOffset? PromisedAt);

public sealed record LogDeliveryLineUpsertRequest(
    Guid? Id, string ProductCode, string ProductName, decimal Qty, string? Unit, string? Note);

public sealed record LogSplitBatchRequest(
    IReadOnlyList<LogSplitLineRequest> Lines, string? Note);

public sealed record LogSplitLineRequest(Guid LineId, decimal Qty);

public sealed record LogPickLineRequest(Guid LineId, decimal QtyPicked);

public sealed record LogPickRequest(IReadOnlyList<LogPickLineRequest> Lines);

public sealed record LogAssignRequest(Guid? CarrierId, Guid? DriverUserId, string? DriverName);

public sealed record LogStatusRequest(string Status, string? Note);

public sealed record LogFailRequest(string Reason);
