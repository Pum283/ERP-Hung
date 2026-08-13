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
[Route("api/lms/training-reports")]
public sealed class LmsTrainingReportsController : ControllerBase
{
    private readonly ILmsTrainingReportsService _svc;

    public LmsTrainingReportsController(ILmsTrainingReportsService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_064: Cảnh báo quá hạn đào tạo
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("lms/alerts/overdue")]
    [AuthorizePermission("lms.report.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsOverdueTrainingAlertDto>>>> GetOverdueTrainingAlerts([FromQuery] Guid? userId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsOverdueTrainingAlertDto>>.Ok(await _svc.GetOverdueTrainingAlertsAsync(TenantId, userId, ct)));

    [HttpPost("lms/alerts/overdue/trigger")]
    [AuthorizePermission("lms.course.write")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsOverdueTrainingAlertDto>>>> TriggerOverdueCheck(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsOverdueTrainingAlertDto>>.Ok(await _svc.TriggerOverdueCheckAsync(TenantId, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_067: Báo cáo điểm thi / tỷ lệ đạt
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("lms/reports/exam-analytics")]
    [AuthorizePermission("lms.report.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsExamAnalyticsReportDto>>>> GetExamAnalyticsReport([FromQuery] Guid? examId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsExamAnalyticsReportDto>>.Ok(await _svc.GetExamAnalyticsReportAsync(TenantId, examId, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_068: Báo cáo học viên bỏ dở
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("lms/reports/dropout-analytics")]
    [AuthorizePermission("lms.report.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsDropoutAnalyticsReportDto>>>> GetDropoutAnalyticsReport([FromQuery] Guid? courseId, [FromQuery] int inactiveDaysThreshold = 14, CancellationToken ct = default)
        => Ok(ApiResponse<IReadOnlyList<LmsDropoutAnalyticsReportDto>>.Ok(await _svc.GetDropoutAnalyticsReportAsync(TenantId, courseId, inactiveDaysThreshold, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_069: Báo cáo hiệu quả khóa
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("lms/reports/course-engagement")]
    [AuthorizePermission("lms.report.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsCourseEngagementReportDto>>>> GetCourseEngagementReport([FromQuery] Guid? courseId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsCourseEngagementReportDto>>.Ok(await _svc.GetCourseEngagementReportAsync(TenantId, courseId, ct)));
}
