using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Nguồn lead website / landing (UC_CRM_026).</summary>
public class CrmWebLead : TenantEntity
{
    public string? SourceUrl { get; set; }
    public string? LandingPage { get; set; }
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }
    public string? FormName { get; set; }
    public string ContactName { get; set; } = "";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Message { get; set; }
    /// <summary>Pending · Synced · Rejected</summary>
    public string SyncStatus { get; set; } = "Pending";
    public Guid? LeadId { get; set; }
    public Guid? CampaignId { get; set; }
}
