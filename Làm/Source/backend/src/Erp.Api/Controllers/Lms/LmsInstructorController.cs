using System.Security.Claims;
using System.Text;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Lms;
using Erp.Application.Interfaces.Services.Lms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Lms;

[ApiController]
[Authorize]
[Route("api/lms/instructors")]
public sealed class LmsInstructorController : ControllerBase
{
    private readonly ILmsInstructorService _svc;
    public LmsInstructorController(ILmsInstructorService svc) => _svc = svc;
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                                      ?? User.FindFirstValue("sub")!);

    [HttpGet]
    [AuthorizePermission("lms.instructor.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsInstructorDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsInstructorDto>>.Ok(await _svc.ListAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("lms.instructor.manage")]
    public async Task<ActionResult<ApiResponse<LmsInstructorDto>>> Upsert(
        [FromBody] LmsInstructorUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsInstructorDto>.Ok(await _svc.UpsertAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/status")]
    [AuthorizePermission("lms.instructor.manage")]
    public async Task<ActionResult<ApiResponse<LmsInstructorDto>>> Status(
        Guid id, [FromBody] StatusBody body, CancellationToken ct)
        => Ok(ApiResponse<LmsInstructorDto>.Ok(
            await _svc.SetStatusAsync(TenantId, UserId, id, body.Status, ct)));

    [HttpPost("{id:guid}/grant-role")]
    [AuthorizePermission("lms.instructor.manage")]
    public async Task<ActionResult<ApiResponse<object>>> GrantRole(Guid id, CancellationToken ct)
    {
        await _svc.GrantRoleAsync(TenantId, UserId, id, ct);
        return Ok(ApiResponse<object>.Ok(new { granted = true }));
    }

    public sealed record StatusBody(string Status);
}

[ApiController]
[Authorize]
[Route("api/lms/reports")]
public sealed class LmsReportController : ControllerBase
{
    private readonly ILmsReportService _svc;
    public LmsReportController(ILmsReportService svc) => _svc = svc;
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("dashboard")]
    [AuthorizePermission("lms.report.read")]
    public async Task<ActionResult<ApiResponse<LmsDashboardDto>>> Dashboard(CancellationToken ct)
        => Ok(ApiResponse<LmsDashboardDto>.Ok(await _svc.DashboardAsync(TenantId, ct)));

    [HttpGet("by-org")]
    [AuthorizePermission("lms.report.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsCompletionByOrgRowDto>>>> ByOrg(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsCompletionByOrgRowDto>>.Ok(
            await _svc.CompletionByOrgAsync(TenantId, ct)));

    [HttpGet("learners")]
    [AuthorizePermission("lms.report.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsLearnerRowDto>>>> Learners(
        [FromQuery] Guid? classId, [FromQuery] Guid? courseId, [FromQuery] Guid? instructorId,
        CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsLearnerRowDto>>.Ok(
            await _svc.LearnersAsync(TenantId, classId, courseId, instructorId, ct)));

    [HttpGet("export.csv")]
    [AuthorizePermission("lms.report.read")]
    public async Task<IActionResult> Export(
        [FromQuery] string report, [FromQuery] Guid? classId = null, [FromQuery] Guid? courseId = null,
        [FromQuery] Guid? instructorId = null, CancellationToken ct = default)
    {
        var csv = await _svc.ExportCsvAsync(TenantId, report, classId, courseId, instructorId, ct);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"lms-{report}-{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
