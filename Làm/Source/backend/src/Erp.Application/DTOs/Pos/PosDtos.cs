namespace Erp.Application.DTOs.Pos;

public sealed record PosStoreDto(
    Guid Id, string Code, string Name, string? Address, string Status,
    int TerminalCount, int PrinterCount, int CashierCount);

public sealed record PosStoreUpsertRequest(
    Guid? Id, string Code, string Name, string? Address, string? Status);

public sealed record PosTerminalDto(
    Guid Id, Guid StoreId, string Code, string Name, string Status);

public sealed record PosTerminalUpsertRequest(
    Guid? Id, string Code, string Name, string? Status);

public sealed record PosPrinterDto(
    Guid Id, Guid StoreId, string Code, string Name, string PrinterType,
    string? ConnectionInfo, string Status);

public sealed record PosPrinterUpsertRequest(
    Guid? Id, string Code, string Name, string PrinterType,
    string? ConnectionInfo, string? Status);

public sealed record PosCashierDto(
    Guid Id, Guid StoreId, Guid UserId, string? UserName, string Role, bool IsActive);

public sealed record PosCashierUpsertRequest(
    Guid? Id, Guid UserId, string Role, bool? IsActive);

public sealed record PosStoreDetailDto(
    PosStoreDto Store,
    IReadOnlyList<PosTerminalDto> Terminals,
    IReadOnlyList<PosPrinterDto> Printers,
    IReadOnlyList<PosCashierDto> Cashiers);

public sealed record PosCategoryDto(Guid Id, string Code, string Name, int SortOrder, bool IsActive, int ProductCount);

public sealed record PosCategoryUpsertRequest(Guid? Id, string Code, string Name, int? SortOrder, bool? IsActive);

public sealed record PosProductDto(
    Guid Id, Guid? CategoryId, string? CategoryName, string Code, string Name,
    string? Unit, string Status, int SortOrder, DateTimeOffset? SyncedAt, int BomLineCount);

public sealed record PosProductUpsertRequest(
    Guid? Id, Guid? CategoryId, string Code, string Name, string? Unit, string? Status, int? SortOrder);

public sealed record PosBomLineDto(
    Guid Id, Guid ProductId, string MaterialCode, string MaterialName, decimal Qty, string Unit);

public sealed record PosBomLineUpsertRequest(
    Guid? Id, string MaterialCode, string MaterialName, decimal Qty, string? Unit);

public sealed record PosTaxRateDto(
    Guid Id, string Code, string Name, decimal RatePct, bool IsDefault, bool IsActive);

public sealed record PosTaxRateUpsertRequest(
    Guid? Id, string Code, string Name, decimal RatePct, bool? IsDefault, bool? IsActive);

public sealed record PosPriceListDto(
    Guid Id, Guid StoreId, string? StoreName, string Code, string Name, string Status, int ItemCount);

public sealed record PosPriceListUpsertRequest(
    Guid? Id, Guid StoreId, string Code, string Name, string? Status);

public sealed record PosPriceItemDto(
    Guid Id, Guid PriceListId, Guid ProductId, string? ProductCode, string? ProductName,
    decimal Price, Guid? TaxRateId, string? TaxCode, decimal? TaxRatePct);

public sealed record PosPriceItemUpsertRequest(
    Guid? Id, Guid ProductId, decimal Price, Guid? TaxRateId);

public sealed record PosSyncResult(int ProductCount, DateTimeOffset SyncedAt);
