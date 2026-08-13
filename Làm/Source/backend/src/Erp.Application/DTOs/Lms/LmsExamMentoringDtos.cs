namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_015: Thời gian làm bài & chống gian lận
// ────────────────────────────────────────────────────────────────────────────

public record LmsExamAntiCheatSessionDto(
    Guid AttemptId,
    Guid ExamId,
    Guid UserId,
    DateTimeOffset StartedAt,
    int TimeLimitMin,
    int RemainingSeconds,
    int FocusLossCount,
    bool IsAutoSubmitted,
    string? AutoSubmitReason
);

public record LmsAntiCheatViolationRequest(
    Guid AttemptId,
    string EventType = "FocusLoss", // FocusLoss | TabSwitch | TimeExpired
    string Action = "RecordViolation" // RecordViolation | ForceSubmit
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_024: Checklist kèm cặp
// ────────────────────────────────────────────────────────────────────────────

public record LmsMentoringChecklistDto(
    Guid Id,
    Guid MentorAssignmentId,
    string TaskName,
    bool IsCompleted,
    DateTimeOffset? CompletedAt,
    string? MentorNote,
    DateTimeOffset CreatedAt
);

public record LmsMentoringChecklistUpsertRequest(
    Guid MentorAssignmentId,
    string TaskName,
    bool IsCompleted = false,
    string? MentorNote = null
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_026: Đánh giá mentor / học viên
// ────────────────────────────────────────────────────────────────────────────

public record LmsMentoringEvaluationDto(
    Guid Id,
    Guid MentorAssignmentId,
    Guid EvaluatorId,
    Guid EvaluateeId,
    string EvaluationType,
    int Rating,
    string Feedback,
    DateTimeOffset CreatedAt
);

public record LmsMentoringEvaluationUpsertRequest(
    Guid MentorAssignmentId,
    Guid EvaluatorId,
    Guid EvaluateeId,
    string EvaluationType = "MentorToMentee", // MentorToMentee | MenteeToMentor
    int Rating = 5,
    string Feedback = ""
);

// ────────────────────────────────────────────────────────────────────────────
// UC_LMS_027: Báo cáo hiệu quả mentoring
// ────────────────────────────────────────────────────────────────────────────

public record LmsMentoringEffectivenessReportDto(
    int TotalAssignments,
    int ActiveAssignments,
    int CompletedChecklistTasks,
    int TotalChecklistTasks,
    decimal OverallCompletionPercentage,
    decimal AverageMentorRating,
    decimal AverageMenteeRating
);
