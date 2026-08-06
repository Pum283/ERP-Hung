using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Dòng bảng lương NV trong kỳ.</summary>
public class PayrollLine : TenantEntity
{
    public Guid PayrollPeriodId { get; set; }
    public Guid EmployeeId { get; set; }
    public decimal WorkUnits { get; set; }
    public int OtMinutes { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal AttendancePay { get; set; }
    public decimal OtPay { get; set; }
    public decimal AllowanceTotal { get; set; }
    public decimal Bonus { get; set; }
    public decimal DeductionTotal { get; set; }
    public decimal InsuranceEmployee { get; set; }
    public decimal Tax { get; set; }
    public decimal GrossPay { get; set; }
    public decimal NetPay { get; set; }
    public bool IsConfirmed { get; set; }
    public string? Note { get; set; }
}
