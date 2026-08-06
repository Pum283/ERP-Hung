using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Phân quyền thu ngân trên điểm bán (UC_POS_007).</summary>
public class PosCashierAssignment : TenantEntity
{
    public Guid StoreId { get; set; }
    public Guid UserId { get; set; }
    /// <summary>Cashier · Supervisor</summary>
    public string Role { get; set; } = "Cashier";
    public bool IsActive { get; set; } = true;
}
