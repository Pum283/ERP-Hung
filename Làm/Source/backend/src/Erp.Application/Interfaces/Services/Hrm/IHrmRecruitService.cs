using Erp.Application.DTOs.Hrm;

namespace Erp.Application.Interfaces.Services.Hrm;

public interface IHrmRecruitService
{
    Task<IReadOnlyList<RecruitmentRequestDto>> ListAsync(Guid tenantId, Guid userId, CancellationToken ct = default);
    Task<RecruitmentRequestDto> CreateAsync(Guid tenantId, Guid userId, RecruitmentRequestCreateRequest req, CancellationToken ct = default);
    Task<RecruitmentRequestDto> SubmitAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);
    Task<RecruitmentRequestDto> CancelOrCloseAsync(Guid tenantId, Guid userId, Guid id, CancellationToken ct = default);

    // ── UC_HRM_051 / 052 — Phê duyệt & Lịch sử duyệt đề xuất ──
    Task<RecruitmentRequestDto> ApproveOrRejectAsync(Guid tenantId, Guid userId, Guid id, ApproveRecruitmentRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<RecruitmentApprovalStepDto>> GetApprovalHistoryAsync(Guid tenantId, Guid id, CancellationToken ct = default);
}

