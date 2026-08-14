using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IPjmHandoverChangeRequestService
{
    // UC_PJM_027: Checklist bàn giao
    Task<PjmHandoverChecklistItemDto> CreateHandoverChecklistAsync(Guid tenantId, PjmCreateHandoverChecklistRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PjmHandoverChecklistItemDto>> GetHandoverChecklistsAsync(Guid tenantId, Guid projectId, CancellationToken ct = default);

    // UC_PJM_028: Ghi nhận ảnh / biên bản
    Task<PjmSiteProtocolAttachmentDto> UploadProtocolAttachmentAsync(Guid tenantId, PjmUploadProtocolAttachmentRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PjmSiteProtocolAttachmentDto>> GetProtocolAttachmentsAsync(Guid tenantId, Guid projectId, CancellationToken ct = default);

    // UC_PJM_029: Phát sinh change request
    Task<PjmEngineeringChangeRequestDto> CreateEcrAsync(Guid tenantId, PjmCreateEcrRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<PjmEngineeringChangeRequestDto>> GetEcrsAsync(Guid tenantId, Guid projectId, CancellationToken ct = default);

    // UC_PJM_030: Duyệt change request
    Task<PjmChangeRequestApprovalDto> ApproveEcrAsync(Guid tenantId, PjmApproveEcrRequest req, CancellationToken ct = default);
}
