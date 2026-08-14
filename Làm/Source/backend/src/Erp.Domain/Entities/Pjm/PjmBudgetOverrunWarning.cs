using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pjm;

/// <summary>Cảnh báo vượt hạn mức ngân sách dự án (UC_PJM_024).</summary>
public class PjmBudgetOverrunWarning : TenantEntity
{
    public Guid ProjectId { get; set; }
    public string ProjectCode { get; set; } = "PRJ-2026-088";
    public string ProjectName { get; set; } = "Hệ thống trạm biến áp và tủ phân phối tổng";
    public decimal ApprovedBudgetVnd { get; set; } = 500000000;
    public decimal ActualCommittedCostVnd { get; set; } = 530000000;
    public decimal OverrunAmountVnd { get; set; } = 30000000;
    public double OverrunPercent { get; set; } = 6.0;
    public string WarningSeverity { get; set; } = "Warning"; // Warning | Critical
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;
}
