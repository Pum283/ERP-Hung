using Erp.Application.DTOs.Hrm;

namespace Erp.Application.Interfaces.Services.Hrm;

public interface IHrmOnboardingService
{
    Task<OnboardingSettingDto> GetSettingsAsync(Guid tenantId, CancellationToken ct = default);
    Task<OnboardingSettingDto> UpsertSettingsAsync(Guid tenantId, Guid userId, OnboardingSettingUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<OnboardingCaseDto>> ListCasesAsync(Guid tenantId, CancellationToken ct = default);
    Task<OnboardingCaseDto> HireFromCandidateAsync(Guid tenantId, Guid userId, HireFromCandidateRequest req, CancellationToken ct = default);
    Task<OnboardingCaseDto> AssignMentorAsync(Guid tenantId, Guid caseId, AssignMentorRequest req, CancellationToken ct = default);
    Task<OnboardingCaseDto> UpdateChecklistAsync(Guid tenantId, Guid caseId, OnboardingChecklistUpdateRequest req, CancellationToken ct = default);
    Task<OnboardingCaseDto> AddDocumentAsync(Guid tenantId, Guid caseId, OnboardingDocUploadRequest req, CancellationToken ct = default);
    Task<OnboardingCaseDto> EvaluateTrialAsync(Guid tenantId, Guid caseId, TrialEvalRequest req, CancellationToken ct = default);
    Task<OnboardingCaseDto> ConvertToOfficialAsync(Guid tenantId, Guid caseId, CancellationToken ct = default);
    Task<IReadOnlyList<TrialExpiringDto>> ListTrialExpiringAsync(Guid tenantId, int withinDays, CancellationToken ct = default);
}
