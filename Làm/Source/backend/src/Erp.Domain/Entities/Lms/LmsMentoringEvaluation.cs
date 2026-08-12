using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Đánh giá mentor / học viên (UC_LMS_026).</summary>
public class LmsMentoringEvaluation : TenantEntity
{
    public Guid MentorAssignmentId { get; set; }
    public Guid EvaluatorId { get; set; }
    public Guid EvaluateeId { get; set; }
    /// <summary>MentorToMentee | MenteeToMentor</summary>
    public string EvaluationType { get; set; } = "MentorToMentee";
    public int Rating { get; set; } = 5;
    public string Feedback { get; set; } = string.Empty;
}
