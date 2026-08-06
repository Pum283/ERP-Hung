using Erp.Domain.Base;

namespace Erp.Domain.Entities.Bi;

/// <summary>Ngưỡng cảnh báo KPI (UC_BI_019) — cấu hình; evaluate on-read.</summary>
public class BiAlertThreshold : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    /// <summary>Revenue · Profit · Custom</summary>
    public string MetricKey { get; set; } = "Revenue";
    public Guid? KpiTargetId { get; set; }
    /// <summary>Gt · Gte · Lt · Lte</summary>
    public string Operator { get; set; } = "Lt";
    public decimal ThresholdValue { get; set; }
    /// <summary>Info · Warn · Critical</summary>
    public string Severity { get; set; } = "Warn";
    public string Status { get; set; } = "Active";
    public string? Note { get; set; }
}
