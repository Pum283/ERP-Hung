using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Phân bổ nhân công trực tiếp & chi phí sản xuất chung (UC_MFG_028).</summary>
public class MfgOverheadCostAllocation : TenantEntity
{
    public string AllocationNumber { get; set; } = "";
    public Guid WorkOrderId { get; set; }
    public string WorkOrderNumber { get; set; } = "";
    public decimal DirectLaborCostVnd { get; set; }
    public decimal MachineDepreciationCostVnd { get; set; }
    public decimal FactoryOverheadCostVnd { get; set; }
    public decimal TotalAllocatedCostVnd { get; set; }
    public decimal ProducedQuantity { get; set; }
    public decimal UnitCostVnd { get; set; }
    public DateTimeOffset AllocatedAt { get; set; } = DateTimeOffset.UtcNow;
}
