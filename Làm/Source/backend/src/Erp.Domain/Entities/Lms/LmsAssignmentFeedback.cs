using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Phản hồi bài tập khóa học (UC_LMS_052).</summary>
public class LmsAssignmentFeedback : TenantEntity
{
    public Guid LessonId { get; set; }
    public Guid StudentUserId { get; set; }
    public Guid InstructorUserId { get; set; }
    public string SubmissionUrl { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public string FeedbackComment { get; set; } = string.Empty;
    public string Status { get; set; } = "Graded"; // Submitted | Graded | RevisionRequired
}
