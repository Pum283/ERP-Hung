using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class ChatMessageReaction : TenantEntity
{
    public Guid MessageId { get; set; }
    public Guid UserId { get; set; }
    /// <summary>Emoji hoặc mã cảm xúc (ví dụ 👍, ❤️).</summary>
    public string ReactionType { get; set; } = "";
}
