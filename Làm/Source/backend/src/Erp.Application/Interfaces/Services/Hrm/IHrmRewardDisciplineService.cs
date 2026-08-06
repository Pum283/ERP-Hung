using Erp.Application.DTOs.Hrm;

namespace Erp.Application.Interfaces.Services.Hrm;

public interface IHrmRewardDisciplineService
{
    Task<IReadOnlyList<RewardDisciplineDto>> ListAsync(Guid tenantId, string? kind, CancellationToken ct = default);
    Task<RewardDisciplineDto> CreateAsync(Guid tenantId, Guid userId, RewardDisciplineCreateRequest req, CancellationToken ct = default);
    Task<RewardDisciplineDto> AttachAsync(Guid tenantId, Guid userId, Guid id, RewardDisciplineAttachRequest req, CancellationToken ct = default);
    Task<RewardDisciplineDto> ApplyToPayrollAsync(Guid tenantId, Guid userId, Guid id, Guid? periodId, CancellationToken ct = default);
    Task<IReadOnlyList<RewardDisciplineReportRowDto>> ReportAsync(Guid tenantId, int? year, CancellationToken ct = default);
}

public interface IHrmOffboardingService
{
    Task<OffboardingSettingDto> GetSettingsAsync(Guid tenantId, CancellationToken ct = default);
    Task<OffboardingSettingDto> UpsertSettingsAsync(Guid tenantId, Guid userId, OffboardingSettingUpsertRequest req, CancellationToken ct = default);

    Task<IReadOnlyList<OffboardingCaseDto>> ListAsync(Guid tenantId, CancellationToken ct = default);
    Task<OffboardingCaseDto> CreateAsync(Guid tenantId, Guid userId, OffboardingCreateRequest req, CancellationToken ct = default);
    Task<OffboardingCaseDto> SubmitAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);
    Task<OffboardingCaseDto> ApproveAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);
    Task<OffboardingCaseDto> RejectAsync(Guid tenantId, Guid userId, Guid id, OffboardingRejectRequest req, CancellationToken ct = default);
    Task<OffboardingCaseDto> UpdateChecklistAsync(Guid tenantId, Guid userId, Guid id, OffboardingChecklistUpdateRequest req, CancellationToken ct = default);
    Task<OffboardingCaseDto> RevokeAccessAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);
    Task<OffboardingCaseDto> SettleAsync(Guid tenantId, Guid userId, Guid id, OffboardingSettleRequest req, CancellationToken ct = default);
    Task<OffboardingCaseDto> SaveInterviewAsync(Guid tenantId, Guid userId, Guid id, OffboardingInterviewRequest req, CancellationToken ct = default);
    Task<OffboardingCaseDto> CompleteAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<OffboardingReportRowDto>> ReportByReasonAsync(Guid tenantId, int? year, CancellationToken ct = default);
}
