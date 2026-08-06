using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Lịch sử chat omnichannel (UC_CRM_047).</summary>
public class CrmChatHistory : TenantEntity
{
    /// <summary>Facebook · Zalo · WebChat · WhatsApp · Line</summary>
    public string Channel { get; set; } = "WebChat";
    public string? ExternalConversationId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? AgentUserId { get; set; }
    /// <summary>Inbound · Outbound</summary>
    public string Direction { get; set; } = "Inbound";
    public string MessageText { get; set; } = "";
    public string? AttachmentUrl { get; set; }
    public DateTimeOffset SentAt { get; set; } = DateTimeOffset.UtcNow;
}
