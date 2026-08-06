using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Lead bán hàng (UC_CRM_049–061).</summary>
public class CrmLead : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? CompanyName { get; set; }
    public Guid? SourceId { get; set; }
    public Guid? OwnerUserId { get; set; }
    public Guid? CustomerId { get; set; }
    /// <summary>New · Contacted · Qualified · Converted · Lost</summary>
    public string PipelineStatus { get; set; } = "New";
    /// <summary>0–100 scoring light</summary>
    public int Score { get; set; }
    public DateTimeOffset? NextFollowUpAt { get; set; }
    public string? LostReason { get; set; }
    public Guid? OpportunityId { get; set; }
    public Guid? MergedIntoId { get; set; }
    public string? Note { get; set; }
    /// <summary>Manual · Auto</summary>
    public string IntakeChannel { get; set; } = "Manual";
}
