namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_060: Báo cáo tỷ lệ xác nhận
// ────────────────────────────────────────────────────────────────────────────

public record LmsAcknowledgementReportDto(
    string Department,
    int TotalEmployees,
    int AcknowledgedCount,
    int PendingCount,
    decimal ComplianceRatePct
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_061: Gán lộ trình theo chức danh
// ────────────────────────────────────────────────────────────────────────────

public record LmsLearningPathItemDto(
    Guid Id,
    Guid LearningPathId,
    Guid CourseId,
    string CourseName,
    int SequenceOrder,
    bool IsMandatory
);

public record LmsLearningPathDto(
    Guid Id,
    string Title,
    string JobTitle,
    string Description,
    int TargetDaysToComplete,
    bool IsActive,
    IReadOnlyList<LmsLearningPathItemDto> Items
);

public record LmsLearningPathUpsertRequest(
    string Title,
    string JobTitle,
    string Description = "",
    int TargetDaysToComplete = 30,
    bool IsActive = true,
    List<Guid>? CourseIds = null
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_062: Tự gán khóa bắt buộc khi nhận việc
// ────────────────────────────────────────────────────────────────────────────

public record LmsAutoAssignOnHireResultDto(
    Guid UserId,
    string JobTitle,
    Guid AssignedPathId,
    IReadOnlyList<Guid> AssignedCourseIds,
    DateTimeOffset DueDate,
    string Message
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_063: Theo dõi hoàn thành lộ trình
// ────────────────────────────────────────────────────────────────────────────

public record LmsUserLearningPathProgressDto(
    Guid Id,
    Guid UserId,
    Guid LearningPathId,
    string PathTitle,
    string JobTitle,
    DateTimeOffset AssignedAt,
    DateTimeOffset DueDate,
    string Status,
    int CompletedCoursesCount,
    int TotalCoursesCount,
    decimal ProgressPct
);
