using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Quản lý lô/mẻ sản xuất (UC_MFG_037).</summary>
public class MfgProductionBatchLot : TenantEntity
{
    public string BatchNumber { get; set; } = "";
    public Guid WorkOrderId { get; set; }
    public string WorkOrderNumber { get; set; } = "";
    public string ProductCode { get; set; } = "";
    public decimal BatchSizePlannedQty { get; set; }
    public decimal BatchSizeActualQty { get; set; }
    public DateTimeOffset ManufacturingDate { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiryDate { get; set; } = DateTimeOffset.UtcNow.AddYears(2);
    public string Status { get; set; } = "InProduction"; // InProduction | Completed | Closed
}
