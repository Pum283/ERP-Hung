using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Tồn theo kho/SKU/lô (UC_INV_039–041).</summary>
public class InvStockBalance : TenantEntity
{
    public Guid WarehouseId { get; set; }
    public Guid SkuId { get; set; }
    public string? LotCode { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public decimal QtyOnHand { get; set; }
    public decimal QtyReserved { get; set; }
    public decimal QtyInTransit { get; set; }
}
