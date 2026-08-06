namespace Erp.Application.Interfaces.Realtime;

public interface IOutboxWriter
{
    Task EnqueueAsync(
        Guid tenantId,
        string eventType,
        string sourceModule,
        object payload,
        Guid? correlationId = null,
        CancellationToken ct = default);
}

public interface IInboxStore
{
    /// <returns>true nếu lần đầu xử lý; false nếu trùng.</returns>
    Task<bool> TryBeginProcessAsync(
        Guid tenantId,
        Guid eventId,
        string consumer,
        string eventType,
        CancellationToken ct = default);
}
