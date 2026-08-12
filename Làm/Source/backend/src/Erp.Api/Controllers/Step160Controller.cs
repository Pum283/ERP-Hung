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
[Route("api/step160")]
public sealed class Step160Controller : ControllerBase
{
    private readonly IStep160Service _svc;

    public Step160Controller(IStep160Service svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    // ────────────────────────────────────────────────────────────────────────────
    // UC_HRM_181: Tổng hợp kết quả đánh giá
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("hrm/evaluation-summary-report")]
    [AuthorizePermission("hrm.report.read")]
    public async Task<ActionResult<ApiResponse<HrmEvaluationSummaryReportDto>>> GetEvaluationSummaryReport([FromQuery] Guid cycleId, CancellationToken ct)
        => Ok(ApiResponse<HrmEvaluationSummaryReportDto>.Ok(await _svc.GetEvaluationSummaryReportAsync(TenantId, cycleId, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_007: Gắn tag kỹ năng / vị trí
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("lms/course-skill-tags")]
    [AuthorizePermission("lms.course.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsCourseSkillTagDto>>>> GetCourseSkillTags([FromQuery] Guid? courseId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsCourseSkillTagDto>>.Ok(await _svc.GetCourseSkillTagsAsync(TenantId, courseId, ct)));

    [HttpPost("lms/course-skill-tags")]
    [AuthorizePermission("lms.course.write")]
    public async Task<ActionResult<ApiResponse<LmsCourseSkillTagDto>>> CreateCourseSkillTag([FromBody] LmsCourseSkillTagUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsCourseSkillTagDto>.Ok(await _svc.CreateCourseSkillTagAsync(TenantId, req, ct)));

    [HttpDelete("lms/course-skill-tags/{id:guid}")]
    [AuthorizePermission("lms.course.write")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCourseSkillTag(Guid id, CancellationToken ct)
    {
        await _svc.DeleteCourseSkillTagAsync(TenantId, id, ct);
        return Ok(ApiResponse<bool>.Ok(true));
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_008: Phiên bản nội dung khóa học
    // ────────────────────────────────────────────────────────────────────────────

    [HttpGet("lms/course-versions")]
    [AuthorizePermission("lms.course.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsCourseVersionDto>>>> GetCourseVersions([FromQuery] Guid courseId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsCourseVersionDto>>.Ok(await _svc.GetCourseVersionsAsync(TenantId, courseId, ct)));

    [HttpPost("lms/course-versions")]
    [AuthorizePermission("lms.course.write")]
    public async Task<ActionResult<ApiResponse<LmsCourseVersionDto>>> CreateCourseVersion([FromBody] LmsCourseVersionUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsCourseVersionDto>.Ok(await _svc.CreateCourseVersionAsync(TenantId, req, ct)));

    // ────────────────────────────────────────────────────────────────────────────
    // UC_LMS_013: Tạo đề thi random
    // ────────────────────────────────────────────────────────────────────────────

    [HttpPost("lms/exams/random-generate")]
    [AuthorizePermission("lms.exam.write")]
    public async Task<ActionResult<ApiResponse<LmsRandomExamResult>>> GenerateRandomExam([FromBody] LmsRandomExamRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsRandomExamResult>.Ok(await _svc.GenerateRandomExamAsync(TenantId, req, ct)));
}
