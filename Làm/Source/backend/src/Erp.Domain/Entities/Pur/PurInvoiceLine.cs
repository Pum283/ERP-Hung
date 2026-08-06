using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

public class PurInvoiceLine : TenantEntity
{
    public Guid InvoiceId { get; set; }
    public Guid? PoLineId { get; set; }
    public Guid? GrnLineId { get; set; }
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public decimal Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineAmount { get; set; }
}
