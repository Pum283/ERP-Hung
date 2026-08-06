namespace Erp.Application.DTOs.Fin;

public sealed record FinRevenueDocumentDto(
    Guid Id, string Code, string Kind, string SourceModule, Guid? SourceId, string? SourceCode,
    DateTimeOffset DocDate, decimal RevenueAmount, decimal TaxAmount, decimal CogsAmount, decimal TotalAmount,
    Guid? PeriodId, string? PeriodCode,
    Guid? DebitAccountId, string? DebitAccountCode, Guid? CreditAccountId, string? CreditAccountCode,
    Guid? FinJournalId, string? FinJournalCode, string Status, DateTimeOffset? PostedAt, string? Note);

public sealed record FinRevenueRecognizeRequest(
    Guid? PeriodId, Guid? DebitAccountId, Guid? CreditAccountId, string? Note);

public sealed record FinRevenueNoteRequest(string? Note);

public sealed record FinRevenueSummaryDto(
    Guid? PeriodId, string? PeriodCode,
    decimal PosRevenue, int PosCount,
    decimal OrderRevenue, int OrderCount,
    decimal ArRevenue, int ArCount,
    decimal CogsAmount, int CogsCount,
    decimal GrossMargin);
