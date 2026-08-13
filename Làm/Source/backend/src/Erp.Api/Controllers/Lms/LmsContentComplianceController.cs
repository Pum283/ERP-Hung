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
[Route("api/lms/content-compliance")]
public sealed class LmsContentComplianceController : ControllerBase
{
    private readonly ILmsContentComplianceService _svc;

    public LmsContentComplianceController(ILmsContentComplianceService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_055: Chặn tải video
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("lms/videos/{lessonId:guid}/protection")]
    [AuthorizePermission("lms.course.read")]
    public async Task<ActionResult<ApiResponse<LmsVideoProtectionDto>>> GetVideoProtectionConfig([FromRoute] Guid lessonId, CancellationToken ct)
        => Ok(ApiResponse<LmsVideoProtectionDto>.Ok(await _svc.GetVideoProtectionConfigAsync(TenantId, lessonId, ct)));

    [HttpPost("lms/videos/protection")]
    [AuthorizePermission("lms.course.write")]
    public async Task<ActionResult<ApiResponse<LmsVideoProtectionDto>>> UpdateVideoProtectionConfig([FromBody] LmsVideoProtectionUpdateRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsVideoProtectionDto>.Ok(await _svc.UpdateVideoProtectionConfigAsync(TenantId, req, ct)));

    [HttpPost("lms/videos/{lessonId:guid}/playback-url")]
    [AuthorizePermission("lms.course.read")]
    public async Task<ActionResult<ApiResponse<LmsVideoPlaybackUrlDto>>> GenerateProtectedPlaybackUrl([FromRoute] Guid lessonId, [FromQuery] string userRole = "Learner", CancellationToken ct = default)
        => Ok(ApiResponse<LmsVideoPlaybackUrlDto>.Ok(await _svc.GenerateProtectedPlaybackUrlAsync(TenantId, UserId, lessonId, userRole, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_056 & UC_LMS_057: Khảo sát hiểu bài & Khảo sát tuân thủ
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("lms/surveys")]
    [AuthorizePermission("lms.course.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsSurveyDto>>>> GetSurveys([FromQuery] string? surveyType, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsSurveyDto>>.Ok(await _svc.GetSurveysAsync(TenantId, surveyType, ct)));

    [HttpPost("lms/surveys")]
    [AuthorizePermission("lms.course.write")]
    public async Task<ActionResult<ApiResponse<LmsSurveyDto>>> CreateSurvey([FromBody] LmsSurveyUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsSurveyDto>.Ok(await _svc.CreateSurveyAsync(TenantId, req, ct)));

    [HttpPost("lms/surveys/submit")]
    [AuthorizePermission("lms.course.read")]
    public async Task<ActionResult<ApiResponse<LmsSurveyResultDto>>> SubmitSurveyResponse([FromBody] LmsSurveySubmissionRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsSurveyResultDto>.Ok(await _svc.SubmitSurveyResponseAsync(TenantId, UserId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_059: Bắt buộc hoàn thành trước ca
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("lms/shift-gates/evaluate")]
    [AuthorizePermission("lms.course.write")]
    public async Task<ActionResult<ApiResponse<LmsShiftGateEvaluationResultDto>>> EvaluateShiftTrainingGate([FromBody] LmsShiftGateCheckRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsShiftGateEvaluationResultDto>.Ok(await _svc.EvaluateShiftTrainingGateAsync(TenantId, req, ct)));
}
