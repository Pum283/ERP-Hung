using Erp.Domain.Base;

namespace Erp.Domain.Entities.Bi;

/// <summary>Mục tiêu KPI theo kỳ (UC_BI_018, 021).</summary>
public class BiKpiTarget : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string ModuleCode { get; set; } = "FIN";
    /// <summary>Revenue · Profit · Custom</summary>
    public string MetricKey { get; set; } = "Revenue";
    /// <summary>vd 2026-08 · 2026-Q3</summary>
    public string PeriodKey { get; set; } = "";
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    public decimal TargetValue { get; set; }
    /// <summary>Giá trị thực tế stub Cap-2 (chưa nối warehouse).</summary>
    public decimal ActualStubValue { get; set; }
    public string? Unit { get; set; }
    public string Status { get; set; } = "Active";
    public string? Note { get; set; }
}
