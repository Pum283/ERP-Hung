namespace Erp.Application.DTOs.Inv;

public sealed record InvItemGroupDto(Guid Id, string Code, string Name, int SortOrder, bool IsActive, int SkuCount);
public sealed record InvItemGroupUpsertRequest(Guid? Id, string Code, string Name, int? SortOrder, bool? IsActive);

public sealed record InvUomDto(Guid Id, string Code, string Name, bool IsActive);
public sealed record InvUomUpsertRequest(Guid? Id, string Code, string Name, bool? IsActive);

public sealed record InvUnitConversionDto(
    Guid Id, Guid FromUnitId, string? FromUnitCode, Guid ToUnitId, string? ToUnitCode, decimal Factor);
public sealed record InvUnitConversionUpsertRequest(Guid? Id, Guid FromUnitId, Guid ToUnitId, decimal Factor);

public sealed record InvSkuDto(
    Guid Id, string Code, string Name, Guid? GroupId, string? GroupName,
    Guid BaseUnitId, string? BaseUnitCode,
    bool TrackLot, bool TrackSerial, bool TrackExpiry,
    string CostingMethod, decimal StandardCost, string Status,
    decimal? MinQty, decimal? MaxQty, decimal? ReorderQty, string? Note);

public sealed record InvSkuUpsertRequest(
    Guid? Id, string Code, string Name, Guid? GroupId, Guid BaseUnitId,
    bool? TrackLot, bool? TrackSerial, bool? TrackExpiry,
    string? CostingMethod, decimal StandardCost, string? Status,
    decimal? MinQty, decimal? MaxQty, decimal? ReorderQty, string? Note);

public sealed record InvSkuStatusRequest(string Status);
public sealed record InvImportRequest(string CsvText);
public sealed record InvImportResult(int Total, int Success, int Failed, IReadOnlyList<string> Messages);

public sealed record InvWarehouseTypeDto(Guid Id, string Code, string Name, bool IsActive);
public sealed record InvWarehouseTypeUpsertRequest(Guid? Id, string Code, string Name, bool? IsActive);

public sealed record InvWarehouseDto(
    Guid Id, string Code, string Name, Guid? WarehouseTypeId, string? WarehouseTypeName,
    string? Address, string Status, string PickPolicy, bool AllowNegativeStock, int KeeperCount);

public sealed record InvWarehouseUpsertRequest(
    Guid? Id, string Code, string Name, Guid? WarehouseTypeId, string? Address, string? Status,
    string? PickPolicy, bool? AllowNegativeStock);

public sealed record InvWarehouseKeeperDto(
    Guid Id, Guid WarehouseId, Guid UserId, string? UserName, string Role, bool IsActive);

public sealed record InvWarehouseKeeperUpsertRequest(
    Guid? Id, Guid UserId, string Role, bool? IsActive);

public sealed record InvWarehouseDetailDto(
    InvWarehouseDto Warehouse, IReadOnlyList<InvWarehouseKeeperDto> Keepers);
