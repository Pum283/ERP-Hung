using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Quỹ phép theo loại nghỉ + loại NS (UC_HRM_130).</summary>
public class LeaveEntitlementRule : TenantEntity
{
    public Guid LeaveTypeId { get; set; }
    /// <summary>null = áp dụng mọi loại NS.</summary>
    public Guid? EmployeeTypeId { get; set; }
    public decimal DaysPerYear { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Note { get; set; }
}
