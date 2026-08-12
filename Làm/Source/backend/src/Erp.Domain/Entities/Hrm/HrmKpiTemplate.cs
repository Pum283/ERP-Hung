using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Mẫu đánh giá KPI / Năng lực (UC_HRM_177).</summary>
public class HrmKpiTemplate : TenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? TargetRole { get; set; }
    public string CriteriaDescription { get; set; } = string.Empty;
    public decimal MaxScore { get; set; } = 100m;
    public decimal WeightPercentage { get; set; } = 100m;
}
