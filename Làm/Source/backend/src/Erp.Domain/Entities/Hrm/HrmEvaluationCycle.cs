using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Kỳ đánh giá hiệu suất / KPI (UC_HRM_178).</summary>
public class HrmEvaluationCycle : TenantEntity
{
    public string CycleName { get; set; } = string.Empty;
    public string PeriodKey { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public Guid? KpiTemplateId { get; set; }
    /// <summary>Draft | Active | Closed</summary>
    public string Status { get; set; } = "Draft";
}
