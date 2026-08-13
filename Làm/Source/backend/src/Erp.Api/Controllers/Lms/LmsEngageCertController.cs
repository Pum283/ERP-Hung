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
[Route("api/lms/engage-cert")]
public sealed class LmsEngageCertController : ControllerBase
{
    private readonly ILmsEngageCertService _svc;

    public LmsEngageCertController(ILmsEngageCertService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_038: Nhắc học tiếp
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("lms/study-reminders")]
    [AuthorizePermission("lms.course.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsStudyReminderDto>>>> GetStudyReminders(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsStudyReminderDto>>.Ok(await _svc.GetStudyRemindersAsync(TenantId, UserId, ct)));

    [HttpPost("lms/study-reminders")]
    [AuthorizePermission("lms.course.write")]
    public async Task<ActionResult<ApiResponse<LmsStudyReminderDto>>> CreateStudyReminder([FromBody] LmsStudyReminderUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsStudyReminderDto>.Ok(await _svc.CreateStudyReminderAsync(TenantId, UserId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_039: Diễn đàn / bình luận
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("lms/forum-topics")]
    [AuthorizePermission("lms.course.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsForumTopicDto>>>> GetForumTopics([FromQuery] Guid courseId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsForumTopicDto>>.Ok(await _svc.GetForumTopicsAsync(TenantId, courseId, ct)));

    [HttpPost("lms/forum-topics")]
    [AuthorizePermission("lms.course.write")]
    public async Task<ActionResult<ApiResponse<LmsForumTopicDto>>> CreateForumTopic([FromBody] LmsForumTopicUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsForumTopicDto>.Ok(await _svc.CreateForumTopicAsync(TenantId, UserId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_046: Mã xác thực chứng chỉ
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("lms/certificates/verify")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LmsCertificateVerificationResultDto>>> VerifyCertificate([FromQuery] string code, [FromHeader(Name = "X-Tenant-Id")] Guid? headerTenantId, CancellationToken ct)
    {
        var tenantId = User.Identity?.IsAuthenticated == true ? TenantId : (headerTenantId ?? Guid.Empty);
        return Ok(ApiResponse<LmsCertificateVerificationResultDto>.Ok(await _svc.VerifyCertificateAsync(tenantId, code, ct)));
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_047: Thu hồi chứng chỉ
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("lms/certificates/revoke")]
    [AuthorizePermission("lms.certificate.write")]
    public async Task<ActionResult<ApiResponse<LmsCertificateRevocationDto>>> RevokeCertificate([FromBody] LmsRevokeCertificateRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsCertificateRevocationDto>.Ok(await _svc.RevokeCertificateAsync(TenantId, UserId, req, ct)));
}
