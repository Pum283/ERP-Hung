using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

public class LeaveRequest : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public decimal Days { get; set; }
    public string? Reason { get; set; }
    /// <summary>Draft | Pending | Approved | Rejected | Cancelled</summary>
    public string Status { get; set; } = "Draft";
    public Guid? WfInstanceId { get; set; }
    public Guid RequestedByUserId { get; set; }
}
