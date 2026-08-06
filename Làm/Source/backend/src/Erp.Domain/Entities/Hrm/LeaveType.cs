using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

public class LeaveType : TenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsPaid { get; set; } = true;
    public decimal DefaultDaysPerYear { get; set; }
    public bool IsActive { get; set; } = true;
}
