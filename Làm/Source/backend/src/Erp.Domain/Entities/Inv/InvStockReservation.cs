using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Giữ hàng soft theo Ref đơn (UC_INV_037–038).</summary>
public class InvStockReservation : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid WarehouseId { get; set; }
    /// <summary>Draft · Active · Released · Consumed</summary>
    public string Status { get; set; } = "Draft";
    public string? RefModule { get; set; }
    public Guid? RefId { get; set; }
    public string? RefCode { get; set; }
    public string? Note { get; set; }
    public DateTimeOffset? ActivatedAt { get; set; }
    public DateTimeOffset? ReleasedAt { get; set; }
    public ICollection<InvStockReservationLine> Lines { get; set; } = new List<InvStockReservationLine>();
}

public class InvStockReservationLine : TenantEntity
{
    public Guid ReservationId { get; set; }
    public Guid SkuId { get; set; }
    public string SkuCode { get; set; } = "";
    public string SkuName { get; set; } = "";
    public decimal Qty { get; set; }
    public string? LotCode { get; set; }
    public DateOnly? ExpiryDate { get; set; }
}
