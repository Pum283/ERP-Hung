using Erp.Application.DTOs.Fin;

namespace Erp.Application.Interfaces.Services.Fin;

public interface IFinApService
{
    Task<IReadOnlyList<FinApInvoiceDto>> ListInvoicesAsync(Guid tenantId, Guid? vendorId = null, string? status = null, CancellationToken ct = default);
    Task<FinApInvoiceDto> UpsertInvoiceAsync(Guid tenantId, Guid userId, FinApInvoiceUpsertRequest req, CancellationToken ct = default);
    Task<FinApInvoiceDto> PostInvoiceAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);
    Task<FinApInvoiceDto> VoidInvoiceAsync(Guid tenantId, Guid userId, Guid id, string? note = null, CancellationToken ct = default);

    Task<IReadOnlyList<FinApVendorBalanceDto>> ListVendorBalancesAsync(Guid tenantId, CancellationToken ct = default);

    Task<IReadOnlyList<FinApPaymentRequestDto>> ListPaymentRequestsAsync(Guid tenantId, Guid? vendorId = null, CancellationToken ct = default);
    Task<FinApPaymentRequestDto> UpsertPaymentRequestAsync(Guid tenantId, Guid userId, FinApPaymentRequestUpsertRequest req, CancellationToken ct = default);
    Task<FinApPaymentRequestDto> SubmitPaymentRequestAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);
    Task<FinApPaymentRequestDto> ApprovePaymentRequestAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);
    Task<FinApPaymentRequestDto> RejectPaymentRequestAsync(Guid tenantId, Guid userId, Guid id, string? note = null, CancellationToken ct = default);
    Task<FinApPaymentRequestDto> VoidPaymentRequestAsync(Guid tenantId, Guid userId, Guid id, string? note = null, CancellationToken ct = default);

    Task<IReadOnlyList<FinApPaymentDto>> ListPaymentsAsync(Guid tenantId, Guid? vendorId = null, CancellationToken ct = default);
    Task<FinApPaymentDto> UpsertPaymentAsync(Guid tenantId, Guid userId, FinApPaymentUpsertRequest req, CancellationToken ct = default);
    Task<FinApPaymentDto> PostPaymentAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);
    Task<FinApPaymentDto> PayFromRequestAsync(Guid tenantId, Guid userId, Guid requestId, CancellationToken ct = default);

    Task<FinApAgingDto> GetAgingAsync(Guid tenantId, DateTimeOffset? asOf = null, CancellationToken ct = default);
}
