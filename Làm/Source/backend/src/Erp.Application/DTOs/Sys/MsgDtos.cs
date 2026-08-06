namespace Erp.Application.DTOs.Sys;

public sealed record ConversationDto(
    Guid Id, string Kind, string? Title, string? PeerDisplayName, Guid? PeerUserId,
    string? LastMessagePreview, DateTimeOffset? LastMessageAt, int UnreadCount, bool Muted);

public sealed record MessageReactionDto(
    Guid Id, Guid MessageId, Guid UserId, string DisplayName, string ReactionType);

public sealed record ChatMessageDto(
    Guid Id, Guid ConversationId, Guid SenderUserId, string SenderDisplayName,
    string Body, Guid? AttachmentFileId, string? AttachmentStorageKey, DateTimeOffset SentAt, bool Recalled,
    Guid? ParentMessageId, string? ParentPreview, bool IsEdited,
    IReadOnlyList<MessageReactionDto> Reactions);

public sealed record CreateConversationRequest(Guid? PeerUserId, string? Title, IReadOnlyList<Guid>? MemberIds);

public sealed record SendMessageRequest(
    string Body, Guid? AttachmentFileId = null, string? AttachmentStorageKey = null, Guid? ParentMessageId = null);

public sealed record EditMessageRequest(string Body);

public sealed record ToggleReactionRequest(string ReactionType);

public sealed record ReactionToggledDto(
    Guid MessageId, Guid ConversationId, Guid UserId, string DisplayName, string ReactionType, bool Removed);

public sealed record MuteConversationRequest(bool Muted);

public sealed record AddMembersRequest(IReadOnlyList<Guid> MemberIds);

public sealed record UnreadCountDto(int Count);

public sealed record MsgDirectoryUserDto(Guid Id, string Username, string DisplayName);

public sealed record ConversationMemberDto(Guid UserId, string DisplayName, string Username, bool IsSelf);
