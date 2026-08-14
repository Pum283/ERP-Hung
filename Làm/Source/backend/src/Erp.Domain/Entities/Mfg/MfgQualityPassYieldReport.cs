using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Báo cáo tỷ lệ đạt chuẩn QC và First-Pass Yield (UC_MFG_036).</summary>
public class MfgQualityPassYieldReport : TenantEntity
{
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public int TotalInspectedLotsCount { get; set; }
    public decimal TotalInspectedQuantity { get; set; }
    public decimal TotalPassedQuantity { get; set; }
    public decimal TotalRejectedQuantity { get; set; }
    public double QualityPassRatePct { get; set; } = 97.5;
    public double FirstPassYieldRatePct { get; set; } = 95.0;
    public DateTimeOffset PeriodStartDate { get; set; }
    public DateTimeOffset PeriodEndDate { get; set; }
}
