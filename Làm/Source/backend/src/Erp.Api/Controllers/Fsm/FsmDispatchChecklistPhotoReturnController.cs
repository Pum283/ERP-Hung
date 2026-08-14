using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/fsm/dispatch-checklist-photo-return")]
public sealed class FsmDispatchChecklistPhotoReturnController : ControllerBase
{
    private readonly IFsmDispatchChecklistPhotoReturnService _svc;

    public FsmDispatchChecklistPhotoReturnController(IFsmDispatchChecklistPhotoReturnService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_FSM_016: Phân công theo rule
    [HttpPost("dispatch-rules")]
    [AuthorizePermission("fsm.dispatch.write")]
    public async Task<ActionResult<ApiResponse<FsmAutoDispatchRuleDto>>> CreateAutoDispatchRule([FromBody] FsmCreateAutoDispatchRuleRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmAutoDispatchRuleDto>.Ok(await _svc.CreateAutoDispatchRuleAsync(TenantId, req, ct)));

    [HttpGet("dispatch-rules")]
    [AuthorizePermission("fsm.dispatch.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FsmAutoDispatchRuleDto>>>> GetAutoDispatchRules(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FsmAutoDispatchRuleDto>>.Ok(await _svc.GetAutoDispatchRulesAsync(TenantId, ct)));

    // UC_FSM_021: Checklist công việc
    [HttpPost("checklist-steps")]
    [AuthorizePermission("fsm.job.write")]
    public async Task<ActionResult<ApiResponse<FsmJobExecutionChecklistDto>>> AddChecklistStep([FromBody] FsmAddChecklistStepRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmJobExecutionChecklistDto>.Ok(await _svc.AddChecklistStepAsync(TenantId, req, ct)));

    [HttpGet("checklist-steps/{ticketId:guid}")]
    [AuthorizePermission("fsm.job.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FsmJobExecutionChecklistDto>>>> GetJobChecklists(Guid ticketId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FsmJobExecutionChecklistDto>>.Ok(await _svc.GetJobChecklistsAsync(TenantId, ticketId, ct)));

    // UC_FSM_023: Chụp ảnh trước/sau
    [HttpPost("job-photos")]
    [AuthorizePermission("fsm.job.write")]
    public async Task<ActionResult<ApiResponse<FsmJobPhotoAttachmentDto>>> UploadJobPhoto([FromBody] FsmUploadJobPhotoRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmJobPhotoAttachmentDto>.Ok(await _svc.UploadJobPhotoAsync(TenantId, req, ct)));

    [HttpGet("job-photos/{ticketId:guid}")]
    [AuthorizePermission("fsm.job.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FsmJobPhotoAttachmentDto>>>> GetJobPhotos(Guid ticketId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FsmJobPhotoAttachmentDto>>.Ok(await _svc.GetJobPhotosAsync(TenantId, ticketId, ct)));

    // UC_FSM_025: Hoàn linh kiện thừa
    [HttpPost("spare-part-returns")]
    [AuthorizePermission("fsm.part.write")]
    public async Task<ActionResult<ApiResponse<FsmSparePartReturnDto>>> CreateSparePartReturn([FromBody] FsmCreateSparePartReturnRequest req, CancellationToken ct)
        => Ok(ApiResponse<FsmSparePartReturnDto>.Ok(await _svc.CreateSparePartReturnAsync(TenantId, req, ct)));

    [HttpGet("spare-part-returns")]
    [AuthorizePermission("fsm.part.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FsmSparePartReturnDto>>>> GetSparePartReturns(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<FsmSparePartReturnDto>>.Ok(await _svc.GetSparePartReturnsAsync(TenantId, ct)));
}
