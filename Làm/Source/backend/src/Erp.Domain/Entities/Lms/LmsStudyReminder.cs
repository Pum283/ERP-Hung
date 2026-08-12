using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Nhắc học tiếp khóa học LMS (UC_LMS_038).</summary>
public class LmsStudyReminder : TenantEntity
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public string Frequency { get; set; } = "Daily"; // Daily | Weekly | Custom
    public DateTimeOffset? LastRemindedAt { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
