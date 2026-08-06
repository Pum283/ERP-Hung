using Erp.Application.DTOs.Sys;

namespace Erp.Application.Interfaces.Services.Sys;

public interface IMsgService
{
    Task<IReadOnlyList<ConversationDto>> ListConversationsAsync(Guid tenantId, Guid userId, CancellationToken ct = default);
    Task<ConversationDto> CreateConversationAsync(Guid tenantId, Guid userId, CreateConversationRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<ChatMessageDto>> ListMessagesAsync(Guid tenantId, Guid userId, Guid conversationId, DateTimeOffset? before, int take, CancellationToken ct = default);
    Task<ChatMessageDto> SendMessageAsync(Guid tenantId, Guid userId, Guid conversationId, SendMessageRequest req, CancellationToken ct = default);
    Task<ChatMessageDto> EditMessageAsync(Guid tenantId, Guid userId, Guid conversationId, Guid messageId, EditMessageRequest req, CancellationToken ct = default);
    Task MarkReadAsync(Guid tenantId, Guid userId, Guid conversationId, CancellationToken ct = default);
    Task SetMutedAsync(Guid tenantId, Guid userId, Guid conversationId, bool muted, CancellationToken ct = default);
    Task<UnreadCountDto> GetUnreadCountAsync(Guid tenantId, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<MsgDirectoryUserDto>> ListDirectoryAsync(Guid tenantId, Guid userId, CancellationToken ct = default);
    Task<ChatMessageDto> RecallMessageAsync(Guid tenantId, Guid userId, Guid conversationId, Guid messageId, CancellationToken ct = default);
    Task<ReactionToggledDto> ToggleReactionAsync(Guid tenantId, Guid userId, Guid conversationId, Guid messageId, ToggleReactionRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<ConversationMemberDto>> ListMembersAsync(Guid tenantId, Guid userId, Guid conversationId, CancellationToken ct = default);
    Task AddMembersAsync(Guid tenantId, Guid userId, Guid conversationId, IReadOnlyList<Guid> memberIds, CancellationToken ct = default);
    Task RemoveMemberAsync(Guid tenantId, Guid actorId, Guid conversationId, Guid memberUserId, CancellationToken ct = default);
}
