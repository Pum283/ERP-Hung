namespace Erp.Application.DTOs.Pur;

public sealed record PurVendorDto(
    Guid Id, string Code, string Name, string? TaxCode, string? Phone, string? Email,
    string? Address, string? PaymentTerms, string Status, int ContactCount, int ProductCount);

public sealed record PurVendorUpsertRequest(
    Guid? Id, string Code, string Name, string? TaxCode, string? Phone, string? Email,
    string? Address, string? PaymentTerms, string? Status);

public sealed record PurVendorContactDto(
    Guid Id, Guid VendorId, string FullName, string? Title, string? Phone, string? Email, bool IsPrimary);

public sealed record PurVendorContactUpsertRequest(
    Guid? Id, string FullName, string? Title, string? Phone, string? Email, bool? IsPrimary);

public sealed record PurVendorProductDto(
    Guid Id, Guid VendorId, string ProductCode, string ProductName, bool IsPreferred);

public sealed record PurVendorProductUpsertRequest(
    Guid? Id, string ProductCode, string ProductName, bool? IsPreferred);

public sealed record PurVendorPriceDto(
    Guid Id, Guid VendorId, string ProductCode, string ProductName,
    decimal UnitPrice, string Currency, DateOnly EffectiveFrom, DateOnly? EffectiveTo);

public sealed record PurVendorPriceUpsertRequest(
    Guid? Id, string ProductCode, string ProductName, decimal UnitPrice,
    string? Currency, DateOnly EffectiveFrom, DateOnly? EffectiveTo);

public sealed record PurVendorDetailDto(
    PurVendorDto Vendor,
    IReadOnlyList<PurVendorContactDto> Contacts,
    IReadOnlyList<PurVendorProductDto> Products,
    IReadOnlyList<PurVendorPriceDto> Prices);

public sealed record PurPrLineDto(
    Guid Id, Guid PrId, string ProductCode, string ProductName, decimal Qty, string Unit, string? Note);

public sealed record PurPrLineUpsertRequest(
    Guid? Id, string ProductCode, string ProductName, decimal Qty, string? Unit, string? Note);

public sealed record PurPurchaseRequestDto(
    Guid Id, string Code, string? RequestingUnit, string? Note, string Status,
    string? DecisionNote, Guid RequestedBy, string? RequestedByName,
    Guid? DecidedBy, string? DecidedByName, DateTimeOffset? DecidedAt, int LineCount);

public sealed record PurPurchaseRequestUpsertRequest(
    Guid? Id, string Code, string? RequestingUnit, string? Note);

public sealed record PurPrDetailDto(PurPurchaseRequestDto Header, IReadOnlyList<PurPrLineDto> Lines);

public sealed record PurPrDecisionRequest(string? Note);

public sealed record PurPoLineDto(
    Guid Id, Guid PoId, string ProductCode, string ProductName,
    decimal Qty, decimal ReceivedQty, decimal InvoicedQty, decimal UnitPrice, string Unit);

public sealed record PurPoLineUpsertRequest(
    Guid? Id, string ProductCode, string ProductName, decimal Qty, decimal UnitPrice, string? Unit);

public sealed record PurPurchaseOrderDto(
    Guid Id, string Code, Guid VendorId, string? VendorName, Guid? SourcePrId, string? SourcePrCode,
    string Status, int Version, decimal TotalAmount, string Currency, string? Note,
    Guid CreatedByUserId, string? CreatedByName,
    Guid? ApprovedBy, string? ApprovedByName, DateTimeOffset? ApprovedAt, DateTimeOffset? SentAt,
    DateTimeOffset? PrintedAt, DateTimeOffset? ClosedAt, string? CancelReason,
    int LineCount, decimal ReceivedPct);

public sealed record PurPurchaseOrderCreateRequest(
    Guid? Id, string Code, Guid VendorId, Guid? SourcePrId, string? Note);

public sealed record PurPoDetailDto(PurPurchaseOrderDto Header, IReadOnlyList<PurPoLineDto> Lines);

public sealed record PurCreatePoFromPrRequest(string Code, Guid VendorId, string? Note);
public sealed record PurPoCancelRequest(string Reason);
