namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_048: Đồng bộ chứng chỉ sang HRM
// ────────────────────────────────────────────────────────────────────────────

public record LmsHrmCertificateSyncResultDto(
    Guid CertificateId,
    string CertificateCode,
    Guid EmployeeId,
    string SkillName,
    bool IsSynced,
    DateTimeOffset SyncedAt,
    string Message
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_052: Phản hồi bài tập
// ────────────────────────────────────────────────────────────────────────────

public record LmsAssignmentFeedbackDto(
    Guid Id,
    Guid LessonId,
    Guid StudentUserId,
    Guid InstructorUserId,
    string SubmissionUrl,
    decimal Score,
    string FeedbackComment,
    string Status,
    DateTimeOffset CreatedAt
);

public record LmsAssignmentFeedbackUpsertRequest(
    Guid LessonId,
    Guid StudentUserId,
    string SubmissionUrl,
    decimal Score = 100m,
    string FeedbackComment = "",
    string Status = "Graded" // Submitted | Graded | RevisionRequired
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_053: Thống kê doanh thu theo khóa
// ────────────────────────────────────────────────────────────────────────────

public record LmsCourseRevenueStatDto(
    Guid CourseId,
    string CourseName,
    decimal Price,
    int TotalEnrollments,
    int PaidEnrollments,
    decimal GrossRevenue,
    string Currency
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_054: Chống chia sẻ tài khoản
// ────────────────────────────────────────────────────────────────────────────

public record LmsAccountSharingGuardDto(
    Guid UserId,
    string DeviceId,
    string IpAddress,
    int ActiveSessionsCount,
    bool IsSharingDetected,
    string ActionTaken,
    string Reason
);

public record LmsSessionValidationRequest(
    string DeviceId,
    string IpAddress
);
