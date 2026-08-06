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
[Route("api/lms/questions")]
public sealed class LmsQuestionController : ControllerBase
{
    private readonly ILmsExamService _svc;
    public LmsQuestionController(ILmsExamService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("lms.exam.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsQuestionDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsQuestionDto>>.Ok(await _svc.ListQuestionsAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("lms.exam.manage")]
    public async Task<ActionResult<ApiResponse<LmsQuestionDto>>> Upsert(
        [FromBody] LmsQuestionUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsQuestionDto>.Ok(await _svc.UpsertQuestionAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/lms/exams")]
public sealed class LmsExamController : ControllerBase
{
    private readonly ILmsExamService _svc;
    public LmsExamController(ILmsExamService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("lms.exam.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsExamDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsExamDto>>.Ok(await _svc.ListExamsAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("lms.exam.manage")]
    public async Task<ActionResult<ApiResponse<LmsExamDto>>> Upsert(
        [FromBody] LmsExamUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsExamDto>.Ok(await _svc.UpsertExamAsync(TenantId, UserId, req, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("lms.exam.read")]
    public async Task<ActionResult<ApiResponse<LmsExamDetailDto>>> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponse<LmsExamDetailDto>.Ok(await _svc.GetExamDetailAsync(TenantId, id, ct)));

    [HttpPost("{id:guid}/publish")]
    [AuthorizePermission("lms.exam.manage")]
    public async Task<ActionResult<ApiResponse<LmsExamDto>>> Publish(
        Guid id, [FromBody] LmsPublishExamRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsExamDto>.Ok(await _svc.SetExamStatusAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/questions")]
    [AuthorizePermission("lms.exam.manage")]
    public async Task<ActionResult<ApiResponse<LmsExamQuestionItemDto>>> AddQuestion(
        Guid id, [FromBody] LmsExamAddQuestionRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsExamQuestionItemDto>.Ok(
            await _svc.AddQuestionToExamAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/start")]
    [AuthorizePermission("lms.learn.read")]
    public async Task<ActionResult<ApiResponse<LmsAttemptDto>>> Start(Guid id, CancellationToken ct)
        => Ok(ApiResponse<LmsAttemptDto>.Ok(await _svc.StartAttemptAsync(TenantId, UserId, id, ct)));
}

[ApiController]
[Authorize]
[Route("api/lms/attempts")]
public sealed class LmsAttemptController : ControllerBase
{
    private readonly ILmsExamService _svc;
    public LmsAttemptController(ILmsExamService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpPost("{id:guid}/submit")]
    [AuthorizePermission("lms.learn.read")]
    public async Task<ActionResult<ApiResponse<LmsAttemptResultDto>>> Submit(
        Guid id, [FromBody] LmsSubmitAttemptRequest req, CancellationToken ct)
        => Ok(ApiResponse<LmsAttemptResultDto>.Ok(await _svc.SubmitAttemptAsync(TenantId, UserId, id, req, ct)));

    [HttpGet("{id:guid}/result")]
    [AuthorizePermission("lms.learn.read")]
    public async Task<ActionResult<ApiResponse<LmsAttemptResultDto>>> Result(Guid id, CancellationToken ct)
        => Ok(ApiResponse<LmsAttemptResultDto>.Ok(await _svc.GetAttemptResultAsync(TenantId, UserId, id, ct)));
}

[ApiController]
[Authorize]
[Route("api/lms/learn")]
public sealed class LmsLearnExamController : ControllerBase
{
    private readonly ILmsExamService _svc;
    public LmsLearnExamController(ILmsExamService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("{courseId:guid}/exams")]
    [AuthorizePermission("lms.learn.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsLearnerExamDto>>>> ListExams(
        Guid courseId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsLearnerExamDto>>.Ok(
            await _svc.ListLearnerExamsAsync(TenantId, UserId, courseId, ct)));
}

[ApiController]
[Authorize]
[Route("api/lms/certificates")]
public sealed class LmsCertificateController : ControllerBase
{
    private readonly ILmsExamService _svc;
    public LmsCertificateController(ILmsExamService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("mine")]
    [AuthorizePermission("lms.learn.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LmsCertificateDto>>>> Mine(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LmsCertificateDto>>.Ok(
            await _svc.ListMyCertificatesAsync(TenantId, UserId, ct)));
}
