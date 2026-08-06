using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

public class InvStockDocLine : TenantEntity
{
    public Guid DocId { get; set; }
    public Guid SkuId { get; set; }
    public string SkuCode { get; set; } = "";
    public string SkuName { get; set; } = "";
    public decimal Qty { get; set; }
    public string? LotCode { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public decimal UnitCost { get; set; }
}
