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
[Route("api/pjm/handover-change-request")]
public sealed class PjmHandoverChangeRequestController : ControllerBase
{
    private readonly IPjmHandoverChangeRequestService _svc;

    public PjmHandoverChangeRequestController(IPjmHandoverChangeRequestService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_PJM_027: Checklist bàn giao
    [HttpPost("handover-checklists")]
    [AuthorizePermission("pjm.handover.write")]
    public async Task<ActionResult<ApiResponse<PjmHandoverChecklistItemDto>>> CreateHandoverChecklist([FromBody] PjmCreateHandoverChecklistRequest req, CancellationToken ct)
        => Ok(ApiResponse<PjmHandoverChecklistItemDto>.Ok(await _svc.CreateHandoverChecklistAsync(TenantId, req, ct)));

    [HttpGet("handover-checklists")]
    [AuthorizePermission("pjm.handover.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PjmHandoverChecklistItemDto>>>> GetHandoverChecklists([FromQuery] Guid? projectId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PjmHandoverChecklistItemDto>>.Ok(await _svc.GetHandoverChecklistsAsync(TenantId, projectId ?? Guid.Empty, ct)));

    // UC_PJM_028: Ghi nhận ảnh / biên bản
    [HttpPost("protocol-attachments")]
    [AuthorizePermission("pjm.handover.write")]
    public async Task<ActionResult<ApiResponse<PjmSiteProtocolAttachmentDto>>> UploadProtocolAttachment([FromBody] PjmUploadProtocolAttachmentRequest req, CancellationToken ct)
        => Ok(ApiResponse<PjmSiteProtocolAttachmentDto>.Ok(await _svc.UploadProtocolAttachmentAsync(TenantId, req, ct)));

    [HttpGet("protocol-attachments")]
    [AuthorizePermission("pjm.handover.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PjmSiteProtocolAttachmentDto>>>> GetProtocolAttachments([FromQuery] Guid? projectId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PjmSiteProtocolAttachmentDto>>.Ok(await _svc.GetProtocolAttachmentsAsync(TenantId, projectId ?? Guid.Empty, ct)));

    // UC_PJM_029: Phát sinh change request
    [HttpPost("ecrs")]
    [AuthorizePermission("pjm.change.write")]
    public async Task<ActionResult<ApiResponse<PjmEngineeringChangeRequestDto>>> CreateEcr([FromBody] PjmCreateEcrRequest req, CancellationToken ct)
        => Ok(ApiResponse<PjmEngineeringChangeRequestDto>.Ok(await _svc.CreateEcrAsync(TenantId, req, ct)));

    [HttpGet("ecrs")]
    [AuthorizePermission("pjm.change.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PjmEngineeringChangeRequestDto>>>> GetEcrs([FromQuery] Guid? projectId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PjmEngineeringChangeRequestDto>>.Ok(await _svc.GetEcrsAsync(TenantId, projectId ?? Guid.Empty, ct)));

    // UC_PJM_030: Duyệt change request
    [HttpPost("ecrs/approve")]
    [AuthorizePermission("pjm.change.write")]
    public async Task<ActionResult<ApiResponse<PjmChangeRequestApprovalDto>>> ApproveEcr([FromBody] PjmApproveEcrRequest req, CancellationToken ct)
        => Ok(ApiResponse<PjmChangeRequestApprovalDto>.Ok(await _svc.ApproveEcrAsync(TenantId, req, ct)));
}
