using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

public class PurPrLine : TenantEntity
{
    public Guid PrId { get; set; }
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public decimal Qty { get; set; }
    public string Unit { get; set; } = "cai";
    public string? Note { get; set; }
}
