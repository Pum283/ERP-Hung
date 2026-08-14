using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Vị trí ô kệ bin trong kho (UC_INV_013).</summary>
public class InvWarehouseBinLocation : TenantEntity
{
    public Guid WarehouseId { get; set; }
    public string LocationCode { get; set; } = ""; // E.g., A-01-02-B03 (Zone A, Aisle 1, Rack 2, Bin 3)
    public string ZoneName { get; set; } = "Zone A";
    public string Aisle { get; set; } = "Aisle 01";
    public string Rack { get; set; } = "Rack 02";
    public string ShelfBin { get; set; } = "Bin 03";
    public bool IsActive { get; set; } = true;
}
