namespace Erp.Application.DTOs.Fin;

public sealed record FinCashFundDto(
    Guid Id, string Code, string Name, Guid CashAccountId, string? CashAccountCode, string? CashAccountName,
    Guid? CustodianUserId, string? CustodianName, decimal OpeningBalance, string Status, string? Note,
    decimal PostedReceiptTotal, decimal PostedPaymentTotal, decimal BookBalance);

public sealed record FinCashFundUpsertRequest(
    Guid? Id, string Code, string Name, Guid CashAccountId, Guid? CustodianUserId, string? CustodianName,
    decimal? OpeningBalance, string? Status, string? Note);

public sealed record FinCashVoucherDto(
    Guid Id, string Code, Guid FundId, string? FundCode, string? FundName,
    string VoucherType, DateTimeOffset DocDate, decimal Amount, string Description,
    string? PartnerCode, Guid? CounterAccountId, string? CounterAccountCode,
    Guid? PeriodId, string? PeriodCode, string Status,
    Guid? FinJournalId, string? FinJournalCode, DateTimeOffset? PostedAt, string? Note);

public sealed record FinCashVoucherUpsertRequest(
    Guid? Id, string? Code, Guid FundId, string VoucherType, DateTimeOffset DocDate, decimal Amount,
    string Description, string? PartnerCode, Guid? CounterAccountId, Guid? PeriodId, string? Note);

public sealed record FinCashBookRowDto(
    DateTimeOffset DocDate, string VoucherCode, string VoucherType, string Description,
    string? PartnerCode, decimal Receipt, decimal Payment, decimal Balance);

public sealed record FinCashBookDto(
    Guid FundId, string FundCode, string FundName, decimal OpeningBalance,
    decimal TotalReceipt, decimal TotalPayment, decimal ClosingBalance,
    IReadOnlyList<FinCashBookRowDto> Rows);

public sealed record FinCashVoidRequest(string? Note);
