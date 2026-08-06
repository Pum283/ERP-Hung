using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Điểm danh buổi học (UC_LMS_019).</summary>
public class LmsSessionAttendance : TenantEntity
{
    public Guid SessionId { get; set; }
    public Guid EnrollmentId { get; set; }
    public bool Present { get; set; }
    public string? Note { get; set; }
}
