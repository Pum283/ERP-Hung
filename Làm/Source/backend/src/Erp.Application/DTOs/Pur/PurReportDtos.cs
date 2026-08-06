namespace Erp.Application.DTOs.Pur;

public sealed record PurPurchaseByVendorRowDto(
    Guid VendorId, string VendorCode, string VendorName,
    int GrnCount, decimal AcceptedQty, decimal Amount);

public sealed record PurPurchaseByProductRowDto(
    string ProductCode, string ProductName,
    decimal AcceptedQty, decimal Amount, int LineCount);

public sealed record PurOpenPrAgingRowDto(
    Guid Id, string Code, string Status, DateTimeOffset CreatedAt, int AgeDays,
    string? RequestedByName, int LineCount, decimal TotalQty);

public sealed record PurOpenPoAgingRowDto(
    Guid Id, string Code, Guid VendorId, string? VendorCode, string? VendorName,
    string Status, DateTimeOffset CreatedAt, int AgeDays,
    decimal OpenQty, decimal OpenAmount);
