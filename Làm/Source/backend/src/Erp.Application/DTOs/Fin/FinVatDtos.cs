namespace Erp.Application.DTOs.Fin;

public sealed record FinVatCalcRequest(decimal TaxableAmount, Guid? TaxId, decimal? RatePercent);

public sealed record FinVatCalcResult(
    decimal TaxableAmount, decimal RatePercent, decimal TaxAmount, decimal TotalAmount,
    Guid? TaxId, string? TaxCode);

public sealed record FinVatDocumentDto(
    Guid Id, string Code, string Direction, Guid? TaxId, string? TaxCode,
    decimal RatePercent, string InvoiceNo, string? InvoiceSeries, DateTimeOffset InvoiceDate,
    string? PartnerCode, string? PartnerName, string? PartnerTaxCode,
    decimal TaxableAmount, decimal TaxAmount, decimal TotalAmount,
    Guid? PeriodId, string? PeriodCode, Guid? ArInvoiceId, Guid? ApInvoiceId,
    string Status, DateTimeOffset? PostedAt, string? Note);

public sealed record FinVatDocumentUpsertRequest(
    Guid? Id, string? Code, string Direction, Guid? TaxId, decimal? RatePercent,
    string InvoiceNo, string? InvoiceSeries, DateTimeOffset InvoiceDate,
    string? PartnerCode, string? PartnerName, string? PartnerTaxCode,
    decimal TaxableAmount, Guid? PeriodId, Guid? ArInvoiceId, Guid? ApInvoiceId, string? Note);

public sealed record FinVatSummaryDto(
    DateTimeOffset? From, DateTimeOffset? To, Guid? PeriodId, string? PeriodCode,
    decimal OutputTaxable, decimal OutputTax, int OutputCount,
    decimal InputTaxable, decimal InputTax, int InputCount,
    decimal NetVatPayable);

public sealed record FinVatNoteRequest(string? Note);
