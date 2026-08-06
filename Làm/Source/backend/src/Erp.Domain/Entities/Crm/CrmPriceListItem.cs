using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

public class CrmPriceListItem : TenantEntity
{
    public Guid PriceListId { get; set; }
    public string ItemCode { get; set; } = "";
    public string ItemName { get; set; } = "";
    public decimal UnitPrice { get; set; }
}
