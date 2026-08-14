using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pjm;

/// <summary>Báo cáo năng suất và hiệu suất sử dụng nguồn lực dự án (UC_PJM_041).</summary>
public class PjmResourceProductivityReport : TenantEntity
{
    public string PeriodLabel { get; set; } = "Tháng 08/2026";
    public int TotalEngineersCount { get; set; } = 18;
    public decimal TotalAllocatedHours { get; set; } = 2880;
    public decimal TotalBillableTimesheetHours { get; set; } = 2650;
    public double ResourceUtilizationRatePct { get; set; } = 92.0;
    public decimal AverageOutputPerEngineerVnd { get; set; } = 125000000;
    public DateTimeOffset ReportGeneratedAt { get; set; } = DateTimeOffset.UtcNow;
}
