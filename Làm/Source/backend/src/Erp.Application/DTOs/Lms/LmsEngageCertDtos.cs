namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_038: Nhắc học tiếp
// ────────────────────────────────────────────────────────────────────────────

public record LmsStudyReminderDto(
    Guid Id,
    Guid UserId,
    Guid CourseId,
    string? CourseName,
    string Frequency,
    DateTimeOffset? LastRemindedAt,
    string Message,
    bool IsActive,
    DateTimeOffset CreatedAt
);

public record LmsStudyReminderUpsertRequest(
    Guid CourseId,
    string Frequency = "Daily", // Daily | Weekly | Custom
    string Message = "Bạn còn bài học chưa hoàn thành, hãy vào học tiếp nhé!"
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_039: Diễn đàn / bình luận
// ────────────────────────────────────────────────────────────────────────────

public record LmsForumTopicDto(
    Guid Id,
    Guid CourseId,
    string? CourseName,
    Guid AuthorId,
    string? AuthorName,
    string Title,
    string Content,
    int ReplyCount,
    bool IsPinned,
    DateTimeOffset CreatedAt
);

public record LmsForumTopicUpsertRequest(
    Guid CourseId,
    string Title,
    string Content,
    bool IsPinned = false
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_046: Mã xác thực chứng chỉ
// ────────────────────────────────────────────────────────────────────────────

public record LmsCertificateVerificationResultDto(
    Guid CertificateId,
    string Code,
    Guid CourseId,
    string? CourseName,
    Guid UserId,
    string? UserName,
    DateTimeOffset IssuedAt,
    string Status,
    bool IsValid,
    decimal? ScoreAtIssue
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_047: Thu hồi chứng chỉ
// ────────────────────────────────────────────────────────────────────────────

public record LmsCertificateRevocationDto(
    Guid Id,
    Guid CertificateId,
    string? CertificateCode,
    string RevocationReason,
    DateTimeOffset RevokedAt,
    Guid RevokedByUserId,
    DateTimeOffset CreatedAt
);

public record LmsRevokeCertificateRequest(
    Guid CertificateId,
    string RevocationReason
);
