namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_055: Chặn tải video
// ────────────────────────────────────────────────────────────────────────────

public record LmsVideoProtectionDto(
    Guid Id,
    Guid LessonId,
    bool IsDownloadBlocked,
    bool WatermarkEnabled,
    string WatermarkText,
    int SignedUrlExpiryMinutes,
    string AllowedRoles
);

public record LmsVideoProtectionUpdateRequest(
    Guid LessonId,
    bool IsDownloadBlocked = true,
    bool WatermarkEnabled = true,
    string WatermarkText = "",
    int SignedUrlExpiryMinutes = 120,
    string AllowedRoles = "Instructor,Admin"
);

public record LmsVideoPlaybackUrlDto(
    Guid LessonId,
    string StreamUrl,
    string SignedToken,
    DateTimeOffset ExpiresAt,
    bool IsDownloadBlocked,
    bool WatermarkEnabled,
    string WatermarkText
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_056 & UC_LMS_057: Khảo sát hiểu bài & Khảo sát tuân thủ
// ────────────────────────────────────────────────────────────────────────────

public record LmsSurveyDto(
    Guid Id,
    string Title,
    string SurveyType, // Comprehension | Compliance | GeneralFeedback
    Guid? CourseId,
    bool IsMandatory,
    bool MustCompleteBeforeShift
);

public record LmsSurveyUpsertRequest(
    string Title,
    string SurveyType = "Comprehension",
    Guid? CourseId = null,
    bool IsMandatory = true,
    bool MustCompleteBeforeShift = false
);

public record LmsSurveySubmissionRequest(
    Guid SurveyId,
    string AnswersJson,
    decimal TargetPassingScore = 70m
);

public record LmsSurveyResultDto(
    Guid ResponseId,
    Guid SurveyId,
    Guid StudentUserId,
    decimal Score,
    bool IsPassed,
    DateTimeOffset SubmittedAt,
    string StatusMessage
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_059: Bắt buộc hoàn thành trước ca
// ────────────────────────────────────────────────────────────────────────────

public record LmsShiftTrainingGateDto(
    Guid Id,
    Guid EmployeeId,
    string ShiftId,
    DateTime ShiftDate,
    DateTimeOffset ShiftStartTime,
    Guid CourseId,
    bool IsMandatoryCompleted,
    bool IsWorkEntryBlocked,
    string GateStatus
);

public record LmsShiftGateCheckRequest(
    Guid EmployeeId,
    string ShiftId,
    DateTime ShiftDate,
    DateTimeOffset ShiftStartTime,
    Guid MandatoryCourseId
);

public record LmsShiftGateEvaluationResultDto(
    Guid EmployeeId,
    string ShiftId,
    bool IsMandatoryCompleted,
    bool IsWorkEntryBlocked,
    string GateStatus,
    string Message
);
