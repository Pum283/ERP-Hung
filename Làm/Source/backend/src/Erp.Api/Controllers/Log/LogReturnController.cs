using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Log;
using Erp.Application.Interfaces.Services.Log;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Log;

[ApiController]
[Authorize]
[Route("api/log/returns")]
public sealed class LogReturnController : ControllerBase
{
    private readonly ILogReturnService _svc;
    public LogReturnController(ILogReturnService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("log.return.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LogReturnNoteDto>>>> List(
        [FromQuery] string? status, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LogReturnNoteDto>>.Ok(await _svc.ListAsync(TenantId, status, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("log.return.read")]
    public async Task<ActionResult<ApiResponse<LogReturnDetailDto>>> Detail(Guid id, CancellationToken ct)
        => Ok(ApiResponse<LogReturnDetailDto>.Ok(await _svc.GetDetailAsync(TenantId, id, ct)));

    [HttpPost]
    [AuthorizePermission("log.return.manage")]
    public async Task<ActionResult<ApiResponse<LogReturnDetailDto>>> Create(
        [FromBody] LogReturnCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogReturnDetailDto>.Ok(await _svc.CreateAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/count")]
    [AuthorizePermission("log.return.manage")]
    public async Task<ActionResult<ApiResponse<LogReturnLineDto>>> Count(
        Guid id, [FromBody] LogReturnCountRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogReturnLineDto>.Ok(await _svc.CountLineAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/confirm-count")]
    [AuthorizePermission("log.return.manage")]
    public async Task<ActionResult<ApiResponse<LogReturnDetailDto>>> ConfirmCount(Guid id, CancellationToken ct)
        => Ok(ApiResponse<LogReturnDetailDto>.Ok(await _svc.ConfirmCountAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/post")]
    [AuthorizePermission("log.return.manage")]
    public async Task<ActionResult<ApiResponse<LogReturnDetailDto>>> Post(Guid id, CancellationToken ct)
        => Ok(ApiResponse<LogReturnDetailDto>.Ok(await _svc.PostAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/cancel")]
    [AuthorizePermission("log.return.manage")]
    public async Task<ActionResult<ApiResponse<LogReturnDetailDto>>> Cancel(
        Guid id, [FromBody] LogStatusRequest? req, CancellationToken ct)
        => Ok(ApiResponse<LogReturnDetailDto>.Ok(await _svc.CancelAsync(TenantId, UserId, id, req?.Note, ct)));
}

[ApiController]
[Authorize]
[Route("api/log/reports")]
public sealed class LogReportController : ControllerBase
{
    private readonly ILogReturnService _svc;
    public LogReportController(ILogReturnService svc) => _svc = svc;
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("ops")]
    [AuthorizePermission("log.delivery.read")]
    public async Task<ActionResult<ApiResponse<LogOpsReportDto>>> Ops(CancellationToken ct)
        => Ok(ApiResponse<LogOpsReportDto>.Ok(await _svc.GetOpsReportAsync(TenantId, ct)));
}
