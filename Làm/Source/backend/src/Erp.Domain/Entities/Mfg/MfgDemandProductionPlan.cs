using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Kế hoạch sản xuất theo nhu cầu MPS (UC_MFG_012).</summary>
public class MfgDemandProductionPlan : TenantEntity
{
    public string PlanNumber { get; set; } = "";
    public string PlanName { get; set; } = "";
    public Guid FinishedProductId { get; set; }
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public decimal SalesForecastDemandQty { get; set; }
    public decimal BacklogOrdersDemandQty { get; set; }
    public decimal PlannedProductionQty { get; set; }
    public string PlanningHorizon { get; set; } = "Monthly-2026-09";
    public string Status { get; set; } = "Draft"; // Draft | Approved | Executing
    public DateTimeOffset CreatedAtDate { get; set; } = DateTimeOffset.UtcNow;
}
