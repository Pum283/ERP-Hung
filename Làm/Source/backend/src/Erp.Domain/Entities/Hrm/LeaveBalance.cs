using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

public class LeaveBalance : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public int Year { get; set; }
    public decimal Entitled { get; set; }
    public decimal Used { get; set; }
    public decimal Remaining { get; set; }
}
