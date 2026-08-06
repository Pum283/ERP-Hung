using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Người thân / liên hệ khẩn cấp nhân sự (UC_HRM_023).</summary>
public class EmployeeRelative : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public string FullName { get; set; } = "";
    /// <summary>Spouse · Child · Parent · Sibling · Other</summary>
    public string Relationship { get; set; } = "Spouse";
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public bool IsEmergencyContact { get; set; } = true;
    public bool IsTaxDependent { get; set; }
    public string? IdNumber { get; set; }
}
