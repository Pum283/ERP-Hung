using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface ILmsAiAssistService
{
    // UC_LMS_071: Gợi ý khóa học tiếp theo
    Task<IReadOnlyList<LmsAiCourseRecommendationDto>> GetCourseRecommendationsAsync(Guid tenantId, Guid userId, CancellationToken ct = default);

    // UC_LMS_072: Tóm tắt bài học bằng AI
    Task<LmsAiLessonSummaryDto> GenerateLessonSummaryAsync(Guid tenantId, LmsGenerateLessonSummaryRequest req, CancellationToken ct = default);

    // UC_LMS_073: AI tạo quiz từ nội dung
    Task<LmsAiGeneratedQuizDto> GenerateQuizFromContentAsync(Guid tenantId, LmsGenerateQuizRequest req, CancellationToken ct = default);

    // UC_LMS_074: Trợ lý hỏi đáp
    Task<LmsAiQnaResponseDto> AskLearningAssistantAsync(Guid tenantId, Guid userId, LmsAskAiAssistantRequest req, CancellationToken ct = default);
}
