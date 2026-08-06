namespace Erp.Application.Interfaces.Realtime;

/// <summary>Đẩy sự kiện inbox WF qua SignalR — không poll HTTP.</summary>
public interface IWfRealtimeNotifier
{
    Task NotifyInboxChangedAsync(Guid userId, string reason, Guid? taskId = null, CancellationToken ct = default);
}
