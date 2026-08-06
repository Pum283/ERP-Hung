namespace Erp.Application.DTOs.Inv;

public sealed record InvStockValueRowDto(
    Guid SkuId, string SkuCode, string SkuName, Guid WarehouseId, string? WarehouseName,
    decimal QtyOnHand, decimal StandardCost, decimal StockValue);

public sealed record InvMovementPeriodRowDto(
    Guid SkuId, string SkuCode, string SkuName,
    decimal QtyIn, decimal QtyOut, decimal QtyNet, decimal ValueIn, decimal ValueOut);

public sealed record InvSkuCardLineDto(
    DateTimeOffset At, string DocCode, string DocType, string SourceType,
    string WarehouseName, decimal QtySigned, decimal UnitCost, decimal Amount, string? RefCode);

public sealed record InvMinMaxAlertRowDto(
    Guid SkuId, string SkuCode, string SkuName, Guid WarehouseId, string? WarehouseName,
    decimal QtyOnHand, decimal? MinQty, decimal? MaxQty, string AlertType);

public sealed record InvStocktakeReportRowDto(
    Guid StocktakeId, string StocktakeCode, string? WarehouseName, string Status,
    string SkuCode, string SkuName, decimal SystemQty, decimal? CountedQty, decimal VarianceQty);

public sealed record InvDashboardDto(
    int SkuCount, int WarehouseCount, decimal TotalQtyOnHand, decimal TotalStockValue,
    int BelowMinCount, int AboveMaxCount, int OpenStocktakeCount,
    IReadOnlyList<InvMinMaxAlertRowDto> TopAlerts,
    int NearExpiryCount, int ExpiredCount, int InsufficientAtpCount);

public sealed record InvNearExpiryRowDto(
    Guid WarehouseId, string? WarehouseName, Guid SkuId, string SkuCode, string SkuName,
    string? LotCode, DateOnly? ExpiryDate, decimal QtyOnHand, decimal QtyReserved, decimal QtyAvailable,
    int DaysToExpiry, string AlertType);
