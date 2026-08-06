using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class ConversationMember : TenantEntity
{
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset? LastReadAt { get; set; }
    public bool Muted { get; set; }
}
