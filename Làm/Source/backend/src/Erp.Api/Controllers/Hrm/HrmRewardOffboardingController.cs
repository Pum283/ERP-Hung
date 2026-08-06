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
[Route("api/hrm/rewards")]
public sealed class HrmRewardDisciplineController : ControllerBase
{
    private readonly IHrmRewardDisciplineService _svc;

    public HrmRewardDisciplineController(IHrmRewardDisciplineService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RewardDisciplineDto>>>> List(
        [FromQuery] string? kind, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<RewardDisciplineDto>>.Ok(await _svc.ListAsync(TenantId, kind, ct)));

    [HttpPost]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<RewardDisciplineDto>>> Create(
        [FromBody] RewardDisciplineCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<RewardDisciplineDto>.Ok(await _svc.CreateAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/attach")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<RewardDisciplineDto>>> Attach(
        Guid id, [FromBody] RewardDisciplineAttachRequest req, CancellationToken ct)
        => Ok(ApiResponse<RewardDisciplineDto>.Ok(await _svc.AttachAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/apply-payroll")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<RewardDisciplineDto>>> ApplyPayroll(
        Guid id, [FromQuery] Guid? periodId, CancellationToken ct)
        => Ok(ApiResponse<RewardDisciplineDto>.Ok(await _svc.ApplyToPayrollAsync(TenantId, UserId, id, periodId, ct)));

    [HttpGet("report")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RewardDisciplineReportRowDto>>>> Report(
        [FromQuery] int? year, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<RewardDisciplineReportRowDto>>.Ok(await _svc.ReportAsync(TenantId, year, ct)));
}

[ApiController]
[Authorize]
[Route("api/hrm/offboarding")]
public sealed class HrmOffboardingController : ControllerBase
{
    private readonly IHrmOffboardingService _svc;

    public HrmOffboardingController(IHrmOffboardingService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("settings")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<OffboardingSettingDto>>> GetSettings(CancellationToken ct)
        => Ok(ApiResponse<OffboardingSettingDto>.Ok(await _svc.GetSettingsAsync(TenantId, ct)));

    [HttpPut("settings")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<OffboardingSettingDto>>> UpsertSettings(
        [FromBody] OffboardingSettingUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<OffboardingSettingDto>.Ok(await _svc.UpsertSettingsAsync(TenantId, UserId, req, ct)));

    [HttpGet]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<OffboardingCaseDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<OffboardingCaseDto>>.Ok(await _svc.ListAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<OffboardingCaseDto>>> Create(
        [FromBody] OffboardingCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<OffboardingCaseDto>.Ok(await _svc.CreateAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/submit")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<OffboardingCaseDto>>> Submit(Guid id, CancellationToken ct)
        => Ok(ApiResponse<OffboardingCaseDto>.Ok(await _svc.SubmitAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/approve")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<OffboardingCaseDto>>> Approve(Guid id, CancellationToken ct)
        => Ok(ApiResponse<OffboardingCaseDto>.Ok(await _svc.ApproveAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/reject")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<OffboardingCaseDto>>> Reject(
        Guid id, [FromBody] OffboardingRejectRequest req, CancellationToken ct)
        => Ok(ApiResponse<OffboardingCaseDto>.Ok(await _svc.RejectAsync(TenantId, UserId, id, req, ct)));

    [HttpPut("{id:guid}/checklist")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<OffboardingCaseDto>>> Checklist(
        Guid id, [FromBody] OffboardingChecklistUpdateRequest req, CancellationToken ct)
        => Ok(ApiResponse<OffboardingCaseDto>.Ok(await _svc.UpdateChecklistAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/revoke-access")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<OffboardingCaseDto>>> Revoke(Guid id, CancellationToken ct)
        => Ok(ApiResponse<OffboardingCaseDto>.Ok(await _svc.RevokeAccessAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/settle")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<OffboardingCaseDto>>> Settle(
        Guid id, [FromBody] OffboardingSettleRequest req, CancellationToken ct)
        => Ok(ApiResponse<OffboardingCaseDto>.Ok(await _svc.SettleAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/interview")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<OffboardingCaseDto>>> Interview(
        Guid id, [FromBody] OffboardingInterviewRequest req, CancellationToken ct)
        => Ok(ApiResponse<OffboardingCaseDto>.Ok(await _svc.SaveInterviewAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/complete")]
    [AuthorizePermission("hrm.employee.manage")]
    public async Task<ActionResult<ApiResponse<OffboardingCaseDto>>> Complete(Guid id, CancellationToken ct)
        => Ok(ApiResponse<OffboardingCaseDto>.Ok(await _svc.CompleteAsync(TenantId, UserId, id, ct)));

    [HttpGet("report")]
    [AuthorizePermission("hrm.employee.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<OffboardingReportRowDto>>>> Report(
        [FromQuery] int? year, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<OffboardingReportRowDto>>.Ok(await _svc.ReportByReasonAsync(TenantId, year, ct)));
}
