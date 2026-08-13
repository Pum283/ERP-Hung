using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface ILmsPathTrackingService
{
    // UC_LMS_060: Báo cáo tỷ lệ xác nhận
    Task<IReadOnlyList<LmsAcknowledgementReportDto>> GetAcknowledgementReportAsync(Guid tenantId, string? department = null, CancellationToken ct = default);

    // UC_LMS_061: Gán lộ trình theo chức danh
    Task<IReadOnlyList<LmsLearningPathDto>> GetLearningPathsAsync(Guid tenantId, string? jobTitle = null, CancellationToken ct = default);
    Task<LmsLearningPathDto> CreateLearningPathAsync(Guid tenantId, LmsLearningPathUpsertRequest req, CancellationToken ct = default);

    // UC_LMS_062: Tự gán khóa bắt buộc khi nhận việc
    Task<LmsAutoAssignOnHireResultDto> AutoAssignOnHireAsync(Guid tenantId, Guid userId, string jobTitle, CancellationToken ct = default);

    // UC_LMS_063: Theo dõi hoàn thành lộ trình
    Task<IReadOnlyList<LmsUserLearningPathProgressDto>> GetUserLearningPathProgressAsync(Guid tenantId, Guid? userId = null, string? jobTitle = null, CancellationToken ct = default);
}
