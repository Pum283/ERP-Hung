using Erp.Application.DTOs.Prt;

namespace Erp.Application.Interfaces.Services.Prt;

public interface IPrtPortalService
{
    Task<IReadOnlyList<PrtAccountDto>> ListAccountsAsync(Guid tenantId, string? q, CancellationToken ct = default);
    Task<PrtAccountDto> UpsertAccountAsync(Guid tenantId, Guid userId, PrtAccountUpsertRequest req, CancellationToken ct = default);
    Task<PrtAccountDto> RegisterAsync(Guid tenantId, Guid userId, PrtRegisterRequest req, CancellationToken ct = default);
    Task<PrtLoginResultDto> LoginStubAsync(Guid tenantId, PrtLoginRequest req, CancellationToken ct = default);
    Task<PrtAccountDto> ForgotPasswordStubAsync(Guid tenantId, PrtForgotPasswordRequest req, CancellationToken ct = default);
    Task<PrtAccountDto> LinkCustomerAsync(Guid tenantId, Guid userId, PrtLinkCustomerRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<PrtOrderDto>> ListOrdersAsync(Guid tenantId, Guid? accountId, CancellationToken ct = default);
    Task<PrtOrderDetailDto> GetOrderDetailAsync(Guid tenantId, Guid orderId, CancellationToken ct = default);
    Task<PrtOrderDto> UpsertOrderAsync(Guid tenantId, Guid userId, PrtOrderUpsertRequest req, CancellationToken ct = default);

    Task<PrtArSummaryDto> GetArSummaryAsync(Guid tenantId, Guid accountId, CancellationToken ct = default);
    Task<IReadOnlyList<PrtInvoiceDto>> ListInvoicesAsync(Guid tenantId, Guid accountId, bool openOnly, CancellationToken ct = default);
    Task<PrtInvoiceDto> UpsertInvoiceAsync(Guid tenantId, Guid userId, PrtInvoiceUpsertRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PrtPaymentDto>> ListPaymentsAsync(Guid tenantId, Guid accountId, CancellationToken ct = default);
    Task<PrtPaymentDto> UpsertPaymentAsync(Guid tenantId, Guid userId, PrtPaymentUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<PrtTicketDto>> ListTicketsAsync(Guid tenantId, Guid? accountId, CancellationToken ct = default);
    Task<PrtTicketDto> UpsertTicketAsync(Guid tenantId, Guid userId, PrtTicketUpsertRequest req, CancellationToken ct = default);
}
