using Erp.Application.DTOs.Fin;

namespace Erp.Application.Interfaces.Services.Fin;

public interface IFinArService
{
    Task<IReadOnlyList<FinArInvoiceDto>> ListInvoicesAsync(Guid tenantId, Guid? customerId = null, string? status = null, CancellationToken ct = default);
    Task<FinArInvoiceDto> UpsertInvoiceAsync(Guid tenantId, Guid userId, FinArInvoiceUpsertRequest req, CancellationToken ct = default);
    Task<FinArInvoiceDto> PostInvoiceAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);
    Task<FinArInvoiceDto> VoidInvoiceAsync(Guid tenantId, Guid userId, Guid id, string? note = null, CancellationToken ct = default);

    Task<IReadOnlyList<FinArCustomerBalanceDto>> ListCustomerBalancesAsync(Guid tenantId, CancellationToken ct = default);

    Task<IReadOnlyList<FinArCreditLimitDto>> ListCreditLimitsAsync(Guid tenantId, CancellationToken ct = default);
    Task<FinArCreditLimitDto> UpsertCreditLimitAsync(Guid tenantId, Guid userId, FinArCreditLimitUpsertRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<FinArCreditLimitDto>> ListCreditAlertsAsync(Guid tenantId, CancellationToken ct = default);

    Task<IReadOnlyList<FinArReceiptDto>> ListReceiptsAsync(Guid tenantId, Guid? customerId = null, CancellationToken ct = default);
    Task<FinArReceiptDto> UpsertReceiptAsync(Guid tenantId, Guid userId, FinArReceiptUpsertRequest req, CancellationToken ct = default);
    Task<FinArReceiptDto> PostReceiptAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);

    Task<FinArAgingDto> GetAgingAsync(Guid tenantId, DateTimeOffset? asOf = null, CancellationToken ct = default);
}
