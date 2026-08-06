using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Cơ hội bán hàng (UC_CRM_062–068).</summary>
public class CrmOpportunity : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public Guid? LeadId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? OwnerUserId { get; set; }
    /// <summary>Qualification · Proposal · Negotiation · Won · Lost</summary>
    public string Stage { get; set; } = "Qualification";
    public decimal EstimatedValue { get; set; }
    public decimal ProbabilityPercent { get; set; } = 20;
    public DateTimeOffset? ExpectedCloseDate { get; set; }
    public Guid? QuoteId { get; set; }
    public string? LostReason { get; set; }
    public string? Note { get; set; }
}
