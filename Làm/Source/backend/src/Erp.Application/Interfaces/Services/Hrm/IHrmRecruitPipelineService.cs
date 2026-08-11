using Erp.Application.DTOs.Hrm;

namespace Erp.Application.Interfaces.Services.Hrm;

public interface IHrmRecruitPipelineService
{
    Task<IReadOnlyList<JobPostingDto>> ListPostingsAsync(Guid tenantId, CancellationToken ct = default);
    Task<JobPostingDto> CreatePostingAsync(Guid tenantId, Guid userId, JobPostingCreateRequest req, CancellationToken ct = default);
    Task ClosePostingAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<CandidateDto>> ListCandidatesAsync(Guid tenantId, Guid? jobPostingId, CancellationToken ct = default);
    Task<CandidateDto> CreateCandidateAsync(Guid tenantId, Guid userId, CandidateCreateRequest req, CancellationToken ct = default);
    Task<CandidateDto> UpdatePipelineAsync(Guid tenantId, Guid id, CandidatePipelineUpdateRequest req, CancellationToken ct = default);
    Task<CandidateDto> EvaluateAsync(Guid tenantId, Guid id, CandidateEvalRequest req, CancellationToken ct = default);
    Task<CandidateDto> AddCareNoteAsync(Guid tenantId, Guid id, CandidateCareNoteRequest req, CancellationToken ct = default);

    // ── UC_HRM_059 — Sơ loại ứng viên ──
    Task<CandidateDto> ScreenCandidateAsync(Guid tenantId, Guid id, CandidateScreenRequest req, CancellationToken ct = default);

    // ── UC_HRM_060 + UC_HRM_061 + UC_HRM_062 — Đánh giá & Ra quyết định tuyển dụng ──
    Task<CandidateDto> AssignEvalOrgUnitAsync(Guid tenantId, Guid id, CandidateAssignEvalOrgRequest req, CancellationToken ct = default);
    Task<CandidateDto> SubmitEvaluationAsync(Guid tenantId, Guid id, CandidateSubmitEvalRequest req, CancellationToken ct = default);
    Task<CandidateDto> DecideCandidateAsync(Guid tenantId, Guid id, CandidateDecideRequest req, CancellationToken ct = default);

    // ── UC_HRM_064 — Lịch sử chăm sóc ứng viên ──
    Task<IReadOnlyList<CareNoteItemDto>> GetCareNotesAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    // ── UC_HRM_065 — Báo cáo hiệu quả kênh tuyển ──
    Task<IReadOnlyList<RecruitChannelReportDto>> GetChannelReportAsync(Guid tenantId, CancellationToken ct = default);

    Task<IReadOnlyList<RecruitChannelStatDto>> ChannelStatsAsync(Guid tenantId, CancellationToken ct = default);
}

