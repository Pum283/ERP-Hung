using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Rule phụ cấp theo ca / đặc thù (UC_HRM_158–159).</summary>
public class AllowanceRule : TenantEntity
{
    public Guid AllowanceTypeId { get; set; }
    /// <summary>null = mọi ca.</summary>
    public string? ShiftCode { get; set; }
    public decimal Amount { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Note { get; set; }
}
