using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Hộp thư tập trung đa kênh & Tiếp nhận hội thoại (UC_CRM_039, UC_CRM_040).</summary>
public class CrmOmnichannelConversation : TenantEntity
{
    /// <summary>Zalo | Facebook | Email | LiveChat</summary>
    public string Channel { get; set; } = "Zalo";
    public string ExternalId { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string CustomerPhone { get; set; } = "";
    public Guid? AssignedAgentId { get; set; }
    /// <summary>New | Assigned | Closed</summary>
    public string Status { get; set; } = "New";
    public string LastMessageSnippet { get; set; } = "";
    public DateTimeOffset LastMessageAt { get; set; } = DateTimeOffset.UtcNow;
}
