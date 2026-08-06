namespace Erp.Application.DTOs.Log;

public sealed record LogReturnCreateRequest(Guid DeliveryOrderId, Guid WarehouseId, string? Reason, string? Note);
public sealed record LogReturnCountRequest(Guid LineId, decimal QtyCounted, decimal? QtyAccepted, string? Note);

public sealed record LogReturnNoteDto(
    Guid Id, string Code, Guid DeliveryOrderId, string? DeliveryCode,
    Guid WarehouseId, string? WarehouseName, string Status,
    string? Reason, string? Note,
    DateTimeOffset? CountedAt, DateTimeOffset? PostedAt,
    Guid? InvStockDocId, string? InvStockDocCode,
    int LineCount, decimal QtyExpectedTotal, decimal QtyAcceptedTotal,
    DateTimeOffset CreatedAt);

public sealed record LogReturnLineDto(
    Guid Id, Guid ReturnNoteId, Guid? DeliveryLineId,
    string ProductCode, string ProductName, string Unit,
    decimal QtyExpected, decimal QtyCounted, decimal QtyAccepted, string? Note);

public sealed record LogReturnDetailDto(
    LogReturnNoteDto Header, IReadOnlyList<LogReturnLineDto> Lines);

public sealed record LogOpsReportDto(
    int DeliveredCount, int FailedCount, int ReturnedCount, int InTransitCount, int OpenCount,
    decimal ReturnRatePct, decimal FailRatePct,
    int ReturnNotesDraft, int ReturnNotesCounted, int ReturnNotesPosted,
    int CodOverdueCount,
    int OnTimeDeliveredCount, int LateDeliveredCount, int PromisedDeliveredCount, decimal OnTimeRatePct);
