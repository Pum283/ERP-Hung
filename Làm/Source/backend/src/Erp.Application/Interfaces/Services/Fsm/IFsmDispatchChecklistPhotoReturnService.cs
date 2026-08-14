using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IFsmDispatchChecklistPhotoReturnService
{
    // UC_FSM_016: Phân công theo rule
    Task<FsmAutoDispatchRuleDto> CreateAutoDispatchRuleAsync(Guid tenantId, FsmCreateAutoDispatchRuleRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<FsmAutoDispatchRuleDto>> GetAutoDispatchRulesAsync(Guid tenantId, CancellationToken ct = default);

    // UC_FSM_021: Checklist công việc
    Task<FsmJobExecutionChecklistDto> AddChecklistStepAsync(Guid tenantId, FsmAddChecklistStepRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<FsmJobExecutionChecklistDto>> GetJobChecklistsAsync(Guid tenantId, Guid ticketId, CancellationToken ct = default);

    // UC_FSM_023: Chụp ảnh trước/sau
    Task<FsmJobPhotoAttachmentDto> UploadJobPhotoAsync(Guid tenantId, FsmUploadJobPhotoRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<FsmJobPhotoAttachmentDto>> GetJobPhotosAsync(Guid tenantId, Guid ticketId, CancellationToken ct = default);

    // UC_FSM_025: Hoàn linh kiện thừa
    Task<FsmSparePartReturnDto> CreateSparePartReturnAsync(Guid tenantId, FsmCreateSparePartReturnRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<FsmSparePartReturnDto>> GetSparePartReturnsAsync(Guid tenantId, CancellationToken ct = default);
}
