namespace Erp.Application.DTOs.Fin;

public sealed record FinApInvoiceDto(
    Guid Id, string Code, Guid VendorId, string? VendorCode, string? VendorName,
    string? VendorInvoiceNo, Guid? PurVendorInvoiceId,
    DateTimeOffset InvoiceDate, DateTimeOffset DueDate,
    decimal SubTotal, decimal TaxAmount, decimal TotalAmount, decimal PaidAmount, decimal OpenAmount,
    string Status, Guid? PeriodId, string? PeriodCode,
    Guid? FinJournalId, string? FinJournalCode, DateTimeOffset? PostedAt, string? Note);

public sealed record FinApInvoiceUpsertRequest(
    Guid? Id, string? Code, Guid VendorId, string? VendorInvoiceNo, Guid? PurVendorInvoiceId,
    DateTimeOffset InvoiceDate, DateTimeOffset DueDate,
    decimal SubTotal, decimal TaxAmount, Guid? PeriodId,
    Guid? ApAccountId, Guid? ExpenseAccountId, string? Note);

public sealed record FinApVendorBalanceDto(
    Guid VendorId, string VendorCode, string VendorName,
    int OpenInvoiceCount, decimal TotalOpen, decimal OverdueAmount, decimal NotDueAmount);

public sealed record FinApPaymentRequestLineDto(Guid ApInvoiceId, string? InvoiceCode, decimal Amount, decimal InvoiceOpen);

public sealed record FinApPaymentRequestDto(
    Guid Id, string Code, Guid VendorId, string? VendorCode, string? VendorName,
    DateTimeOffset RequestDate, decimal RequestAmount, string PayMethod,
    Guid? CashFundId, Guid? BankAccountId, string Status,
    Guid? PaymentId, string? PaymentCode, DateTimeOffset? ApprovedAt, string? Note,
    IReadOnlyList<FinApPaymentRequestLineDto> Lines);

public sealed record FinApPaymentRequestLineInput(Guid ApInvoiceId, decimal Amount);

public sealed record FinApPaymentRequestUpsertRequest(
    Guid? Id, string? Code, Guid VendorId, DateTimeOffset? RequestDate, string PayMethod,
    Guid? CashFundId, Guid? BankAccountId, string? Note,
    IReadOnlyList<FinApPaymentRequestLineInput> Lines);

public sealed record FinApPaymentAllocationDto(Guid ApInvoiceId, string? InvoiceCode, decimal Amount);

public sealed record FinApPaymentDto(
    Guid Id, string Code, Guid VendorId, string? VendorCode, string? VendorName,
    DateTimeOffset PayDate, decimal Amount, string PayMethod,
    Guid? CashFundId, Guid? BankAccountId, Guid? PaymentRequestId,
    Guid? CashVoucherId, Guid? BankVoucherId, string Status,
    Guid? FinJournalId, string? FinJournalCode, DateTimeOffset? PostedAt, string? Note,
    IReadOnlyList<FinApPaymentAllocationDto> Allocations);

public sealed record FinApPaymentAllocationInput(Guid ApInvoiceId, decimal Amount);

public sealed record FinApPaymentUpsertRequest(
    Guid? Id, string? Code, Guid VendorId, DateTimeOffset PayDate, string PayMethod,
    Guid? CashFundId, Guid? BankAccountId, Guid? PaymentRequestId, Guid? PeriodId, string? Note,
    IReadOnlyList<FinApPaymentAllocationInput> Allocations);

public sealed record FinApAgingBucketDto(string Bucket, decimal Amount, int InvoiceCount);

public sealed record FinApAgingRowDto(
    Guid VendorId, string VendorCode, string VendorName,
    decimal Current, decimal D1To30, decimal D31To60, decimal D61To90, decimal Over90, decimal Total);

public sealed record FinApAgingDto(
    DateTimeOffset AsOf, IReadOnlyList<FinApAgingBucketDto> Buckets, IReadOnlyList<FinApAgingRowDto> Rows);

public sealed record FinApNoteRequest(string? Note);
