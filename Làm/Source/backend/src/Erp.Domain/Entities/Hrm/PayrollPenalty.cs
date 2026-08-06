using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Bảng phạt & phạt vào kỳ lương (UC_HRM_124, 125).</summary>
public class PayrollPenalty : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public Guid? PayrollPeriodId { get; set; }
    public string Reason { get; set; } = "";
    /// <summary>LateArrival · EarlyLeave · RegulationBreach · SafetyViolation · Other</summary>
    public string PenaltyType { get; set; } = "LateArrival";
    public decimal Amount { get; set; }
    public DateTimeOffset ViolationDate { get; set; } = DateTimeOffset.UtcNow;
    /// <summary>Pending · Applied · Cancelled</summary>
    public string Status { get; set; } = "Pending";
    public string? ApprovedByNote { get; set; }
}
