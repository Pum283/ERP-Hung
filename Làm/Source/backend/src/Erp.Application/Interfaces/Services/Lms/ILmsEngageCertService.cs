using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface ILmsEngageCertService
{
    // UC_LMS_038: Nhắc học tiếp
    Task<IReadOnlyList<LmsStudyReminderDto>> GetStudyRemindersAsync(Guid tenantId, Guid userId, CancellationToken ct = default);
    Task<LmsStudyReminderDto> CreateStudyReminderAsync(Guid tenantId, Guid userId, LmsStudyReminderUpsertRequest req, CancellationToken ct = default);

    // UC_LMS_039: Diễn đàn / bình luận
    Task<IReadOnlyList<LmsForumTopicDto>> GetForumTopicsAsync(Guid tenantId, Guid courseId, CancellationToken ct = default);
    Task<LmsForumTopicDto> CreateForumTopicAsync(Guid tenantId, Guid authorId, LmsForumTopicUpsertRequest req, CancellationToken ct = default);

    // UC_LMS_046: Mã xác thực chứng chỉ
    Task<LmsCertificateVerificationResultDto> VerifyCertificateAsync(Guid tenantId, string code, CancellationToken ct = default);

    // UC_LMS_047: Thu hồi chứng chỉ
    Task<LmsCertificateRevocationDto> RevokeCertificateAsync(Guid tenantId, Guid revokedByUserId, LmsRevokeCertificateRequest req, CancellationToken ct = default);
}
