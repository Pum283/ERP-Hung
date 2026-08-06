using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Quyết định khen thưởng / kỷ luật (UC_HRM_139–143).</summary>
public class RewardDisciplineDecision : TenantEntity
{
    public Guid EmployeeId { get; set; }
    /// <summary>Reward | Discipline</summary>
    public string Kind { get; set; } = "Reward";
    public string Title { get; set; } = string.Empty;
    public DateOnly DecisionDate { get; set; }
    public string? Reason { get; set; }
    /// <summary>Số tiền ảnh hưởng lương (+ thưởng / − phạt).</summary>
    public decimal PayrollImpactAmount { get; set; }
    /// <summary>Bonus | Deduction | None</summary>
    public string PayrollImpactKind { get; set; } = "None";
    public string? DecisionStorageKey { get; set; }
    /// <summary>Draft | Issued | Applied | Cancelled</summary>
    public string Status { get; set; } = "Issued";
    public Guid? AppliedPayrollPeriodId { get; set; }
    public string? Note { get; set; }
}
