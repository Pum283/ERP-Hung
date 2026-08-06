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
[Route("api/lms/classes")]
public sealed class LmsClassController : ControllerBase
{
    private readonly ILmsClassService _svc;

    public LmsClassController(ILmsClassService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("lms.class.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsTrainingClassDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsTrainingClassDto>>.Ok(await _svc.ListClassesAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("lms.class.manage")]
    public async Task<ActionResult<ApiResponse<LmsTrainingClassDto>>> Upsert(
        [FromBody] LmsTrainingClassUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsTrainingClassDto>.Ok(await _svc.UpsertClassAsync(TenantId, UserId, req, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("lms.class.read")]
    public async Task<ActionResult<ApiResponse<LmsClassDetailDto>>> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponse<LmsClassDetailDto>.Ok(await _svc.GetClassDetailAsync(TenantId, id, ct)));

    [HttpPost("{id:guid}/sessions")]
    [AuthorizePermission("lms.class.manage")]
    public async Task<ActionResult<ApiResponse<LmsClassSessionDto>>> AddSession(
        Guid id, [FromBody] LmsClassSessionCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsClassSessionDto>.Ok(await _svc.AddSessionAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/enrollments")]
    [AuthorizePermission("lms.class.manage")]
    public async Task<ActionResult<ApiResponse<LmsClassEnrollmentDto>>> Enroll(
        Guid id, [FromBody] LmsClassEnrollmentRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsClassEnrollmentDto>.Ok(await _svc.EnrollAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/close")]
    [AuthorizePermission("lms.class.manage")]
    public async Task<ActionResult<ApiResponse<LmsTrainingClassDto>>> Close(
        Guid id, [FromBody] LmsClassCloseRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsTrainingClassDto>.Ok(await _svc.CloseClassAsync(TenantId, UserId, id, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/lms/sessions")]
public sealed class LmsSessionController : ControllerBase
{
    private readonly ILmsClassService _svc;

    public LmsSessionController(ILmsClassService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpPost("{id:guid}/attendance")]
    [AuthorizePermission("lms.class.manage")]
    public async Task<ActionResult<ApiResponse<LmsSessionAttendanceDto>>> RecordAttendance(
        Guid id, [FromBody] LmsSessionAttendanceRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsSessionAttendanceDto>.Ok(
            await _svc.RecordAttendanceAsync(TenantId, UserId, id, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/lms/mentors")]
public sealed class LmsMentorController : ControllerBase
{
    private readonly ILmsClassService _svc;

    public LmsMentorController(ILmsClassService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("lms.class.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsMentorAssignmentDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsMentorAssignmentDto>>.Ok(await _svc.ListMentorsAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("lms.class.manage")]
    public async Task<ActionResult<ApiResponse<LmsMentorAssignmentDto>>> Assign(
        [FromBody] LmsMentorAssignRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsMentorAssignmentDto>.Ok(await _svc.AssignMentorAsync(TenantId, UserId, req, ct)));
}
