namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_071: Gợi ý khóa học tiếp theo (AI Recommendation)
// ────────────────────────────────────────────────────────────────────────────

public record LmsAiCourseRecommendationDto(
    Guid CourseId,
    string CourseCode,
    string CourseName,
    decimal MatchPercentage,
    string RecommendationReason,
    List<string> SkillTags
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_072: Tóm tắt bài học bằng AI
// ────────────────────────────────────────────────────────────────────────────

public record LmsGenerateLessonSummaryRequest(
    Guid? CourseId,
    string LessonTitle,
    string RawContentText
);

public record LmsAiLessonSummaryDto(
    string LessonTitle,
    string SummaryOverview,
    List<string> KeyTakeaways,
    List<string> SuggestedNextTopics
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_073: AI tạo quiz từ nội dung
// ────────────────────────────────────────────────────────────────────────────

public record LmsGenerateQuizRequest(
    Guid? CourseId,
    string TopicTitle,
    string SourceTextContent,
    int NumberOfQuestions = 3
);

public record LmsAiQuizQuestionItemDto(
    int QuestionNo,
    string QuestionText,
    List<string> Options,
    int CorrectOptionIndex,
    string Explanation
);

public record LmsAiGeneratedQuizDto(
    string TopicTitle,
    int TotalQuestions,
    List<LmsAiQuizQuestionItemDto> Questions
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_074: Trợ lý hỏi đáp
// ────────────────────────────────────────────────────────────────────────────

public record LmsAskAiAssistantRequest(
    Guid? LessonId,
    string QuestionText
);

public record LmsAiQnaResponseDto(
    Guid LogId,
    string QuestionText,
    string AnswerText,
    decimal ConfidenceScore,
    DateTimeOffset Timestamp
);
