using Erp.Application.DTOs.Lms;

namespace Erp.Application.Interfaces.Services.Lms;

public interface ILmsCourseService
{
    Task<IReadOnlyList<LmsProgramDto>> ListProgramsAsync(Guid tenantId, CancellationToken ct = default);
    Task<LmsProgramDto> UpsertProgramAsync(
        Guid tenantId, Guid userId, LmsProgramUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<LmsCourseDto>> ListCoursesAsync(Guid tenantId, CancellationToken ct = default);
    Task<LmsCourseDto> UpsertCourseAsync(
        Guid tenantId, Guid userId, LmsCourseUpsertRequest req, CancellationToken ct = default);
    Task<LmsCourseDetailDto> GetCourseDetailAsync(Guid tenantId, Guid courseId, CancellationToken ct = default);
    Task<LmsCourseDto> SetPublishStatusAsync(
        Guid tenantId, Guid userId, Guid courseId, LmsPublishCourseRequest req, CancellationToken ct = default);

    Task<LmsChapterDto> UpsertChapterAsync(
        Guid tenantId, Guid userId, Guid courseId, LmsChapterUpsertRequest req, CancellationToken ct = default);
    Task<LmsLessonDto> UpsertLessonAsync(
        Guid tenantId, Guid userId, Guid chapterId, LmsLessonUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<LmsCatalogCourseDto>> ListCatalogAsync(
        Guid tenantId, Guid userId, CancellationToken ct = default);
    Task<LmsOnlineEnrollmentDto> EnrollAsync(
        Guid tenantId, Guid userId, Guid courseId, LmsEnrollRequest req, CancellationToken ct = default);
    Task<LmsLearnCourseDto> GetLearnAsync(
        Guid tenantId, Guid userId, Guid courseId, CancellationToken ct = default);
    Task<LmsLessonProgressDto> CompleteLessonAsync(
        Guid tenantId, Guid userId, Guid courseId, Guid lessonId, LmsCompleteLessonRequest req,
        CancellationToken ct = default);
}
