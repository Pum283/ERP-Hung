using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Gợi ý khóa học tiếp theo + AI trợ lý học tập (UC_LMS_071–074).</summary>
public class LmsAiRecommendation : TenantEntity
{
    public Guid UserId { get; set; }
    public Guid RecommendedCourseId { get; set; }
    public Guid CourseId { get; set; }
    public decimal MatchPercentage { get; set; }
    public string RecommendationReason { get; set; } = "";
    public string LessonSummary { get; set; } = "";
    public string GeneratedQuizJson { get; set; } = "[]";
    public double ConfidenceScore { get; set; } = 0.95;
    public DateTimeOffset RecommendedAt { get; set; } = DateTimeOffset.UtcNow;
}
