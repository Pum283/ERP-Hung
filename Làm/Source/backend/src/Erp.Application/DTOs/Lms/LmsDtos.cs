namespace Erp.Application.DTOs.Lms;

public sealed record LmsTrainingClassDto(
    Guid Id,
    string Code,
    string Name,
    string CourseTitle,
    Guid? InstructorId,
    string? InstructorName,
    string? Location,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    string? SummaryNote,
    int SessionCount,
    int EnrollmentCount);

public sealed record LmsTrainingClassUpsertRequest(
    Guid? Id,
    string Code,
    string Name,
    string CourseTitle,
    Guid? InstructorId,
    string? InstructorName,
    string? Location,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Status);

public sealed record LmsClassSessionDto(
    Guid Id,
    Guid ClassId,
    DateOnly SessionDate,
    string Topic,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int SortOrder);

public sealed record LmsClassSessionCreateRequest(
    DateOnly SessionDate,
    string Topic,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int? SortOrder);

public sealed record LmsClassEnrollmentDto(
    Guid Id,
    Guid ClassId,
    Guid EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    string Status,
    DateTimeOffset EnrolledAt);

public sealed record LmsSessionAttendanceDto(
    Guid Id,
    Guid SessionId,
    Guid EnrollmentId,
    bool Present,
    string? Note);

public sealed record LmsClassDetailDto(
    LmsTrainingClassDto Class,
    IReadOnlyList<LmsClassSessionDto> Sessions,
    IReadOnlyList<LmsClassEnrollmentDto> Enrollments,
    IReadOnlyList<LmsSessionAttendanceDto> Attendance);

public sealed record LmsClassEnrollmentRequest(Guid EmployeeId);

public sealed record LmsClassCloseRequest(string? SummaryNote);

public sealed record LmsSessionAttendanceRequest(
    Guid EnrollmentId,
    bool Present,
    string? Note);

public sealed record LmsMentorAssignmentDto(
    Guid Id,
    Guid MenteeEmployeeId,
    string MenteeCode,
    string MenteeName,
    Guid MentorEmployeeId,
    string MentorCode,
    string MentorName,
    string? Note,
    bool IsActive);

public sealed record LmsMentorAssignRequest(
    Guid MenteeEmployeeId,
    Guid MentorEmployeeId,
    string? Note);

// ——— Catalog (UC_LMS_001–006, 009) ———

public sealed record LmsProgramDto(Guid Id, string Code, string Name, string? Description, string Status);

public sealed record LmsProgramUpsertRequest(
    Guid? Id,
    string Code,
    string Name,
    string? Description,
    string? Status);

public sealed record LmsCourseDto(
    Guid Id,
    Guid? ProgramId,
    string? ProgramName,
    string Code,
    string Name,
    string? Summary,
    string DeliveryMode,
    string Status,
    decimal Price,
    string Currency,
    string? CoverUrl,
    int ChapterCount,
    int LessonCount);

public sealed record LmsCourseUpsertRequest(
    Guid? Id,
    Guid? ProgramId,
    string Code,
    string Name,
    string? Summary,
    string DeliveryMode,
    string? Status,
    decimal Price,
    string? Currency,
    string? CoverUrl);

public sealed record LmsChapterDto(Guid Id, Guid CourseId, string Title, int SortOrder, int LessonCount);

public sealed record LmsChapterUpsertRequest(Guid? Id, string Title, int? SortOrder);

public sealed record LmsLessonDto(
    Guid Id,
    Guid ChapterId,
    string Title,
    string LessonType,
    string? ContentUrl,
    string? Body,
    int SortOrder,
    int? DurationSec);

public sealed record LmsLessonUpsertRequest(
    Guid? Id,
    string Title,
    string LessonType,
    string? ContentUrl,
    string? Body,
    int? SortOrder,
    int? DurationSec);

public sealed record LmsCourseDetailDto(
    LmsCourseDto Course,
    IReadOnlyList<LmsChapterDto> Chapters,
    IReadOnlyList<LmsLessonDto> Lessons);

public sealed record LmsPublishCourseRequest(string Status);

// ——— Online learner (UC_LMS_030–037) ———

public sealed record LmsCatalogCourseDto(
    Guid Id,
    string Code,
    string Name,
    string? Summary,
    string DeliveryMode,
    decimal Price,
    string Currency,
    string? CoverUrl,
    int LessonCount,
    string? EnrollmentStatus,
    decimal ProgressPct);

public sealed record LmsOnlineEnrollmentDto(
    Guid Id,
    Guid CourseId,
    Guid UserId,
    string Status,
    decimal PaidAmount,
    DateTimeOffset? PaidAt,
    Guid? LastLessonId,
    decimal ProgressPct);

public sealed record LmsEnrollRequest(string? VoucherCode);

public sealed record LmsLessonProgressDto(
    Guid LessonId,
    string Status,
    DateTimeOffset? CompletedAt,
    int? LastPositionSec);

public sealed record LmsLearnCourseDto(
    LmsCourseDto Course,
    IReadOnlyList<LmsChapterDto> Chapters,
    IReadOnlyList<LmsLessonDto> Lessons,
    LmsOnlineEnrollmentDto Enrollment,
    IReadOnlyList<LmsLessonProgressDto> Progress,
    Guid? ResumeLessonId);

public sealed record LmsCompleteLessonRequest(int? LastPositionSec);

// ——— Quiz / exam / certificate (UC_LMS_010, 012, 014, 040–045) ———

public sealed record LmsQuestionOptionDto(string Key, string Text);

public sealed record LmsQuestionDto(
    Guid Id,
    string Code,
    string Stem,
    string QuestionType,
    IReadOnlyList<LmsQuestionOptionDto> Options,
    IReadOnlyList<string> CorrectKeys,
    decimal Points,
    string? Tag,
    bool IsActive);

public sealed record LmsQuestionUpsertRequest(
    Guid? Id,
    string Code,
    string Stem,
    string QuestionType,
    IReadOnlyList<LmsQuestionOptionDto> Options,
    IReadOnlyList<string> CorrectKeys,
    decimal Points,
    string? Tag,
    bool? IsActive);

public sealed record LmsExamDto(
    Guid Id,
    string Code,
    string Name,
    string ExamType,
    Guid? CourseId,
    string? CourseName,
    Guid? ChapterId,
    string? ChapterTitle,
    decimal PassScore,
    int MaxAttempts,
    int? TimeLimitMin,
    string Status,
    int QuestionCount);

public sealed record LmsExamUpsertRequest(
    Guid? Id,
    string Code,
    string Name,
    string ExamType,
    Guid? CourseId,
    Guid? ChapterId,
    decimal PassScore,
    int MaxAttempts,
    int? TimeLimitMin,
    string? Status);

public sealed record LmsExamQuestionItemDto(
    Guid Id,
    Guid QuestionId,
    string QuestionCode,
    string Stem,
    string QuestionType,
    int SortOrder,
    decimal Points);

public sealed record LmsExamDetailDto(
    LmsExamDto Exam,
    IReadOnlyList<LmsExamQuestionItemDto> Questions);

public sealed record LmsExamAddQuestionRequest(Guid QuestionId, decimal? PointsOverride);

public sealed record LmsPublishExamRequest(string Status);

public sealed record LmsLearnerExamDto(
    Guid Id,
    string Code,
    string Name,
    string ExamType,
    Guid? ChapterId,
    decimal PassScore,
    int MaxAttempts,
    int AttemptsUsed,
    bool CanStart,
    bool? LastPassed,
    decimal? LastScore);

public sealed record LmsTakeQuestionDto(
    Guid QuestionId,
    string Stem,
    string QuestionType,
    IReadOnlyList<LmsQuestionOptionDto> Options,
    decimal Points,
    int SortOrder);

public sealed record LmsAttemptDto(
    Guid Id,
    Guid ExamId,
    int AttemptNo,
    string Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? SubmittedAt,
    decimal Score,
    decimal MaxScore,
    bool Passed,
    IReadOnlyList<LmsTakeQuestionDto>? Questions);

public sealed record LmsSubmitAttemptRequest(Dictionary<string, string> Answers);

public sealed record LmsAttemptResultDto(
    Guid Id,
    Guid ExamId,
    int AttemptNo,
    decimal Score,
    decimal MaxScore,
    bool Passed,
    decimal PassScore,
    IReadOnlyList<LmsAnswerReviewDto> Reviews,
    LmsCertificateDto? Certificate);

public sealed record LmsAnswerReviewDto(
    Guid QuestionId,
    string Stem,
    string? YourKey,
    IReadOnlyList<string> CorrectKeys,
    bool IsCorrect,
    decimal PointsEarned,
    decimal Points);

public sealed record LmsCertificateDto(
    Guid Id,
    Guid CourseId,
    string CourseName,
    Guid UserId,
    string Code,
    DateTimeOffset IssuedAt,
    string Status,
    decimal? ScoreAtIssue);
