using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Lịch ca nhân viên theo ngày (UC_HRM_082–091).</summary>
public class ShiftAssignment : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public Guid WorkShiftId { get; set; }
    public DateOnly WorkDate { get; set; }
    /// <summary>Scheduled | Cancelled</summary>
    public string Status { get; set; } = "Scheduled";
    public string? Note { get; set; }
}
