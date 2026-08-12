using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IStep160Service
{
    // UC_HRM_181: Tổng hợp kết quả đánh giá
    Task<HrmEvaluationSummaryReportDto> GetEvaluationSummaryReportAsync(Guid tenantId, Guid cycleId, CancellationToken ct = default);

    // UC_LMS_007: Gắn tag kỹ năng / vị trí
    Task<IReadOnlyList<LmsCourseSkillTagDto>> GetCourseSkillTagsAsync(Guid tenantId, Guid? courseId = null, CancellationToken ct = default);
    Task<LmsCourseSkillTagDto> CreateCourseSkillTagAsync(Guid tenantId, LmsCourseSkillTagUpsertRequest req, CancellationToken ct = default);
    Task DeleteCourseSkillTagAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    // UC_LMS_008: Phiên bản nội dung khóa học
    Task<IReadOnlyList<LmsCourseVersionDto>> GetCourseVersionsAsync(Guid tenantId, Guid courseId, CancellationToken ct = default);
    Task<LmsCourseVersionDto> CreateCourseVersionAsync(Guid tenantId, LmsCourseVersionUpsertRequest req, CancellationToken ct = default);

    // UC_LMS_013: Tạo đề thi random
    Task<LmsRandomExamResult> GenerateRandomExamAsync(Guid tenantId, LmsRandomExamRequest req, CancellationToken ct = default);
}
