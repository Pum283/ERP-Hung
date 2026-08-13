using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Hội thoại hỏi đáp trợ lý học tập AI (UC_LMS_074).</summary>
public class LmsAiQnaLog : TenantEntity
{
    public Guid UserId { get; set; }
    public Guid? LessonId { get; set; }
    public string Question { get; set; } = "";
    public string Answer { get; set; } = "";
    public decimal ConfidenceScore { get; set; } = 0.95m;
    public DateTimeOffset AskedAt { get; set; } = DateTimeOffset.UtcNow;
}
