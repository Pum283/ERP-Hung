using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Task / nhắc follow-up lead (UC_CRM_054–055).</summary>
public class CrmLeadTask : TenantEntity
{
    public Guid LeadId { get; set; }
    public string Title { get; set; } = "";
    public DateTimeOffset DueAt { get; set; }
    public Guid? AssigneeUserId { get; set; }
    /// <summary>Open · Done · Cancelled</summary>
    public string Status { get; set; } = "Open";
    public bool IsReminder { get; set; }
    public string? Note { get; set; }
}
