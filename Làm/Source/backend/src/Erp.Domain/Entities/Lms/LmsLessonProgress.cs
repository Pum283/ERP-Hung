using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Tiến độ bài học (UC_LMS_035–037).</summary>
public class LmsLessonProgress : TenantEntity
{
    public Guid EnrollmentId { get; set; }
    public Guid LessonId { get; set; }
    /// <summary>InProgress · Completed</summary>
    public string Status { get; set; } = "InProgress";
    public DateTimeOffset? CompletedAt { get; set; }
    public int? LastPositionSec { get; set; }
}