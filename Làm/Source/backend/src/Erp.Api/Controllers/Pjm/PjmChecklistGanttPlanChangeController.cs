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
[Route("api/pjm/checklist-gantt-plan-change")]
public sealed class PjmChecklistGanttPlanChangeController : ControllerBase
{
    private readonly IPjmChecklistGanttPlanChangeService _svc;

    public PjmChecklistGanttPlanChangeController(IPjmChecklistGanttPlanChangeService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_PJM_003: Mẫu checklist nghiệm thu
    [HttpPost("acceptance-templates")]
    [AuthorizePermission("pjm.template.write")]
    public async Task<ActionResult<ApiResponse<PjmAcceptanceChecklistTemplateDto>>> CreateAcceptanceTemplate([FromBody] PjmCreateAcceptanceTemplateRequest req, CancellationToken ct)
        => Ok(ApiResponse<PjmAcceptanceChecklistTemplateDto>.Ok(await _svc.CreateAcceptanceTemplateAsync(TenantId, req, ct)));

    [HttpGet("acceptance-templates")]
    [AuthorizePermission("pjm.template.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PjmAcceptanceChecklistTemplateDto>>>> GetAcceptanceTemplates([FromQuery] string? projectCategory, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PjmAcceptanceChecklistTemplateDto>>.Ok(await _svc.GetAcceptanceTemplatesAsync(TenantId, projectCategory ?? "", ct)));

    // UC_PJM_016: Gantt / timeline tiến độ
    [HttpPost("milestones")]
    [AuthorizePermission("pjm.milestone.write")]
    public async Task<ActionResult<ApiResponse<PjmGanttTimelineMilestoneDto>>> CreateMilestone([FromBody] PjmCreateMilestoneRequest req, CancellationToken ct)
        => Ok(ApiResponse<PjmGanttTimelineMilestoneDto>.Ok(await _svc.CreateMilestoneAsync(TenantId, req, ct)));

    [HttpGet("milestones")]
    [AuthorizePermission("pjm.milestone.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PjmGanttTimelineMilestoneDto>>>> GetMilestones([FromQuery] Guid? projectId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PjmGanttTimelineMilestoneDto>>.Ok(await _svc.GetMilestonesAsync(TenantId, projectId ?? Guid.Empty, ct)));

    // UC_PJM_018: Nhật ký thay đổi kế hoạch
    [HttpPost("plan-changes")]
    [AuthorizePermission("pjm.change.write")]
    public async Task<ActionResult<ApiResponse<PjmPlanChangeAuditLogDto>>> LogPlanChange([FromBody] PjmLogPlanChangeRequest req, CancellationToken ct)
        => Ok(ApiResponse<PjmPlanChangeAuditLogDto>.Ok(await _svc.LogPlanChangeAsync(TenantId, req, ct)));

    [HttpGet("plan-changes")]
    [AuthorizePermission("pjm.change.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PjmPlanChangeAuditLogDto>>>> GetPlanChangeLogs([FromQuery] Guid? projectId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PjmPlanChangeAuditLogDto>>.Ok(await _svc.GetPlanChangeLogsAsync(TenantId, projectId ?? Guid.Empty, ct)));
}
