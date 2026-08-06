using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>SKU sản phẩm (UC_INV_001, 004–005, 007, 010).</summary>
public class InvSku : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public Guid? GroupId { get; set; }
    public Guid BaseUnitId { get; set; }
    public bool TrackLot { get; set; }
    public bool TrackSerial { get; set; }
    public bool TrackExpiry { get; set; }
    /// <summary>Average · Fifo</summary>
    public string CostingMethod { get; set; } = "Average";
    public decimal StandardCost { get; set; }
    /// <summary>Active · Inactive</summary>
    public string Status { get; set; } = "Active";
    public decimal? MinQty { get; set; }
    public decimal? MaxQty { get; set; }
    public decimal? ReorderQty { get; set; }
    public string? Note { get; set; }
}
