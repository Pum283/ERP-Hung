namespace Erp.Application.DTOs.Fin;

public sealed record FinBankAccountDto(
    Guid Id, string Code, string Name, string BankName, string AccountNumber, string? BranchName,
    Guid GlAccountId, string? GlAccountCode, string? GlAccountName,
    decimal OpeningBalance, string Status, string? Note,
    decimal PostedCreditTotal, decimal PostedDebitTotal, decimal BookBalance);

public sealed record FinBankAccountUpsertRequest(
    Guid? Id, string Code, string Name, string BankName, string AccountNumber, string? BranchName,
    Guid GlAccountId, decimal? OpeningBalance, string? Status, string? Note);

public sealed record FinBankVoucherDto(
    Guid Id, string Code, Guid BankAccountId, string? BankAccountCode, string? BankAccountName,
    string VoucherType, DateTimeOffset DocDate, decimal Amount, string Description,
    string? BankRef, string? PartnerCode, Guid? CounterAccountId, string? CounterAccountCode,
    Guid? PeriodId, string? PeriodCode, string Status,
    Guid? FinJournalId, string? FinJournalCode, DateTimeOffset? PostedAt,
    Guid? TransferRequestId, string? Note);

public sealed record FinBankVoucherUpsertRequest(
    Guid? Id, string? Code, Guid BankAccountId, string VoucherType, DateTimeOffset DocDate, decimal Amount,
    string Description, string? BankRef, string? PartnerCode, Guid? CounterAccountId, Guid? PeriodId, string? Note);

public sealed record FinBankTransferRequestDto(
    Guid Id, string Code, Guid FromBankAccountId, string? FromBankAccountCode,
    string BeneficiaryName, string BeneficiaryAccount, string BeneficiaryBank,
    decimal Amount, string Description, DateTimeOffset RequestDate,
    Guid? CounterAccountId, string? CounterAccountCode, Guid? PeriodId, string? PeriodCode,
    string Status, Guid? ExecutedVoucherId, string? ExecutedVoucherCode,
    DateTimeOffset? ApprovedAt, string? Note);

public sealed record FinBankTransferUpsertRequest(
    Guid? Id, string? Code, Guid FromBankAccountId, string BeneficiaryName, string BeneficiaryAccount,
    string BeneficiaryBank, decimal Amount, string Description, DateTimeOffset? RequestDate,
    Guid? CounterAccountId, Guid? PeriodId, string? Note);

public sealed record FinBankStatementLineDto(
    Guid Id, Guid BankAccountId, string? BankAccountCode, DateTimeOffset StmtDate, string Description,
    string? BankRef, string Direction, decimal Amount, string Status,
    Guid? MatchedVoucherId, string? MatchedVoucherCode, DateTimeOffset? MatchedAt, string? Note);

public sealed record FinBankStatementUpsertRequest(
    Guid? Id, Guid BankAccountId, DateTimeOffset StmtDate, string Description, string? BankRef,
    string Direction, decimal Amount, string? Note);

public sealed record FinBankMatchRequest(Guid VoucherId);

public sealed record FinBankBookRowDto(
    DateTimeOffset DocDate, string VoucherCode, string VoucherType, string Description,
    string? BankRef, decimal Credit, decimal Debit, decimal Balance);

public sealed record FinBankBookDto(
    Guid BankAccountId, string BankAccountCode, string BankAccountName, decimal OpeningBalance,
    decimal TotalCredit, decimal TotalDebit, decimal ClosingBalance,
    IReadOnlyList<FinBankBookRowDto> Rows);

public sealed record FinBankVoidRequest(string? Note);
