using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface ILmsContentComplianceService
{
    // UC_LMS_055: Chặn tải video
    Task<LmsVideoProtectionDto> GetVideoProtectionConfigAsync(Guid tenantId, Guid lessonId, CancellationToken ct = default);
    Task<LmsVideoProtectionDto> UpdateVideoProtectionConfigAsync(Guid tenantId, LmsVideoProtectionUpdateRequest req, CancellationToken ct = default);
    Task<LmsVideoPlaybackUrlDto> GenerateProtectedPlaybackUrlAsync(Guid tenantId, Guid userId, Guid lessonId, string userRole = "Learner", CancellationToken ct = default);

    // UC_LMS_056 & UC_LMS_057: Khảo sát hiểu bài & Khảo sát tuân thủ
    Task<IReadOnlyList<LmsSurveyDto>> GetSurveysAsync(Guid tenantId, string? surveyType = null, CancellationToken ct = default);
    Task<LmsSurveyDto> CreateSurveyAsync(Guid tenantId, LmsSurveyUpsertRequest req, CancellationToken ct = default);
    Task<LmsSurveyResultDto> SubmitSurveyResponseAsync(Guid tenantId, Guid userId, LmsSurveySubmissionRequest req, CancellationToken ct = default);

    // UC_LMS_059: Bắt buộc hoàn thành trước ca
    Task<LmsShiftGateEvaluationResultDto> EvaluateShiftTrainingGateAsync(Guid tenantId, LmsShiftGateCheckRequest req, CancellationToken ct = default);
}
