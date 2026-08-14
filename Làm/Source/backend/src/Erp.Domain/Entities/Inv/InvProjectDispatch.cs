using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Xuất kho cho dự án (UC_INV_028).</summary>
public class InvProjectDispatch : TenantEntity
{
    public string DispatchNumber { get; set; } = "";
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = "";
    public Guid WarehouseId { get; set; }
    public decimal TotalAllocatedValueVnd { get; set; }
    public string ProjectPhase { get; set; } = "Phase 1 - Triển khai thi công";
    public DateTimeOffset DispatchedAt { get; set; } = DateTimeOffset.UtcNow;
}
