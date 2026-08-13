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
[Route("api/lms/path-tracking")]
public sealed class LmsPathTrackingController : ControllerBase
{
    private readonly ILmsPathTrackingService _svc;

    public LmsPathTrackingController(ILmsPathTrackingService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_060: Báo cáo tỷ lệ xác nhận
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("lms/reports/acknowledgements")]
    [AuthorizePermission("lms.report.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsAcknowledgementReportDto>>>> GetAcknowledgementReport([FromQuery] string? department, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsAcknowledgementReportDto>>.Ok(await _svc.GetAcknowledgementReportAsync(TenantId, department, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_061: Gán lộ trình theo chức danh
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("lms/learning-paths")]
    [AuthorizePermission("lms.course.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsLearningPathDto>>>> GetLearningPaths([FromQuery] string? jobTitle, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsLearningPathDto>>.Ok(await _svc.GetLearningPathsAsync(TenantId, jobTitle, ct)));

    [HttpPost("lms/learning-paths")]
    [AuthorizePermission("lms.course.write")]
    public async Task<ActionResult<ApiResponse<LmsLearningPathDto>>> CreateLearningPath([FromBody] LmsLearningPathUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsLearningPathDto>.Ok(await _svc.CreateLearningPathAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_062: Tự gán khóa bắt buộc khi nhận việc
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("lms/learning-paths/auto-assign-on-hire")]
    [AuthorizePermission("lms.course.write")]
    public async Task<ActionResult<ApiResponse<LmsAutoAssignOnHireResultDto>>> AutoAssignOnHire([FromQuery] Guid targetUserId, [FromQuery] string jobTitle, CancellationToken ct)
        => Ok(ApiResponse<LmsAutoAssignOnHireResultDto>.Ok(await _svc.AutoAssignOnHireAsync(TenantId, targetUserId != Guid.Empty ? targetUserId : UserId, jobTitle, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_063: Theo dõi hoàn thành lộ trình
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("lms/learning-paths/progress")]
    [AuthorizePermission("lms.report.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsUserLearningPathProgressDto>>>> GetUserLearningPathProgress([FromQuery] Guid? userId, [FromQuery] string? jobTitle, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsUserLearningPathProgressDto>>.Ok(await _svc.GetUserLearningPathProgressAsync(TenantId, userId, jobTitle, ct)));
}
