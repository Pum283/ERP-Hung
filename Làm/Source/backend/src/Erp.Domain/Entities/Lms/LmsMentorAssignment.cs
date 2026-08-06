using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Gán mentor cho học viên (UC_LMS_023).</summary>
public class LmsMentorAssignment : TenantEntity
{
    public Guid MenteeEmployeeId { get; set; }
    public Guid MentorEmployeeId { get; set; }
    public string? Note { get; set; }
    public bool IsActive { get; set; } = true;
}
