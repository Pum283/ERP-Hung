using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

public class CrmSalesOrderLine : TenantEntity
{
    public Guid OrderId { get; set; }
    public string ItemCode { get; set; } = "";
    public string ItemName { get; set; } = "";
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal LineAmount { get; set; }
    public int LineNo { get; set; }
}
