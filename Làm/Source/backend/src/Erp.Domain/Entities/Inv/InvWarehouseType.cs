using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Loại kho (UC_INV_012).</summary>
public class InvWarehouseType : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
