using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Thưởng / phụ cấp phát sinh / khấu trừ / tạm ứng (UC_HRM_166–167).</summary>
public class PayrollAdjustment : TenantEntity
{
    public Guid PayrollPeriodId { get; set; }
    public Guid EmployeeId { get; set; }
    /// <summary>Bonus | Allowance | Deduction | Advance</summary>
    public string Kind { get; set; } = "Bonus";
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Note { get; set; }
}
