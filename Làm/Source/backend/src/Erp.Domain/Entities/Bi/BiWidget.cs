using Erp.Domain.Base;

namespace Erp.Domain.Entities.Bi;

/// <summary>Widget dashboard (UC_BI_008).</summary>
public class BiWidget : TenantEntity
{
    public Guid DashboardId { get; set; }
    public string Code { get; set; } = "";
    public string Title { get; set; } = "";
    /// <summary>Kpi · Chart · Table</summary>
    public string WidgetType { get; set; } = "Kpi";
    /// <summary>Revenue · Profit · Custom</summary>
    public string MetricKey { get; set; } = "Revenue";
    public decimal StubValue { get; set; }
    public string? Unit { get; set; }
    public int SortOrder { get; set; }
    public string Status { get; set; } = "Active";
}
