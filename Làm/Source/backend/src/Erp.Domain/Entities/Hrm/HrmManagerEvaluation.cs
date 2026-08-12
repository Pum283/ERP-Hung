using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Đánh giá nhân viên của quản lý (UC_HRM_179).</summary>
public class HrmManagerEvaluation : TenantEntity
{
    public Guid EvaluationCycleId { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid EvaluatorId { get; set; }
    public decimal KpiScore { get; set; }
    public decimal CompetencyScore { get; set; }
    /// <summary>A | B | C | D</summary>
    public string FinalGrade { get; set; } = "B";
    public string? ManagerComments { get; set; }
    /// <summary>Pending | Completed</summary>
    public string Status { get; set; } = "Pending";
}
