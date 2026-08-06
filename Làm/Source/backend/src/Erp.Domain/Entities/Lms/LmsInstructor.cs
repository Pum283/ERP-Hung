using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Hồ sơ giảng viên (UC_LMS_049–050).</summary>
public class LmsInstructor : TenantEntity
{
    public string Code { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public Guid? EmployeeId { get; set; }
    public Guid? UserId { get; set; }
    public string? Title { get; set; }
    public string? Specialty { get; set; }
    public string? Bio { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    /// <summary>Active · Inactive</summary>
    public string Status { get; set; } = "Active";
    /// <summary>UC_050 — đã gán role LMS_INSTRUCTOR cho UserId.</summary>
    public bool RoleGranted { get; set; }
}
