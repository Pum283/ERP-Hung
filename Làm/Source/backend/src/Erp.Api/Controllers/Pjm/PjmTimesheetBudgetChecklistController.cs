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
[Route("api/pjm/timesheet-budget-checklist")]
public sealed class PjmTimesheetBudgetChecklistController : ControllerBase
{
    private readonly IPjmTimesheetBudgetChecklistService _svc;

    public PjmTimesheetBudgetChecklistController(IPjmTimesheetBudgetChecklistService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // UC_PJM_020: Timesheet theo dự án
    [HttpPost("timesheets")]
    [AuthorizePermission("pjm.timesheet.write")]
    public async Task<ActionResult<ApiResponse<PjmProjectTimesheetEntryDto>>> CreateTimesheet([FromBody] PjmCreateTimesheetRequest req, CancellationToken ct)
        => Ok(ApiResponse<PjmProjectTimesheetEntryDto>.Ok(await _svc.CreateTimesheetEntryAsync(TenantId, req, ct)));

    [HttpGet("timesheets")]
    [AuthorizePermission("pjm.timesheet.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PjmProjectTimesheetEntryDto>>>> GetTimesheets([FromQuery] Guid? projectId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PjmProjectTimesheetEntryDto>>.Ok(await _svc.GetTimesheetEntriesAsync(TenantId, projectId ?? Guid.Empty, ct)));

    // UC_PJM_024: Cảnh báo vượt ngân sách
    [HttpGet("budget-warnings")]
    [AuthorizePermission("pjm.budget.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PjmBudgetOverrunWarningDto>>>> GetBudgetWarnings(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PjmBudgetOverrunWarningDto>>.Ok(await _svc.GetBudgetOverrunWarningsAsync(TenantId, ct)));

    // UC_PJM_025: Checklist khảo sát
    [HttpPost("survey-checklists")]
    [AuthorizePermission("pjm.survey.write")]
    public async Task<ActionResult<ApiResponse<PjmSurveyChecklistItemDto>>> CreateSurveyChecklist([FromBody] PjmCreateSurveyChecklistRequest req, CancellationToken ct)
        => Ok(ApiResponse<PjmSurveyChecklistItemDto>.Ok(await _svc.CreateSurveyChecklistAsync(TenantId, req, ct)));

    [HttpGet("survey-checklists")]
    [AuthorizePermission("pjm.survey.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PjmSurveyChecklistItemDto>>>> GetSurveyChecklists([FromQuery] Guid? projectId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PjmSurveyChecklistItemDto>>.Ok(await _svc.GetSurveyChecklistsAsync(TenantId, projectId ?? Guid.Empty, ct)));

    // UC_PJM_026: Checklist lắp đặt
    [HttpPost("installation-checklists")]
    [AuthorizePermission("pjm.install.write")]
    public async Task<ActionResult<ApiResponse<PjmInstallationChecklistItemDto>>> CreateInstallationChecklist([FromBody] PjmCreateInstallationChecklistRequest req, CancellationToken ct)
        => Ok(ApiResponse<PjmInstallationChecklistItemDto>.Ok(await _svc.CreateInstallationChecklistAsync(TenantId, req, ct)));

    [HttpGet("installation-checklists")]
    [AuthorizePermission("pjm.install.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PjmInstallationChecklistItemDto>>>> GetInstallationChecklists([FromQuery] Guid? projectId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PjmInstallationChecklistItemDto>>.Ok(await _svc.GetInstallationChecklistsAsync(TenantId, projectId ?? Guid.Empty, ct)));
}
