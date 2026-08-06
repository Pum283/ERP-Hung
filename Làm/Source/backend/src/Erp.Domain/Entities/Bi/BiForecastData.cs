using Erp.Domain.Base;

namespace Erp.Domain.Entities.Bi;

/// <summary>Thực thể dự báo & phát hiện bất thường BI (UC_BI_027, 028, 029, 030).</summary>
public class BiForecastData : TenantEntity
{
    public string ForecastType { get; set; } = "Revenue"; // Revenue | Demand | Anomaly
    public decimal ProjectedValue { get; set; }
    public bool IsAnomalyDetected { get; set; } = false;
    public string AiInsightSummary { get; set; } = string.Empty;
    public DateTimeOffset PeriodDate { get; set; } = DateTimeOffset.UtcNow;
}
