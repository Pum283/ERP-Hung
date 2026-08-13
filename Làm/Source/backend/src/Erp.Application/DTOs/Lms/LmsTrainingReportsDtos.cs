namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_064: Cảnh báo quá hạn đào tạo
// ────────────────────────────────────────────────────────────────────────────

public record LmsOverdueTrainingAlertDto(
    Guid Id,
    Guid UserId,
    string UserName,
    Guid CourseId,
    string CourseName,
    DateTimeOffset DueDate,
    int OverdueDays,
    DateTimeOffset AlertSentAt,
    string AlertStatus
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_067: Báo cáo điểm thi / tỷ lệ đạt
// ────────────────────────────────────────────────────────────────────────────

public record LmsExamAnalyticsReportDto(
    Guid ExamId,
    string ExamTitle,
    int TotalAttempts,
    int PassedAttempts,
    int FailedAttempts,
    decimal PassRatePct,
    decimal AverageScore,
    decimal HighestScore,
    decimal LowestScore
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_068: Báo cáo học viên bỏ dở
// ────────────────────────────────────────────────────────────────────────────

public record LmsDropoutAnalyticsReportDto(
    Guid CourseId,
    string CourseName,
    int TotalEnrolled,
    int ActiveLearners,
    int DropoutLearners,
    decimal DropoutRatePct,
    string CommonDropoutStage
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_069: Báo cáo hiệu quả khóa
// ────────────────────────────────────────────────────────────────────────────

public record LmsCourseEngagementReportDto(
    Guid CourseId,
    string CourseName,
    int TotalEnrolled,
    int TotalCompleted,
    decimal CompletionRatePct,
    decimal AverageRating,
    int TotalFeedbackComments,
    decimal AverageStudyHours
);
