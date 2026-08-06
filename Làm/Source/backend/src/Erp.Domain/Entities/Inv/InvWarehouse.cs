using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Kho (UC_INV_011, 015–016).</summary>
public class InvWarehouse : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public Guid? WarehouseTypeId { get; set; }
    public string? Address { get; set; }
    /// <summary>Active · Inactive</summary>
    public string Status { get; set; } = "Active";
    /// <summary>Fifo · Fefo</summary>
    public string PickPolicy { get; set; } = "Fifo";
    public bool AllowNegativeStock { get; set; }
}
