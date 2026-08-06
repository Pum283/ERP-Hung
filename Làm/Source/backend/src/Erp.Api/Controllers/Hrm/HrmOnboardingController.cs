using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Hrm;
using Erp.Application.Interfaces.Services.Hrm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Hrm;

[ApiController]
[Authorize]
[Route("api/hrm/onboarding")]
public sealed class HrmOnboardingController : ControllerBase
{
    private readonly IHrmOnboardingService _svc;

    public HrmOnboardingController(IHrmOnboardingService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("settings")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<OnboardingSettingDto>>> Settings(CancellationToken ct)
        => Ok(ApiResponse<OnboardingSettingDto>.Ok(await _svc.GetSettingsAsync(TenantId, ct)));

    [HttpPut("settings")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<OnboardingSettingDto>>> UpsertSettings(
        [FromBody] OnboardingSettingUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<OnboardingSettingDto>.Ok(await _svc.UpsertSettingsAsync(TenantId, UserId, req, ct)));

    [HttpGet("cases")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<OnboardingCaseDto>>>> Cases(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<OnboardingCaseDto>>.Ok(await _svc.ListCasesAsync(TenantId, ct)));

    [HttpGet("trial-expiring")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TrialExpiringDto>>>> TrialExpiring(
        [FromQuery] int days = 14, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<TrialExpiringDto>>.Ok(await _svc.ListTrialExpiringAsync(TenantId, days, ct)));

    [HttpPost("hire-from-candidate")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<OnboardingCaseDto>>> Hire(
        [FromBody] HireFromCandidateRequest req, CancellationToken ct)
        => Ok(ApiResponse<OnboardingCaseDto>.Ok(await _svc.HireFromCandidateAsync(TenantId, UserId, req, ct)));

    [HttpPost("cases/{id:guid}/mentor")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<OnboardingCaseDto>>> Mentor(
        Guid id, [FromBody] AssignMentorRequest req, CancellationToken ct)
        => Ok(ApiResponse<OnboardingCaseDto>.Ok(await _svc.AssignMentorAsync(TenantId, id, req, ct)));

    [HttpPut("cases/{id:guid}/checklist")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<OnboardingCaseDto>>> Checklist(
        Guid id, [FromBody] OnboardingChecklistUpdateRequest req, CancellationToken ct)
        => Ok(ApiResponse<OnboardingCaseDto>.Ok(await _svc.UpdateChecklistAsync(TenantId, id, req, ct)));

    [HttpPost("cases/{id:guid}/documents")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<OnboardingCaseDto>>> AddDoc(
        Guid id, [FromBody] OnboardingDocUploadRequest req, CancellationToken ct)
        => Ok(ApiResponse<OnboardingCaseDto>.Ok(await _svc.AddDocumentAsync(TenantId, id, req, ct)));

    [HttpPost("cases/{id:guid}/trial-eval")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<OnboardingCaseDto>>> TrialEval(
        Guid id, [FromBody] TrialEvalRequest req, CancellationToken ct)
        => Ok(ApiResponse<OnboardingCaseDto>.Ok(await _svc.EvaluateTrialAsync(TenantId, id, req, ct)));

    [HttpPost("cases/{id:guid}/convert")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<OnboardingCaseDto>>> Convert(Guid id, CancellationToken ct)
        => Ok(ApiResponse<OnboardingCaseDto>.Ok(await _svc.ConvertToOfficialAsync(TenantId, id, ct)));
}
