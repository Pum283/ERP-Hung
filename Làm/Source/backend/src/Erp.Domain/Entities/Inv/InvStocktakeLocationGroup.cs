using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Kiểm kê theo vị trí / nhóm sản phẩm (UC_INV_051).</summary>
public class InvStocktakeLocationGroup : TenantEntity
{
    public string StocktakeCode { get; set; } = "";
    public Guid WarehouseId { get; set; }
    public string ScopeType { get; set; } = "ByLocation"; // ByLocation | ByProductGroup
    public string ScopeTarget { get; set; } = "Zone A"; // E.g., Zone A or Hàng Điện Tử
    public int PlannedItemsCount { get; set; }
    public int CountedItemsCount { get; set; }
    public string Status { get; set; } = "InProgress"; // InProgress | Completed
    public DateTimeOffset ScheduledDate { get; set; } = DateTimeOffset.UtcNow;
}
