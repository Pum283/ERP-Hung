using Erp.Application.DTOs.Fsm;

namespace Erp.Application.Interfaces.Services.Fsm;

public interface IFsmFieldService
{
    Task<IReadOnlyList<FsmServiceTypeDto>> ListServiceTypesAsync(Guid tenantId, CancellationToken ct = default);
    Task<FsmServiceTypeDto> UpsertServiceTypeAsync(Guid tenantId, Guid userId, FsmServiceTypeUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<FsmFaultCodeDto>> ListFaultCodesAsync(Guid tenantId, CancellationToken ct = default);
    Task<FsmFaultCodeDto> UpsertFaultCodeAsync(Guid tenantId, Guid userId, FsmFaultCodeUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<FsmPartDto>> ListPartsAsync(Guid tenantId, CancellationToken ct = default);
    Task<FsmPartDto> UpsertPartAsync(Guid tenantId, Guid userId, FsmPartUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<FsmSlaPolicyDto>> ListSlaPoliciesAsync(Guid tenantId, CancellationToken ct = default);
    Task<FsmSlaPolicyDto> UpsertSlaPolicyAsync(Guid tenantId, Guid userId, FsmSlaPolicyUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<FsmAssetDto>> ListAssetsAsync(Guid tenantId, string? q, CancellationToken ct = default);
    Task<FsmAssetDetailDto> GetAssetDetailAsync(Guid tenantId, Guid assetId, CancellationToken ct = default);
    Task<FsmAssetDto> UpsertAssetAsync(Guid tenantId, Guid userId, FsmAssetUpsertRequest req, CancellationToken ct = default);
    Task<FsmAssetHistoryDto> AddAssetHistoryAsync(Guid tenantId, Guid userId, Guid assetId, FsmAssetHistoryCreateRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<FsmTicketDto>> ListTicketsAsync(Guid tenantId, string? q, CancellationToken ct = default);
    Task<FsmTicketDetailDto> GetTicketDetailAsync(Guid tenantId, Guid ticketId, CancellationToken ct = default);
    Task<FsmTicketDto> UpsertTicketAsync(Guid tenantId, Guid userId, FsmTicketUpsertRequest req, CancellationToken ct = default);
    Task<FsmTicketDto> AssignTicketAsync(Guid tenantId, Guid userId, Guid ticketId, FsmAssignRequest req, CancellationToken ct = default);
    Task<FsmTicketDto> EscalateTicketAsync(Guid tenantId, Guid userId, Guid ticketId, FsmEscalateRequest req, CancellationToken ct = default);
    Task<FsmTicketDto> SetTicketStatusAsync(Guid tenantId, Guid userId, Guid ticketId, FsmTicketStatusRequest req, CancellationToken ct = default);

    Task<FsmTicketDto> SetAppointmentAsync(Guid tenantId, Guid userId, Guid ticketId, FsmAppointmentRequest req, CancellationToken ct = default);
    Task<FsmTicketDto> WorkLogAsync(Guid tenantId, Guid userId, Guid ticketId, FsmWorkLogRequest req, CancellationToken ct = default);
    Task<FsmTicketDto> CheckoutAsync(Guid tenantId, Guid userId, Guid ticketId, FsmCheckoutRequest req, CancellationToken ct = default);
    Task<FsmTicketDto> AcceptAsync(Guid tenantId, Guid userId, Guid ticketId, FsmAcceptRequest req, CancellationToken ct = default);
    Task<FsmTicketDto> CloseAsync(Guid tenantId, Guid userId, Guid ticketId, FsmCloseRequest req, CancellationToken ct = default);
}
