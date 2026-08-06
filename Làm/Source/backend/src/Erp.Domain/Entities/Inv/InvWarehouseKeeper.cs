using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Gán thủ kho / quyền (UC_INV_014).</summary>
public class InvWarehouseKeeper : TenantEntity
{
    public Guid WarehouseId { get; set; }
    public Guid UserId { get; set; }
    /// <summary>Keeper · Supervisor</summary>
    public string Role { get; set; } = "Keeper";
    public bool IsActive { get; set; } = true;
}
