using Erp.Application.DTOs.Lms;

namespace Erp.Application.Interfaces.Services.Lms;

public interface ILmsExamService
{
    Task<IReadOnlyList<LmsQuestionDto>> ListQuestionsAsync(Guid tenantId, CancellationToken ct = default);
    Task<LmsQuestionDto> UpsertQuestionAsync(
        Guid tenantId, Guid userId, LmsQuestionUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<LmsExamDto>> ListExamsAsync(Guid tenantId, CancellationToken ct = default);
    Task<LmsExamDto> UpsertExamAsync(
        Guid tenantId, Guid userId, LmsExamUpsertRequest req, CancellationToken ct = default);
    Task<LmsExamDetailDto> GetExamDetailAsync(Guid tenantId, Guid examId, CancellationToken ct = default);
    Task<LmsExamDto> SetExamStatusAsync(
        Guid tenantId, Guid userId, Guid examId, LmsPublishExamRequest req, CancellationToken ct = default);
    Task<LmsExamQuestionItemDto> AddQuestionToExamAsync(
        Guid tenantId, Guid userId, Guid examId, LmsExamAddQuestionRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<LmsLearnerExamDto>> ListLearnerExamsAsync(
        Guid tenantId, Guid userId, Guid courseId, CancellationToken ct = default);
    Task<LmsAttemptDto> StartAttemptAsync(
        Guid tenantId, Guid userId, Guid examId, CancellationToken ct = default);
    Task<LmsAttemptResultDto> SubmitAttemptAsync(
        Guid tenantId, Guid userId, Guid attemptId, LmsSubmitAttemptRequest req, CancellationToken ct = default);
    Task<LmsAttemptResultDto> GetAttemptResultAsync(
        Guid tenantId, Guid userId, Guid attemptId, CancellationToken ct = default);

    Task<IReadOnlyList<LmsCertificateDto>> ListMyCertificatesAsync(
        Guid tenantId, Guid userId, CancellationToken ct = default);
}
