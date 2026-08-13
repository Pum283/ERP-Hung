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
[Route("api/lms/cert-sync")]
public sealed class LmsCertSyncOpsController : ControllerBase
{
    private readonly ILmsCertSyncOpsService _svc;

    public LmsCertSyncOpsController(ILmsCertSyncOpsService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_048: Đồng bộ chứng chỉ sang HRM
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("lms/certificates/sync-to-hrm")]
    [AuthorizePermission("lms.certificate.write")]
    public async Task<ActionResult<ApiResponse<LmsHrmCertificateSyncResultDto>>> SyncCertificateToHrm([FromQuery] Guid certificateId, CancellationToken ct)
        => Ok(ApiResponse<LmsHrmCertificateSyncResultDto>.Ok(await _svc.SyncCertificateToHrmAsync(TenantId, certificateId, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_052: Phản hồi bài tập
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("lms/assignments/feedback")]
    [AuthorizePermission("lms.course.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsAssignmentFeedbackDto>>>> GetAssignmentFeedbacks([FromQuery] Guid lessonId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsAssignmentFeedbackDto>>.Ok(await _svc.GetAssignmentFeedbacksAsync(TenantId, lessonId, ct)));

    [HttpPost("lms/assignments/feedback")]
    [AuthorizePermission("lms.course.write")]
    public async Task<ActionResult<ApiResponse<LmsAssignmentFeedbackDto>>> CreateAssignmentFeedback([FromBody] LmsAssignmentFeedbackUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsAssignmentFeedbackDto>.Ok(await _svc.CreateAssignmentFeedbackAsync(TenantId, UserId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_053: Thống kê doanh thu theo khóa
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("lms/courses/revenue-stats")]
    [AuthorizePermission("lms.report.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsCourseRevenueStatDto>>>> GetCourseRevenueStats(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsCourseRevenueStatDto>>.Ok(await _svc.GetCourseRevenueStatsAsync(TenantId, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_054: Chống chia sẻ tài khoản
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("lms/account-sharing/validate-session")]
    [AuthorizePermission("lms.course.read")]
    public async Task<ActionResult<ApiResponse<LmsAccountSharingGuardDto>>> ValidateAccountSession([FromBody] LmsSessionValidationRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsAccountSharingGuardDto>.Ok(await _svc.ValidateAccountSessionAsync(TenantId, UserId, req, ct)));
}
