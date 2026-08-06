using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class ChatMessage : TenantEntity
{
    public Guid ConversationId { get; set; }
    public Guid SenderUserId { get; set; }
    public string Body { get; set; } = "";
    public Guid? AttachmentFileId { get; set; }
    /// <summary>Storage key từ /api/sys/files/upload (Digi FileURL tương đương).</summary>
    public string? AttachmentStorageKey { get; set; }
    public Guid? ParentMessageId { get; set; }
    public DateTimeOffset SentAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? RecalledAt { get; set; }
    public bool IsEdited { get; set; }
    public DateTimeOffset? EditedAt { get; set; }
}
