using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Ghi danh học viên (UC_LMS_018).</summary>
public class LmsClassEnrollment : TenantEntity
{
    public Guid ClassId { get; set; }
    public Guid EmployeeId { get; set; }
    /// <summary>Enrolled | Completed | Dropped</summary>
    public string Status { get; set; } = "Enrolled";
    public DateTimeOffset EnrolledAt { get; set; } = DateTimeOffset.UtcNow;
}
