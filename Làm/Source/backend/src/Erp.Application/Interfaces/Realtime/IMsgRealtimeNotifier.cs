namespace Erp.Application.Interfaces.Realtime;

public interface IMsgRealtimeNotifier
{
    Task MessageReceivedAsync(IEnumerable<Guid> userIds, object payload, CancellationToken ct = default);
    Task MessageEditedAsync(IEnumerable<Guid> userIds, object payload, CancellationToken ct = default);
    Task ReactionToggledAsync(IEnumerable<Guid> userIds, object payload, CancellationToken ct = default);
    Task ConversationUpdatedAsync(IEnumerable<Guid> userIds, object payload, CancellationToken ct = default);
    Task TypingAsync(Guid conversationId, object payload, CancellationToken ct = default);
}
