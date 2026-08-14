using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Khóa giao dịch khi đang kiểm kê (UC_INV_054).</summary>
public class InvStocktakeLock : TenantEntity
{
    public Guid WarehouseId { get; set; }
    public string LockScope { get; set; } = "FullWarehouse"; // FullWarehouse | SpecificZone | SpecificCategory
    public string TargetIdentifier { get; set; } = "Warehouse-All";
    public bool IsLocked { get; set; } = true;
    public string LockedBy { get; set; } = "";
    public string LockReason { get; set; } = "Đang kiểm kê định kỳ cuối tháng";
    public DateTimeOffset LockedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UnlockedAt { get; set; }
}
