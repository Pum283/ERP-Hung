using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Chỉ số hiệu suất thiết bị tổng thể OEE (UC_MFG_044).</summary>
public class MfgOverallEquipmentEffectiveness : TenantEntity
{
    public string WorkCenterCode { get; set; } = "";
    public string WorkCenterName { get; set; } = "";
    public double AvailabilityRatePct { get; set; } = 92.5; // Sẵn sàng %
    public double PerformanceRatePct { get; set; } = 88.0;  // Hiệu suất vận hành %
    public double QualityRatePct { get; set; } = 98.0;      // Chất lượng %
    public double OverallOeePct { get; set; } = 79.77;      // Availability * Performance * Quality
    public DateTimeOffset CalculationPeriod { get; set; } = DateTimeOffset.UtcNow;
}
