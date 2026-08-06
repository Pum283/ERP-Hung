namespace Erp.Application.DTOs.Fsm;

public sealed record FsmServiceTypeDto(Guid Id, string Code, string Name, string Status, string? Note);
public sealed record FsmServiceTypeUpsertRequest(Guid? Id, string Code, string Name, string? Status, string? Note);

public sealed record FsmFaultCodeDto(Guid Id, string Code, string Name, string Severity, string Status, string? Note);
public sealed record FsmFaultCodeUpsertRequest(Guid? Id, string Code, string Name, string? Severity, string? Status, string? Note);

public sealed record FsmPartDto(Guid Id, string Code, string Name, string Unit, string Status, string? Note);
public sealed record FsmPartUpsertRequest(Guid? Id, string Code, string Name, string? Unit, string? Status, string? Note);

public sealed record FsmSlaPolicyDto(
    Guid Id, string Code, string Name, string Priority, int ResponseHours, int ResolveHours, bool IsActive, string? Note);
public sealed record FsmSlaPolicyUpsertRequest(
    Guid? Id, string Code, string Name, string Priority, int ResponseHours, int ResolveHours, bool? IsActive, string? Note);

public sealed record FsmAssetDto(
    Guid Id, string Code, string CustomerName, string? CustomerPhone, string SerialNo, string? Model,
    DateTimeOffset? ActivatedAt, DateTimeOffset? WarrantyEndAt, string Status, string? Address, string? Note,
    bool WarrantyExpiringSoon);

public sealed record FsmAssetUpsertRequest(
    Guid? Id, string? Code, string CustomerName, string? CustomerPhone, string SerialNo, string? Model,
    DateTimeOffset? ActivatedAt, DateTimeOffset? WarrantyEndAt, string? Status, string? Address, string? Note);

public sealed record FsmAssetHistoryDto(
    Guid Id, Guid AssetId, string EventType, string Summary, Guid? TicketId,
    Guid ActorUserId, string? ActorName, DateTimeOffset OccurredAt);

public sealed record FsmAssetHistoryCreateRequest(string EventType, string Summary);

public sealed record FsmAssetDetailDto(FsmAssetDto Asset, IReadOnlyList<FsmAssetHistoryDto> History);

public sealed record FsmTicketDto(
    Guid Id, string Code, string Channel, string Subject, string? Description,
    string CustomerName, string? CustomerPhone,
    Guid? ServiceTypeId, string? ServiceTypeName,
    Guid? FaultCodeId, string? FaultCodeName,
    Guid? AssetId, string? AssetCode, string? SerialNo,
    Guid? SlaPolicyId, string? SlaPolicyName,
    string Priority, string Status,
    Guid? AssignedTechUserId, string? AssignedTechName,
    DateTimeOffset? DueResponseAt, DateTimeOffset? DueResolveAt,
    string? EscalateReason, DateTimeOffset CreatedAt,
    DateTimeOffset? AppointmentAt, string? AppointmentNote,
    string? RootCause, string? ResolutionNote,
    DateTimeOffset? CheckedOutAt,
    DateTimeOffset? AcceptanceSignedAt, string? AcceptanceSignerName, string? AcceptanceNote,
    DateTimeOffset? ResolvedAt, DateTimeOffset? ClosedAt,
    bool? SlaResponseMet, bool? SlaResolveMet);

public sealed record FsmTicketUpsertRequest(
    Guid? Id, string? Code, string Channel, string Subject, string? Description,
    string CustomerName, string? CustomerPhone,
    Guid? ServiceTypeId, Guid? FaultCodeId, Guid? AssetId, string? Priority);

public sealed record FsmAssignRequest(Guid TechUserId, string? TechName);
public sealed record FsmEscalateRequest(Guid NewTechUserId, string? NewTechName, string Reason);
public sealed record FsmTicketStatusRequest(string Status, string? Note);

public sealed record FsmAppointmentRequest(DateTimeOffset AppointmentAt, string? Note);
public sealed record FsmWorkLogRequest(string RootCause, string ResolutionNote, Guid? FaultCodeId);
public sealed record FsmCheckoutRequest(string? Note);
public sealed record FsmAcceptRequest(string SignerName, string? Note);
public sealed record FsmCloseRequest(string? Note);

public sealed record FsmTicketPartLineDto(
    Guid Id, Guid TicketId, Guid PartId, string PartCode, string PartName,
    decimal Qty, decimal UnitCost, decimal Amount, string Source,
    Guid? TechUserId, string? TechName, DateTimeOffset IssuedAt, string? Note);

public sealed record FsmConsumePartRequest(
    Guid PartId, decimal Qty, decimal? UnitCost, string? Source, Guid? TechUserId, string? Note);

public sealed record FsmTicketDetailDto(FsmTicketDto Ticket, IReadOnlyList<FsmTicketPartLineDto> PartLines);

public sealed record FsmPartStockDto(
    Guid Id, Guid PartId, string PartCode, string PartName, string Unit,
    string LocationType, Guid? TechUserId, string? TechName,
    decimal QtyOnHand, decimal UnitCost, decimal Amount);

public sealed record FsmPartReceiptRequest(Guid PartId, decimal Qty, decimal? UnitCost, string? Note);

public sealed record FsmPartIssueLineDto(Guid Id, Guid PartId, string PartCode, string PartName, decimal Qty, decimal UnitCost);
public sealed record FsmPartIssueDocDto(
    Guid Id, string Code, Guid TechUserId, string TechName, string Status,
    string? Note, DateTimeOffset? PostedAt, DateTimeOffset CreatedAt,
    IReadOnlyList<FsmPartIssueLineDto> Lines);
public sealed record FsmPartIssueLineRequest(Guid PartId, decimal Qty, decimal? UnitCost);
public sealed record FsmPartIssueCreateRequest(Guid TechUserId, string? TechName, string? Note, IReadOnlyList<FsmPartIssueLineRequest> Lines);

public sealed record FsmPartReconcileLineDto(
    Guid Id, Guid PartId, string PartCode, string PartName,
    decimal SystemQty, decimal CountedQty, decimal DiffQty, decimal UnitCost);
public sealed record FsmPartReconcileDocDto(
    Guid Id, string Code, string Scope, Guid? TechUserId, string? TechName, string Status,
    string? Note, DateTimeOffset? PostedAt, DateTimeOffset CreatedAt,
    IReadOnlyList<FsmPartReconcileLineDto> Lines);
public sealed record FsmPartReconcileLineRequest(Guid PartId, decimal CountedQty);
public sealed record FsmPartReconcileCreateRequest(
    string Scope, Guid? TechUserId, string? TechName, string? Note,
    IReadOnlyList<FsmPartReconcileLineRequest> Lines);
