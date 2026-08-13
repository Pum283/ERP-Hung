using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>SLA phản hồi & cảnh báo (UC_CRM_043).</summary>
public class CrmChatSlaAlert : TenantEntity
{
    public Guid ConversationId { get; set; }
    public int MaxResponseMinutes { get; set; } = 5;
    public int ActualResponseMinutes { get; set; }
    public bool IsBreached { get; set; }
    /// <summary>Normal | Warning | Breached</summary>
    public string AlertStatus { get; set; } = "Normal";
    public DateTimeOffset BreachedAt { get; set; } = DateTimeOffset.UtcNow;
}
