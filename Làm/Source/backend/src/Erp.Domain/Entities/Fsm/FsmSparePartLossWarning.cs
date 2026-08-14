using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Cảnh báo thất thoát & bất thường tồn kho linh kiện KTV (UC_FSM_040).</summary>
public class FsmSparePartLossWarning : TenantEntity
{
    public Guid TechnicianUserId { get; set; }
    public string TechnicianName { get; set; } = "";
    public string PartCode { get; set; } = "";
    public string PartName { get; set; } = "";
    public decimal IssuedQuantity { get; set; }
    public decimal UsedQuantity { get; set; }
    public decimal ReturnedQuantity { get; set; }
    public decimal DiscrepancyLossQty { get; set; }
    public string LossSeverity { get; set; } = "Warning"; // Warning | Critical
    public DateTimeOffset WarningGeneratedAt { get; set; } = DateTimeOffset.UtcNow;
}
