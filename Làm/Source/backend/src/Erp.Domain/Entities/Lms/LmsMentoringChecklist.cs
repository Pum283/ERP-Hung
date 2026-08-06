using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Checklist kèm cặp mentoring (UC_LMS_024).</summary>
public class LmsMentoringChecklist : TenantEntity
{
    public Guid MentorAssignmentId { get; set; }
    public string TaskName { get; set; } = "";
    public bool IsCompleted { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? MentorNote { get; set; }
}
