using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Dòng SP/DV trên báo giá (UC_CRM_071).</summary>
public class CrmQuoteLine : TenantEntity
{
    public Guid QuoteId { get; set; }
    public string ItemCode { get; set; } = "";
    public string ItemName { get; set; } = "";
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal LineAmount { get; set; }
    public int LineNo { get; set; }
}
