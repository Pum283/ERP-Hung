using Erp.Application.DTOs.Log;

namespace Erp.Application.Interfaces.Services.Log;

public interface ILogCodService
{
    Task<LogDeliveryOrderDto> MarkCodAsync(Guid tenantId, Guid userId, Guid orderId, LogCodMarkRequest req, CancellationToken ct = default);
    Task<LogDeliveryOrderDto> SetCodAmountAsync(Guid tenantId, Guid userId, Guid orderId, LogCodAmountRequest req, CancellationToken ct = default);
    Task<LogDeliveryOrderDto> ConfirmCollectedAsync(Guid tenantId, Guid userId, Guid orderId, LogCodCollectRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<LogDeliveryOrderDto>> ListCodDeliveriesAsync(Guid tenantId, string? status, CancellationToken ct = default);
    Task<IReadOnlyList<LogDeliveryOrderDto>> ListOverdueAsync(Guid tenantId, CancellationToken ct = default);
    Task<LogCodReportDto> GetReportAsync(Guid tenantId, CancellationToken ct = default);

    Task<IReadOnlyList<LogCodHandoverDto>> ListHandoversAsync(Guid tenantId, CancellationToken ct = default);
    Task<LogCodHandoverDetailDto> GetHandoverAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<LogCodHandoverDetailDto> CreateHandoverAsync(Guid tenantId, Guid userId, LogCodHandoverCreateRequest req, CancellationToken ct = default);
    Task<LogCodHandoverDetailDto> SubmitHandoverAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);
    Task<LogCodHandoverDetailDto> ReconcileHandoverAsync(Guid tenantId, Guid userId, Guid id, LogCodReconcileRequest req, CancellationToken ct = default);
    Task<LogCodHandoverDetailDto> ResolveVarianceAsync(Guid tenantId, Guid userId, Guid id, LogCodResolveVarianceRequest req, CancellationToken ct = default);
}
