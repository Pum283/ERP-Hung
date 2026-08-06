using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Lượt làm bài + điểm tự động (UC_LMS_040–043).</summary>
public class LmsExamAttempt : TenantEntity
{
    public Guid ExamId { get; set; }
    public Guid UserId { get; set; }
    public Guid? EnrollmentId { get; set; }
    public int AttemptNo { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    /// <summary>InProgress · Submitted</summary>
    public string Status { get; set; } = "InProgress";
    /// <summary>JSON { "questionId": "A", ... }</summary>
    public string AnswersJson { get; set; } = "{}";
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public bool Passed { get; set; }
}
