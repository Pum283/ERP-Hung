using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

public class PurPoLine : TenantEntity
{
    public Guid PoId { get; set; }
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public decimal Qty { get; set; }
    public decimal ReceivedQty { get; set; }
    public decimal InvoicedQty { get; set; }
    public decimal UnitPrice { get; set; }
    public string Unit { get; set; } = "cai";
}
