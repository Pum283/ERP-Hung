using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Chiến dịch marketing (UC_CRM_016, 019, 023).</summary>
public class CrmCampaign : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    /// <summary>Email · Social · SEM · Event · Other</summary>
    public string Channel { get; set; } = "Other";
    /// <summary>Draft · Active · Paused · Closed</summary>
    public string Status { get; set; } = "Draft";
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public decimal BudgetAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public Guid? OwnerUserId { get; set; }
    public int LeadCount { get; set; }
    public decimal RevenueGenerated { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public string? ClosedReason { get; set; }
}
