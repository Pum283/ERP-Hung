namespace Erp.Application.DTOs.Fin;

public sealed record FinAccountGroupDto(Guid Id, string Code, string Name, int SortOrder, bool IsActive, int AccountCount);
public sealed record FinAccountGroupUpsertRequest(Guid? Id, string Code, string Name, int? SortOrder, bool? IsActive);

public sealed record FinAccountDto(
    Guid Id, string Code, string Name, Guid? GroupId, string? GroupName,
    string AccountType, bool IsPostable, string Status, string? Note);
public sealed record FinAccountUpsertRequest(
    Guid? Id, string Code, string Name, Guid? GroupId, string AccountType, bool? IsPostable, string? Status, string? Note);

public sealed record FinFiscalYearDto(
    Guid Id, string Code, string Name, int Year, DateTimeOffset StartDate, DateTimeOffset EndDate, bool IsActive, int PeriodCount);
public sealed record FinFiscalYearUpsertRequest(
    Guid? Id, string Code, string Name, int Year, DateTimeOffset StartDate, DateTimeOffset EndDate, bool? IsActive, bool? GenerateMonths);

public sealed record FinPeriodDto(
    Guid Id, Guid FiscalYearId, string Code, string Name,
    DateTimeOffset StartDate, DateTimeOffset EndDate, string Status, DateTimeOffset? LockedAt);
public sealed record FinPeriodLockRequest(bool Lock);

public sealed record FinCostCenterDto(Guid Id, string Code, string Name, string Status, string? Note);
public sealed record FinCostCenterUpsertRequest(Guid? Id, string Code, string Name, string? Status, string? Note);

public sealed record FinPaymentMethodDto(Guid Id, string Code, string Name, string Status);
public sealed record FinPaymentMethodUpsertRequest(Guid? Id, string Code, string Name, string? Status);

public sealed record FinTaxDto(
    Guid Id, string Code, string Name, decimal RatePercent, string TaxType, bool IsDefault,
    DateOnly? EffectiveFrom, DateOnly? EffectiveTo, string Status, string? Note);
public sealed record FinTaxUpsertRequest(
    Guid? Id, string Code, string Name, decimal RatePercent, string? TaxType, bool? IsDefault,
    DateOnly? EffectiveFrom, DateOnly? EffectiveTo, string? Status, string? Note);

public sealed record FinJournalLineDto(
    Guid Id, Guid JournalId, Guid AccountId, string? AccountCode, string? AccountName,
    decimal Debit, decimal Credit, string? PartnerCode, Guid? CostCenterId, string? CostCenterName,
    string? Note, int LineNo);

public sealed record FinJournalLineUpsertRequest(
    Guid? Id, Guid AccountId, decimal Debit, decimal Credit,
    string? PartnerCode, Guid? CostCenterId, string? Note);

public sealed record FinJournalDto(
    Guid Id, string Code, Guid PeriodId, string? PeriodCode, DateTimeOffset EntryDate,
    string Description, string Status, string Source, Guid? ReversedFromId, Guid? ReversalId,
    string? PartnerCode, Guid? CostCenterId, string? CostCenterName,
    decimal TotalDebit, decimal TotalCredit, int LineCount, DateTimeOffset? PostedAt);

public sealed record FinJournalUpsertRequest(
    Guid? Id, string? Code, Guid PeriodId, DateTimeOffset EntryDate, string Description,
    string? PartnerCode, Guid? CostCenterId, string? Source,
    IReadOnlyList<FinJournalLineUpsertRequest>? Lines);

public sealed record FinJournalDetailDto(FinJournalDto Journal, IReadOnlyList<FinJournalLineDto> Lines);

public sealed record FinLedgerRowDto(
    Guid AccountId, string AccountCode, string AccountName,
    decimal Debit, decimal Credit, decimal Balance);

public sealed record FinDetailLedgerRowDto(
    Guid JournalId, string JournalCode, DateTimeOffset EntryDate, string Description,
    Guid AccountId, string AccountCode, decimal Debit, decimal Credit,
    string? PartnerCode, Guid? CostCenterId, string? CostCenterName);

public sealed record FinLedgerQuery(Guid? AccountId, string? PartnerCode, Guid? CostCenterId, Guid? PeriodId);
