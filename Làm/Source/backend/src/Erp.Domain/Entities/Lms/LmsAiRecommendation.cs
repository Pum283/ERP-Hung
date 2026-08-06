using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Thực thể gợi ý khóa học & tóm tắt AI (UC_LMS_071, 072, 073, 074).</summary>
public class LmsAiRecommendation : TenantEntity
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public string RecommendationReason { get; set; } = string.Empty;
    public string LessonSummary { get; set; } = string.Empty;
    public string GeneratedQuizJson { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; } = 0.95;
}
