using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Lms;
using Erp.Application.Interfaces.Services.Lms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Lms;

[ApiController]
[Authorize]
[Route("api/lms/programs")]
public sealed class LmsProgramController : ControllerBase
{
    private readonly ILmsCourseService _svc;
    public LmsProgramController(ILmsCourseService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("lms.course.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsProgramDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsProgramDto>>.Ok(await _svc.ListProgramsAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("lms.course.manage")]
    public async Task<ActionResult<ApiResponse<LmsProgramDto>>> Upsert(
        [FromBody] LmsProgramUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsProgramDto>.Ok(await _svc.UpsertProgramAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/lms/courses")]
public sealed class LmsCourseController : ControllerBase
{
    private readonly ILmsCourseService _svc;
    public LmsCourseController(ILmsCourseService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("lms.course.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsCourseDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsCourseDto>>.Ok(await _svc.ListCoursesAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("lms.course.manage")]
    public async Task<ActionResult<ApiResponse<LmsCourseDto>>> Upsert(
        [FromBody] LmsCourseUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsCourseDto>.Ok(await _svc.UpsertCourseAsync(TenantId, UserId, req, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("lms.course.read")]
    public async Task<ActionResult<ApiResponse<LmsCourseDetailDto>>> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponse<LmsCourseDetailDto>.Ok(await _svc.GetCourseDetailAsync(TenantId, id, ct)));

    [HttpPost("{id:guid}/publish")]
    [AuthorizePermission("lms.course.manage")]
    public async Task<ActionResult<ApiResponse<LmsCourseDto>>> Publish(
        Guid id, [FromBody] LmsPublishCourseRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsCourseDto>.Ok(await _svc.SetPublishStatusAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/chapters")]
    [AuthorizePermission("lms.course.manage")]
    public async Task<ActionResult<ApiResponse<LmsChapterDto>>> UpsertChapter(
        Guid id, [FromBody] LmsChapterUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsChapterDto>.Ok(await _svc.UpsertChapterAsync(TenantId, UserId, id, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/lms/chapters")]
public sealed class LmsChapterController : ControllerBase
{
    private readonly ILmsCourseService _svc;
    public LmsChapterController(ILmsCourseService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpPost("{id:guid}/lessons")]
    [AuthorizePermission("lms.course.manage")]
    public async Task<ActionResult<ApiResponse<LmsLessonDto>>> UpsertLesson(
        Guid id, [FromBody] LmsLessonUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsLessonDto>.Ok(await _svc.UpsertLessonAsync(TenantId, UserId, id, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/lms/catalog")]
public sealed class LmsCatalogController : ControllerBase
{
    private readonly ILmsCourseService _svc;
    public LmsCatalogController(ILmsCourseService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("lms.learn.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsCatalogCourseDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsCatalogCourseDto>>.Ok(await _svc.ListCatalogAsync(TenantId, UserId, ct)));

    [HttpPost("{courseId:guid}/enroll")]
    [AuthorizePermission("lms.learn.enroll")]
    public async Task<ActionResult<ApiResponse<LmsOnlineEnrollmentDto>>> Enroll(
        Guid courseId, [FromBody] LmsEnrollRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsOnlineEnrollmentDto>.Ok(await _svc.EnrollAsync(TenantId, UserId, courseId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/lms/learn")]
public sealed class LmsLearnController : ControllerBase
{
    private readonly ILmsCourseService _svc;
    public LmsLearnController(ILmsCourseService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("{courseId:guid}")]
    [AuthorizePermission("lms.learn.read")]
    public async Task<ActionResult<ApiResponse<LmsLearnCourseDto>>> Get(Guid courseId, CancellationToken ct)
        => Ok(ApiResponse<LmsLearnCourseDto>.Ok(await _svc.GetLearnAsync(TenantId, UserId, courseId, ct)));

    [HttpPost("{courseId:guid}/lessons/{lessonId:guid}/complete")]
    [AuthorizePermission("lms.learn.read")]
    public async Task<ActionResult<ApiResponse<LmsLessonProgressDto>>> Complete(
        Guid courseId, Guid lessonId, [FromBody] LmsCompleteLessonRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsLessonProgressDto>.Ok(
            await _svc.CompleteLessonAsync(TenantId, UserId, courseId, lessonId, req, ct)));
}
