using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>SP / giá trị ước tính trên cơ hội (UC_CRM_065).</summary>
public class CrmOpportunityLine : TenantEntity
{
    public Guid OpportunityId { get; set; }
    public string ItemCode { get; set; } = "";
    public string ItemName { get; set; } = "";
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal LineAmount { get; set; }
    public int LineNo { get; set; }
}
