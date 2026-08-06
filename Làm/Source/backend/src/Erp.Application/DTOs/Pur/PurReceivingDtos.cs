namespace Erp.Application.DTOs.Pur;

public sealed record PurGrnDto(
    Guid Id, string Code, Guid PoId, string? PoCode, Guid VendorId, string? VendorName,
    string Status, DateTimeOffset ReceivedAt, string? QualityNote,
    string InventoryPushStatus, string? Note, int LineCount,
    decimal TotalReceivedQty, decimal TotalAcceptedQty, decimal TotalRejectedQty);
public sealed record PurGrnLineDto(
    Guid Id, Guid GrnId, Guid? PoLineId, string ProductCode, string ProductName,
    decimal OrderedQty, decimal ReceivedQty, decimal AcceptedQty, decimal RejectedQty,
    string Unit, decimal UnitPrice);
public sealed record PurGrnDetailDto(PurGrnDto Header, IReadOnlyList<PurGrnLineDto> Lines);
public sealed record PurGrnCreateRequest(Guid PoId, string? Note, string? QualityNote);
public sealed record PurGrnLineUpdateRequest(
    Guid LineId, decimal ReceivedQty, decimal AcceptedQty, decimal RejectedQty);

public sealed record PurInvoiceDto(
    Guid Id, string Code, Guid VendorId, string? VendorName, Guid? PoId, string? PoCode,
    string InvoiceNumber, DateTimeOffset InvoiceDate, string Status,
    decimal SubTotal, decimal TaxAmount, decimal TotalAmount,
    string MatchStatus, string? MatchNote, string ApPushStatus, string? Note, int LineCount);
public sealed record PurInvoiceLineDto(
    Guid Id, Guid InvoiceId, Guid? PoLineId, Guid? GrnLineId,
    string ProductCode, string ProductName, decimal Qty, decimal UnitPrice, decimal LineAmount);
public sealed record PurInvoiceDetailDto(PurInvoiceDto Header, IReadOnlyList<PurInvoiceLineDto> Lines);
public sealed record PurInvoiceCreateRequest(
    Guid VendorId, Guid? PoId, string InvoiceNumber, DateTimeOffset? InvoiceDate,
    decimal? TaxAmount, string? Note);
public sealed record PurInvoiceLineUpsertRequest(
    Guid? Id, Guid? PoLineId, Guid? GrnLineId, string ProductCode, string ProductName,
    decimal Qty, decimal UnitPrice);
