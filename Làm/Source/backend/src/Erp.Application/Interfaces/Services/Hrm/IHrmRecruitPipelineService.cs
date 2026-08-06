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
    Task<IReadOnlyList<RecruitChannelStatDto>> ChannelStatsAsync(Guid tenantId, CancellationToken ct = default);
}
