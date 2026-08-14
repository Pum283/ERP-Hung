using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Đối chiếu chi phí sản xuất định mức lý thuyết vs chi phí thực tế (UC_MFG_030).</summary>
public class MfgCostVarianceAnalysis : TenantEntity
{
    public string AnalysisNumber { get; set; } = "";
    public Guid WorkOrderId { get; set; }
    public string WorkOrderNumber { get; set; } = "";
    public decimal StandardTheoreticalCostVnd { get; set; }
    public decimal ActualIncurredCostVnd { get; set; }
    public decimal CostVarianceVnd { get; set; } // Actual - Standard
    public double VariancePercentage { get; set; }
    public string VarianceRootCause { get; set; } = "Hao hụt NVL và tăng giờ công do máy dừng";
    public DateTimeOffset AnalyzedAt { get; set; } = DateTimeOffset.UtcNow;
}
