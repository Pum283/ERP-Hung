using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Kiểm tra chất lượng thành phẩm hoàn thiện FQC (UC_MFG_033).</summary>
public class MfgFinishedGoodsQcCheck : TenantEntity
{
    public string InspectionNumber { get; set; } = "";
    public Guid WorkOrderId { get; set; }
    public string WorkOrderNumber { get; set; } = "";
    public string FinishedProductCode { get; set; } = "";
    public decimal SampleSizeQty { get; set; }
    public decimal DefectFoundQty { get; set; }
    public string InspectionResult { get; set; } = "Pass"; // Pass | Fail | ConditionalPass
    public string InspectorName { get; set; } = "";
    public DateTimeOffset InspectedAt { get; set; } = DateTimeOffset.UtcNow;
}
