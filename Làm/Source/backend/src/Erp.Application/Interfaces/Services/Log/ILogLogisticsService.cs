using Erp.Application.DTOs.Log;

namespace Erp.Application.Interfaces.Services.Log;

public interface ILogLogisticsService
{
    Task<IReadOnlyList<LogCarrierDto>> ListCarriersAsync(Guid tenantId, string? q, CancellationToken ct = default);
    Task<LogCarrierDto> UpsertCarrierAsync(Guid tenantId, Guid userId, LogCarrierUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<LogDeliveryOrderDto>> ListDeliveriesAsync(Guid tenantId, string? q, CancellationToken ct = default);
    Task<LogDeliveryDetailDto> GetDeliveryDetailAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<LogDeliveryOrderDto> UpsertDeliveryAsync(Guid tenantId, Guid userId, LogDeliveryUpsertRequest req, CancellationToken ct = default);
    Task<LogDeliveryLineDto> UpsertLineAsync(Guid tenantId, Guid userId, Guid orderId, LogDeliveryLineUpsertRequest req, CancellationToken ct = default);
    Task<LogDeliveryOrderDto> ConfirmAsync(Guid tenantId, Guid userId, Guid orderId, CancellationToken ct = default);
    Task<LogDeliveryOrderDto> SplitBatchAsync(Guid tenantId, Guid userId, Guid orderId, LogSplitBatchRequest req, CancellationToken ct = default);
    Task<LogDeliveryOrderDto> StartPickAsync(Guid tenantId, Guid userId, Guid orderId, CancellationToken ct = default);
    Task<LogDeliveryOrderDto> ConfirmPickAsync(Guid tenantId, Guid userId, Guid orderId, LogPickRequest req, CancellationToken ct = default);
    Task<LogDeliveryOrderDto> DispatchAsync(Guid tenantId, Guid userId, Guid orderId, CancellationToken ct = default);
    Task<LogDeliveryOrderDto> PrintWaybillAsync(Guid tenantId, Guid userId, Guid orderId, CancellationToken ct = default);
    Task<LogDeliveryOrderDto> AssignAsync(Guid tenantId, Guid userId, Guid orderId, LogAssignRequest req, CancellationToken ct = default);
    Task<LogDeliveryOrderDto> UpdateStatusAsync(Guid tenantId, Guid userId, Guid orderId, LogStatusRequest req, CancellationToken ct = default);
    Task<LogDeliveryOrderDto> CancelAsync(Guid tenantId, Guid userId, Guid orderId, LogStatusRequest req, CancellationToken ct = default);
    Task<LogDeliveryOrderDto> ReturnAsync(Guid tenantId, Guid userId, Guid orderId, LogStatusRequest req, CancellationToken ct = default);
    Task<LogDeliveryOrderDto> FailAsync(Guid tenantId, Guid userId, Guid orderId, LogFailRequest req, CancellationToken ct = default);
}
