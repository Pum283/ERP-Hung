using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

public class PurGrnLine : TenantEntity
{
    public Guid GrnId { get; set; }
    public Guid? PoLineId { get; set; }
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public decimal OrderedQty { get; set; }
    public decimal ReceivedQty { get; set; }
    public decimal AcceptedQty { get; set; }
    public decimal RejectedQty { get; set; }
    public string Unit { get; set; } = "cai";
    public decimal UnitPrice { get; set; }
}
