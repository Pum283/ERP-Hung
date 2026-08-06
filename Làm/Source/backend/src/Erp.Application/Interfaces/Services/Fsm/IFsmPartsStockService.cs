using Erp.Application.DTOs.Fsm;

namespace Erp.Application.Interfaces.Services.Fsm;

public interface IFsmPartsStockService
{
    Task<IReadOnlyList<FsmPartStockDto>> ListStockAsync(
        Guid tenantId, string? locationType, Guid? techUserId, CancellationToken ct = default);

    Task<FsmPartStockDto> ReceiptWarehouseAsync(
        Guid tenantId, Guid userId, FsmPartReceiptRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<FsmPartIssueDocDto>> ListIssuesAsync(Guid tenantId, CancellationToken ct = default);
    Task<FsmPartIssueDocDto> CreateAndPostIssueAsync(
        Guid tenantId, Guid userId, FsmPartIssueCreateRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<FsmPartReconcileDocDto>> ListReconcilesAsync(Guid tenantId, CancellationToken ct = default);
    Task<FsmPartReconcileDocDto> CreateAndPostReconcileAsync(
        Guid tenantId, Guid userId, FsmPartReconcileCreateRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<FsmTicketPartLineDto>> ListTicketPartsAsync(
        Guid tenantId, Guid ticketId, CancellationToken ct = default);

    Task<FsmTicketPartLineDto> ConsumeTicketPartAsync(
        Guid tenantId, Guid userId, Guid ticketId, FsmConsumePartRequest req, CancellationToken ct = default);
}
