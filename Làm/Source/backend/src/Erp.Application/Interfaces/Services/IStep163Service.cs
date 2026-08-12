using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IStep163Service
{
    // UC_LMS_048: Đồng bộ chứng chỉ sang HRM
    Task<LmsHrmCertificateSyncResultDto> SyncCertificateToHrmAsync(Guid tenantId, Guid certificateId, CancellationToken ct = default);

    // UC_LMS_052: Phản hồi bài tập
    Task<IReadOnlyList<LmsAssignmentFeedbackDto>> GetAssignmentFeedbacksAsync(Guid tenantId, Guid lessonId, CancellationToken ct = default);
    Task<LmsAssignmentFeedbackDto> CreateAssignmentFeedbackAsync(Guid tenantId, Guid instructorUserId, LmsAssignmentFeedbackUpsertRequest req, CancellationToken ct = default);

    // UC_LMS_053: Thống kê doanh thu theo khóa
    Task<IReadOnlyList<LmsCourseRevenueStatDto>> GetCourseRevenueStatsAsync(Guid tenantId, CancellationToken ct = default);

    // UC_LMS_054: Chống chia sẻ tài khoản
    Task<LmsAccountSharingGuardDto> ValidateAccountSessionAsync(Guid tenantId, Guid userId, LmsSessionValidationRequest req, CancellationToken ct = default);
}
