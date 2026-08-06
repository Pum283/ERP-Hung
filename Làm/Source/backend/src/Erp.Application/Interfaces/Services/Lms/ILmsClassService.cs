using Erp.Application.DTOs.Lms;

namespace Erp.Application.Interfaces.Services.Lms;

public interface ILmsClassService
{
    Task<IReadOnlyList<LmsTrainingClassDto>> ListClassesAsync(Guid tenantId, CancellationToken ct = default);
    Task<LmsTrainingClassDto> UpsertClassAsync(
        Guid tenantId, Guid userId, LmsTrainingClassUpsertRequest req, CancellationToken ct = default);
    Task<LmsClassDetailDto> GetClassDetailAsync(Guid tenantId, Guid classId, CancellationToken ct = default);

    Task<LmsClassSessionDto> AddSessionAsync(
        Guid tenantId, Guid userId, Guid classId, LmsClassSessionCreateRequest req, CancellationToken ct = default);

    Task<LmsClassEnrollmentDto> EnrollAsync(
        Guid tenantId, Guid userId, Guid classId, LmsClassEnrollmentRequest req, CancellationToken ct = default);

    Task<LmsTrainingClassDto> CloseClassAsync(
        Guid tenantId, Guid userId, Guid classId, LmsClassCloseRequest req, CancellationToken ct = default);

    Task<LmsSessionAttendanceDto> RecordAttendanceAsync(
        Guid tenantId, Guid userId, Guid sessionId, LmsSessionAttendanceRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<LmsMentorAssignmentDto>> ListMentorsAsync(Guid tenantId, CancellationToken ct = default);
    Task<LmsMentorAssignmentDto> AssignMentorAsync(
        Guid tenantId, Guid userId, LmsMentorAssignRequest req, CancellationToken ct = default);
}
