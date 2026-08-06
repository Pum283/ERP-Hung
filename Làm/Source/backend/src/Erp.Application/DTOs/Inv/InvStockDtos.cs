namespace Erp.Application.DTOs.Inv;

public sealed record InvBalanceDto(
    Guid Id, Guid WarehouseId, string? WarehouseName, Guid SkuId, string SkuCode, string SkuName,
    string? LotCode, DateOnly? ExpiryDate,
    decimal QtyOnHand, decimal QtyReserved, decimal QtyInTransit, decimal QtyAvailable);

public sealed record InvStockDocDto(
    Guid Id, string Code, string DocType, string SourceType, Guid WarehouseId, string? WarehouseName,
    string Status, string? RefModule, Guid? RefId, string? RefCode,
    DateTimeOffset? PostedAt, string? Note, int LineCount);
public sealed record InvStockDocLineDto(
    Guid Id, Guid DocId, Guid SkuId, string SkuCode, string SkuName,
    decimal Qty, string? LotCode, DateOnly? ExpiryDate, decimal UnitCost);
public sealed record InvStockDocDetailDto(InvStockDocDto Header, IReadOnlyList<InvStockDocLineDto> Lines);
public sealed record InvStockDocCreateRequest(
    string DocType, string SourceType, Guid WarehouseId, string? Note);
public sealed record InvStockDocLineRequest(
    Guid? Id, Guid SkuId, decimal Qty, string? LotCode, DateOnly? ExpiryDate, decimal? UnitCost);

public sealed record InvLotPickDto(
    Guid SkuId, string SkuCode, string? LotCode, DateOnly? ExpiryDate, decimal QtyAvailable, decimal QtyPick);
public sealed record InvSuggestLotsRequest(Guid WarehouseId, Guid SkuId, decimal Qty);

public sealed record InvReservationLineDto(
    Guid Id, Guid ReservationId, Guid SkuId, string SkuCode, string SkuName,
    decimal Qty, string? LotCode, DateOnly? ExpiryDate);
public sealed record InvReservationDto(
    Guid Id, string Code, Guid WarehouseId, string? WarehouseName, string Status,
    string? RefModule, Guid? RefId, string? RefCode, string? Note,
    DateTimeOffset? ActivatedAt, DateTimeOffset? ReleasedAt, int LineCount);
public sealed record InvReservationDetailDto(InvReservationDto Header, IReadOnlyList<InvReservationLineDto> Lines);
public sealed record InvReservationLineRequest(Guid SkuId, decimal Qty, string? LotCode, DateOnly? ExpiryDate);
public sealed record InvReservationCreateRequest(
    Guid WarehouseId, string? RefModule, Guid? RefId, string? RefCode, string? Note,
    bool Activate, IReadOnlyList<InvReservationLineRequest> Lines);

public sealed record InvAtpAlertRowDto(
    Guid WarehouseId, string? WarehouseName, Guid SkuId, string SkuCode, string SkuName,
    string? LotCode, DateOnly? ExpiryDate,
    decimal QtyOnHand, decimal QtyReserved, decimal QtyAvailable, string AlertType);

public sealed record InvTransferDto(
    Guid Id, string Code, Guid FromWarehouseId, string? FromWarehouseName,
    Guid ToWarehouseId, string? ToWarehouseName, string Status,
    DateTimeOffset? ShippedAt, DateTimeOffset? ReceivedAt, string? Note, int LineCount);
public sealed record InvTransferLineDto(
    Guid Id, Guid TransferId, Guid SkuId, string SkuCode, string SkuName,
    decimal Qty, string? LotCode, DateOnly? ExpiryDate);
public sealed record InvTransferDetailDto(InvTransferDto Header, IReadOnlyList<InvTransferLineDto> Lines);
public sealed record InvTransferCreateRequest(Guid FromWarehouseId, Guid ToWarehouseId, string? Note);
public sealed record InvTransferLineRequest(Guid? Id, Guid SkuId, decimal Qty, string? LotCode, DateOnly? ExpiryDate);

public sealed record InvStocktakeDto(
    Guid Id, string Code, Guid WarehouseId, string? WarehouseName, string Status,
    DateTimeOffset? CountedAt, DateTimeOffset? PostedAt, string? Note,
    int LineCount, int VarianceCount);
public sealed record InvStocktakeLineDto(
    Guid Id, Guid StocktakeId, Guid SkuId, string SkuCode, string SkuName,
    string? LotCode, decimal SystemQty, decimal? CountedQty, decimal VarianceQty);
public sealed record InvStocktakeDetailDto(InvStocktakeDto Header, IReadOnlyList<InvStocktakeLineDto> Lines);
public sealed record InvStocktakeCreateRequest(Guid WarehouseId, string? Note);
public sealed record InvStocktakeCountRequest(Guid LineId, decimal CountedQty);
