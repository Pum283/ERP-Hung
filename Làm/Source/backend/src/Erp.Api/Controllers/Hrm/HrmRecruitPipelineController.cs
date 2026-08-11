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
[Route("api/hrm")]
public sealed class HrmRecruitPipelineController : ControllerBase
{
    private readonly IHrmRecruitPipelineService _svc;

    public HrmRecruitPipelineController(IHrmRecruitPipelineService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // ── UC_HRM_054 + UC_HRM_055 — Tin tuyển & kênh ──

    [HttpGet("job-postings")]
    [AuthorizePermission("hrm.recruit.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<JobPostingDto>>>> Postings(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<JobPostingDto>>.Ok(await _svc.ListPostingsAsync(TenantId, ct)));

    [HttpPost("job-postings")]
    [AuthorizePermission("hrm.recruit.manage")]
    public async Task<ActionResult<ApiResponse<JobPostingDto>>> CreatePosting(
        [FromBody] JobPostingCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<JobPostingDto>.Ok(await _svc.CreatePostingAsync(TenantId, UserId, req, ct)));

    [HttpPost("job-postings/{id:guid}/close")]
    [AuthorizePermission("hrm.recruit.manage")]
    public async Task<ActionResult<ApiResponse<object>>> ClosePosting(Guid id, CancellationToken ct)
    {
        await _svc.ClosePostingAsync(TenantId, id, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    // ── UC_HRM_056 + UC_HRM_057 — Nhập hồ sơ ứng viên & CV ──

    [HttpGet("candidates")]
    [AuthorizePermission("hrm.recruit.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CandidateDto>>>> Candidates(
        [FromQuery] Guid? jobPostingId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CandidateDto>>.Ok(
            await _svc.ListCandidatesAsync(TenantId, jobPostingId, ct)));

    [HttpPost("candidates")]
    [AuthorizePermission("hrm.recruit.manage")]
    public async Task<ActionResult<ApiResponse<CandidateDto>>> CreateCandidate(
        [FromBody] CandidateCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<CandidateDto>.Ok(await _svc.CreateCandidateAsync(TenantId, UserId, req, ct)));

    [HttpPost("candidates/{id:guid}/pipeline")]
    [AuthorizePermission("hrm.recruit.manage")]
    public async Task<ActionResult<ApiResponse<CandidateDto>>> Pipeline(
        Guid id, [FromBody] CandidatePipelineUpdateRequest req, CancellationToken ct)
        => Ok(ApiResponse<CandidateDto>.Ok(await _svc.UpdatePipelineAsync(TenantId, id, req, ct)));

    [HttpPost("candidates/{id:guid}/evaluate")]
    [AuthorizePermission("hrm.recruit.manage")]
    public async Task<ActionResult<ApiResponse<CandidateDto>>> Evaluate(
        Guid id, [FromBody] CandidateEvalRequest req, CancellationToken ct)
        => Ok(ApiResponse<CandidateDto>.Ok(await _svc.EvaluateAsync(TenantId, id, req, ct)));

    [HttpPost("candidates/{id:guid}/care-notes")]
    [AuthorizePermission("hrm.recruit.manage")]
    public async Task<ActionResult<ApiResponse<CandidateDto>>> CareNote(
        Guid id, [FromBody] CandidateCareNoteRequest req, CancellationToken ct)
        => Ok(ApiResponse<CandidateDto>.Ok(await _svc.AddCareNoteAsync(TenantId, id, req, ct)));

    // ── UC_HRM_059 — Sơ loại ứng viên ──

    [HttpPost("candidates/{id:guid}/screen")]
    [AuthorizePermission("hrm.recruit.manage")]
    public async Task<ActionResult<ApiResponse<CandidateDto>>> ScreenCandidate(
        Guid id, [FromBody] CandidateScreenRequest req, CancellationToken ct)
        => Ok(ApiResponse<CandidateDto>.Ok(await _svc.ScreenCandidateAsync(TenantId, id, req, ct)));

    // ── UC_HRM_060 — Chuyển ứng viên cho đơn vị đánh giá ──

    [HttpPost("candidates/{id:guid}/assign-eval-org")]
    [AuthorizePermission("hrm.recruit.manage")]
    public async Task<ActionResult<ApiResponse<CandidateDto>>> AssignEvalOrgUnit(
        Guid id, [FromBody] CandidateAssignEvalOrgRequest req, CancellationToken ct)
        => Ok(ApiResponse<CandidateDto>.Ok(await _svc.AssignEvalOrgUnitAsync(TenantId, id, req, ct)));

    // ── UC_HRM_061 — Form đánh giá ứng viên chi tiết ──

    [HttpPost("candidates/{id:guid}/submit-evaluation")]
    [AuthorizePermission("hrm.recruit.manage")]
    public async Task<ActionResult<ApiResponse<CandidateDto>>> SubmitEvaluation(
        Guid id, [FromBody] CandidateSubmitEvalRequest req, CancellationToken ct)
        => Ok(ApiResponse<CandidateDto>.Ok(await _svc.SubmitEvaluationAsync(TenantId, id, req, ct)));

    // ── UC_HRM_062 — Ra quyết định tuyển dụng (Accept / Reject) ──

    [HttpPost("candidates/{id:guid}/decide")]
    [AuthorizePermission("hrm.recruit.manage")]
    public async Task<ActionResult<ApiResponse<CandidateDto>>> DecideCandidate(
        Guid id, [FromBody] CandidateDecideRequest req, CancellationToken ct)
        => Ok(ApiResponse<CandidateDto>.Ok(await _svc.DecideCandidateAsync(TenantId, id, req, ct)));

    // ── UC_HRM_064 — Lịch sử chăm sóc ứng viên ──

    [HttpGet("candidates/{id:guid}/care-notes")]
    [AuthorizePermission("hrm.recruit.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CareNoteItemDto>>>> CareNotesHistory(Guid id, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<CareNoteItemDto>>.Ok(await _svc.GetCareNotesAsync(TenantId, id, ct)));

    // ── UC_HRM_055 + UC_HRM_065 — Thống kê & báo cáo kênh tuyển dụng ──

    [HttpGet("recruit/channel-stats")]
    [AuthorizePermission("hrm.recruit.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RecruitChannelStatDto>>>> ChannelStats(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<RecruitChannelStatDto>>.Ok(await _svc.ChannelStatsAsync(TenantId, ct)));

    [HttpGet("recruit/channel-report")]
    [AuthorizePermission("hrm.recruit.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RecruitChannelReportDto>>>> ChannelReport(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<RecruitChannelReportDto>>.Ok(await _svc.GetChannelReportAsync(TenantId, ct)));
}
