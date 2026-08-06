using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Phiếu kiểm kê (UC_INV_049–053).</summary>
public class InvStocktake : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid WarehouseId { get; set; }
    /// <summary>Draft · Counting · Reviewed · Posted</summary>
    public string Status { get; set; } = "Draft";
    public DateTimeOffset? CountedAt { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public string? Note { get; set; }
}
