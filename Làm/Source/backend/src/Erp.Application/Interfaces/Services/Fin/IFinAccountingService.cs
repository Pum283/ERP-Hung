using Erp.Application.DTOs.Fin;

namespace Erp.Application.Interfaces.Services.Fin;

public interface IFinAccountingService
{
    Task<IReadOnlyList<FinAccountGroupDto>> ListGroupsAsync(Guid tenantId, CancellationToken ct = default);
    Task<FinAccountGroupDto> UpsertGroupAsync(Guid tenantId, Guid userId, FinAccountGroupUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<FinAccountDto>> ListAccountsAsync(Guid tenantId, string? q, CancellationToken ct = default);
    Task<FinAccountDto> UpsertAccountAsync(Guid tenantId, Guid userId, FinAccountUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<FinFiscalYearDto>> ListFiscalYearsAsync(Guid tenantId, CancellationToken ct = default);
    Task<FinFiscalYearDto> UpsertFiscalYearAsync(Guid tenantId, Guid userId, FinFiscalYearUpsertRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<FinPeriodDto>> ListPeriodsAsync(Guid tenantId, Guid? fiscalYearId, CancellationToken ct = default);
    Task<FinPeriodDto> SetPeriodLockAsync(Guid tenantId, Guid userId, Guid periodId, FinPeriodLockRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<FinCostCenterDto>> ListCostCentersAsync(Guid tenantId, CancellationToken ct = default);
    Task<FinCostCenterDto> UpsertCostCenterAsync(Guid tenantId, Guid userId, FinCostCenterUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<FinPaymentMethodDto>> ListPaymentMethodsAsync(Guid tenantId, CancellationToken ct = default);
    Task<FinPaymentMethodDto> UpsertPaymentMethodAsync(Guid tenantId, Guid userId, FinPaymentMethodUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<FinTaxDto>> ListTaxesAsync(Guid tenantId, CancellationToken ct = default);
    Task<FinTaxDto> UpsertTaxAsync(Guid tenantId, Guid userId, FinTaxUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<FinJournalDto>> ListJournalsAsync(Guid tenantId, string? q, CancellationToken ct = default);
    Task<FinJournalDetailDto> GetJournalDetailAsync(Guid tenantId, Guid journalId, CancellationToken ct = default);
    Task<FinJournalDto> UpsertJournalAsync(Guid tenantId, Guid userId, FinJournalUpsertRequest req, CancellationToken ct = default);
    Task<FinJournalDto> PostJournalAsync(Guid tenantId, Guid userId, Guid journalId, CancellationToken ct = default);
    Task<FinJournalDto> ReverseJournalAsync(Guid tenantId, Guid userId, Guid journalId, CancellationToken ct = default);
    Task<FinJournalDto> CreateAutoJournalStubAsync(Guid tenantId, Guid userId, FinJournalUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<FinLedgerRowDto>> GetLedgerAsync(Guid tenantId, FinLedgerQuery query, CancellationToken ct = default);
    Task<IReadOnlyList<FinDetailLedgerRowDto>> GetDetailLedgerAsync(Guid tenantId, FinLedgerQuery query, CancellationToken ct = default);
}
