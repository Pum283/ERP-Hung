namespace Erp.Application.DTOs.Fin;

public sealed record FinArInvoiceDto(
    Guid Id, string Code, Guid CustomerId, string? CustomerCode, string? CustomerName,
    string? CustomerInvoiceNo, Guid? CrmOrderId,
    DateTimeOffset InvoiceDate, DateTimeOffset DueDate,
    decimal SubTotal, decimal TaxAmount, decimal TotalAmount, decimal ReceivedAmount, decimal OpenAmount,
    string Status, bool CreditLimitWarned, Guid? PeriodId, string? PeriodCode,
    Guid? FinJournalId, string? FinJournalCode, DateTimeOffset? PostedAt, string? Note);

public sealed record FinArInvoiceUpsertRequest(
    Guid? Id, string? Code, Guid CustomerId, string? CustomerInvoiceNo, Guid? CrmOrderId,
    DateTimeOffset InvoiceDate, DateTimeOffset DueDate,
    decimal SubTotal, decimal TaxAmount, Guid? PeriodId,
    Guid? ArAccountId, Guid? RevenueAccountId, string? Note);

public sealed record FinArCustomerBalanceDto(
    Guid CustomerId, string CustomerCode, string CustomerName,
    int OpenInvoiceCount, decimal TotalOpen, decimal OverdueAmount, decimal NotDueAmount,
    decimal? CreditLimit, decimal? CreditUsedPct, string CreditStatus);

public sealed record FinArCreditLimitDto(
    Guid Id, Guid CustomerId, string? CustomerCode, string? CustomerName,
    decimal CreditLimit, decimal WarningPercent, bool IsActive, string? Note,
    decimal OpenBalance, string CreditStatus);

public sealed record FinArCreditLimitUpsertRequest(
    Guid? Id, Guid CustomerId, decimal CreditLimit, decimal? WarningPercent, bool? IsActive, string? Note);

public sealed record FinArReceiptAllocationDto(Guid ArInvoiceId, string? InvoiceCode, decimal Amount);

public sealed record FinArReceiptDto(
    Guid Id, string Code, Guid CustomerId, string? CustomerCode, string? CustomerName,
    DateTimeOffset ReceiptDate, decimal Amount, string PayMethod,
    Guid? CashFundId, Guid? BankAccountId, Guid? CashVoucherId, Guid? BankVoucherId,
    string Status, Guid? FinJournalId, string? FinJournalCode, DateTimeOffset? PostedAt, string? Note,
    IReadOnlyList<FinArReceiptAllocationDto> Allocations);

public sealed record FinArReceiptAllocationInput(Guid ArInvoiceId, decimal Amount);

public sealed record FinArReceiptUpsertRequest(
    Guid? Id, string? Code, Guid CustomerId, DateTimeOffset ReceiptDate, string PayMethod,
    Guid? CashFundId, Guid? BankAccountId, Guid? PeriodId, string? Note,
    IReadOnlyList<FinArReceiptAllocationInput> Allocations);

public sealed record FinArAgingBucketDto(string Bucket, decimal Amount, int InvoiceCount);

public sealed record FinArAgingRowDto(
    Guid CustomerId, string CustomerCode, string CustomerName,
    decimal Current, decimal D1To30, decimal D31To60, decimal D61To90, decimal Over90, decimal Total);

public sealed record FinArAgingDto(
    DateTimeOffset AsOf, IReadOnlyList<FinArAgingBucketDto> Buckets, IReadOnlyList<FinArAgingRowDto> Rows);

public sealed record FinArNoteRequest(string? Note);
