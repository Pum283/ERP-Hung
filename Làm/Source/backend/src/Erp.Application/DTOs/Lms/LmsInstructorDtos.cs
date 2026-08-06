namespace Erp.Application.DTOs.Lms;

public sealed record LmsInstructorDto(
    Guid Id, string Code, string DisplayName,
    Guid? EmployeeId, string? EmployeeCode, string? EmployeeName,
    Guid? UserId, string? Title, string? Specialty, string? Bio,
    string? Email, string? Phone, string Status, bool RoleGranted,
    int ClassCount);

public sealed record LmsInstructorUpsertRequest(
    Guid? Id, string Code, string DisplayName,
    Guid? EmployeeId, Guid? UserId,
    string? Title, string? Specialty, string? Bio,
    string? Email, string? Phone, string? Status,
    bool GrantInstructorRole = false);

public sealed record LmsLearnerRowDto(
    string Source, Guid? ClassId, string? ClassCode, string? ClassName,
    Guid? CourseId, string? CourseCode, string? CourseName,
    Guid? EmployeeId, Guid? UserId, string LearnerCode, string LearnerName,
    string? OrgUnitName, string Status, DateTimeOffset EnrolledAt,
    decimal ProgressPercent, int? PresentSessions, int? TotalSessions);

public sealed record LmsDashboardDto(
    int CourseCount, int PublishedCourseCount,
    int OpenClassCount, int ClosedClassCount,
    int OfflineEnrollmentCount, int OfflineCompletedCount,
    int OnlineEnrollmentCount, int OnlineCompletedCount,
    int ActiveCertificateCount, int InstructorCount,
    decimal AvgOnlineProgressPercent, decimal ExamPassRatePercent);

public sealed record LmsCompletionByOrgRowDto(
    Guid? OrgUnitId, string OrgUnitCode, string OrgUnitName,
    int OfflineTotal, int OfflineCompleted,
    int OnlineTotal, int OnlineCompleted,
    decimal CompletionRatePercent);
