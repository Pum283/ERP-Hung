namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_HRM_181: Tổng hợp kết quả đánh giá
// ────────────────────────────────────────────────────────────────────────────

public record HrmGradeDistributionDto(
    string Grade,
    int Count,
    decimal Percentage
);

public record HrmEvaluationSummaryReportDto(
    Guid EvaluationCycleId,
    string CycleName,
    int TotalEvaluatedCount,
    decimal AverageKpiScore,
    decimal AverageCompetencyScore,
    IReadOnlyList<HrmGradeDistributionDto> GradeDistributions
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_007: Gắn tag kỹ năng / vị trí
// ────────────────────────────────────────────────────────────────────────────

public record LmsCourseSkillTagDto(
    Guid Id,
    Guid CourseId,
    string? CourseTitle,
    string TagName,
    string TagType,
    Guid? RelatedRefId,
    DateTimeOffset CreatedAt
);

public record LmsCourseSkillTagUpsertRequest(
    Guid CourseId,
    string TagName,
    string TagType = "Skill",
    Guid? RelatedRefId = null
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_008: Phiên bản nội dung khóa học
// ────────────────────────────────────────────────────────────────────────────

public record LmsCourseVersionDto(
    Guid Id,
    Guid CourseId,
    string? CourseTitle,
    string VersionNumber,
    string Changelog,
    bool IsPublished,
    DateTimeOffset PublishedAt,
    DateTimeOffset CreatedAt
);

public record LmsCourseVersionUpsertRequest(
    Guid CourseId,
    string VersionNumber = "1.0",
    string Changelog = "",
    bool IsPublished = true
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_013: Tạo đề thi random
// ────────────────────────────────────────────────────────────────────────────

public record LmsRandomExamRequest(
    Guid CourseId,
    string ExamTitle,
    int TotalQuestions = 10,
    decimal PassingScore = 70m,
    int DurationMinutes = 45
);

public record LmsRandomExamResult(
    Guid ExamId,
    string ExamTitle,
    Guid CourseId,
    int SelectedQuestionCount,
    decimal PassingScore,
    int DurationMinutes,
    IReadOnlyList<Guid> GeneratedQuestionIds
);
