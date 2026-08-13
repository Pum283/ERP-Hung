using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Lộ trình đào tạo cá nhân gán cho học viên (UC_LMS_062, UC_LMS_063).</summary>
public class LmsUserLearningPath : TenantEntity
{
    public Guid UserId { get; set; }
    public Guid LearningPathId { get; set; }
    public string JobTitle { get; set; } = "";
    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset DueDate { get; set; } = DateTimeOffset.UtcNow.AddDays(30);
    public string Status { get; set; } = "InProgress"; // InProgress | Completed | Overdue
    public int CompletedCoursesCount { get; set; }
    public int TotalCoursesCount { get; set; }
    public decimal ProgressPct { get; set; }
}
