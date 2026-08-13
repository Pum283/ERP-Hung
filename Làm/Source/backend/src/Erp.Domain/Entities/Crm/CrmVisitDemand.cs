using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Ghi nhận nhu cầu khách hàng tại điểm thăm (UC_CRM_094).</summary>
public class CrmVisitDemand : TenantEntity
{
    public Guid VisitPlanId { get; set; }
    public Guid CustomerId { get; set; }
    public string ProductInterestCategory { get; set; } = "";
    public int EstimatedQuantity { get; set; } = 1;
    /// <summary>Low | Medium | High</summary>
    public string Urgency { get; set; } = "Medium";
    public string CompetitorInfo { get; set; } = "";
    public string CustomerFeedback { get; set; } = "";
    public DateTimeOffset LoggedAt { get; set; } = DateTimeOffset.UtcNow;
}
