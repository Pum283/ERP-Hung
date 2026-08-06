using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Lương thực tế / bậc / đơn giá NV (UC_HRM_153–156).</summary>
public class EmployeeSalary : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public Guid? SalaryGradeId { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal? HourlyRate { get; set; }
    public decimal? DailyRate { get; set; }
    /// <summary>Áp dụng khi Status NV khớp (Active/Probation…); null = mọi TT.</summary>
    public string? AppliesToStatus { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Note { get; set; }
}
