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
[Route("api/step161")]
public sealed class Step161Controller : ControllerBase
{
    private readonly IStep161Service _svc;

    public Step161Controller(IStep161Service svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_015: Thời gian làm bài & chống gian lận
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("lms/exams/anti-cheat-violation")]
    [AuthorizePermission("lms.exam.write")]
    public async Task<ActionResult<ApiResponse<LmsExamAntiCheatSessionDto>>> ProcessAntiCheatViolation([FromBody] LmsAntiCheatViolationRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsExamAntiCheatSessionDto>.Ok(await _svc.ProcessAntiCheatViolationAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_024: Checklist kèm cặp
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("lms/mentoring-checklists")]
    [AuthorizePermission("lms.mentoring.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsMentoringChecklistDto>>>> GetMentoringChecklists([FromQuery] Guid assignmentId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsMentoringChecklistDto>>.Ok(await _svc.GetMentoringChecklistsAsync(TenantId, assignmentId, ct)));

    [HttpPost("lms/mentoring-checklists")]
    [AuthorizePermission("lms.mentoring.write")]
    public async Task<ActionResult<ApiResponse<LmsMentoringChecklistDto>>> CreateMentoringChecklistTask([FromBody] LmsMentoringChecklistUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsMentoringChecklistDto>.Ok(await _svc.CreateMentoringChecklistTaskAsync(TenantId, req, ct)));

    [HttpPut("lms/mentoring-checklists/{taskId:guid}/toggle")]
    [AuthorizePermission("lms.mentoring.write")]
    public async Task<ActionResult<ApiResponse<LmsMentoringChecklistDto>>> ToggleChecklistTask(Guid taskId, [FromQuery] bool isCompleted, [FromQuery] string? note, CancellationToken ct)
        => Ok(ApiResponse<LmsMentoringChecklistDto>.Ok(await _svc.ToggleChecklistTaskAsync(TenantId, taskId, isCompleted, note, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_026: Đánh giá mentor / học viên
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("lms/mentoring-evaluations")]
    [AuthorizePermission("lms.mentoring.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsMentoringEvaluationDto>>>> GetMentoringEvaluations([FromQuery] Guid assignmentId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsMentoringEvaluationDto>>.Ok(await _svc.GetMentoringEvaluationsAsync(TenantId, assignmentId, ct)));

    [HttpPost("lms/mentoring-evaluations")]
    [AuthorizePermission("lms.mentoring.write")]
    public async Task<ActionResult<ApiResponse<LmsMentoringEvaluationDto>>> CreateMentoringEvaluation([FromBody] LmsMentoringEvaluationUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsMentoringEvaluationDto>.Ok(await _svc.CreateMentoringEvaluationAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_027: Báo cáo hiệu quả mentoring
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("lms/mentoring-effectiveness-report")]
    [AuthorizePermission("lms.mentoring.read")]
    public async Task<ActionResult<ApiResponse<LmsMentoringEffectivenessReportDto>>> GetMentoringEffectivenessReport(CancellationToken ct)
        => Ok(ApiResponse<LmsMentoringEffectivenessReportDto>.Ok(await _svc.GetMentoringEffectivenessReportAsync(TenantId, ct)));
}
