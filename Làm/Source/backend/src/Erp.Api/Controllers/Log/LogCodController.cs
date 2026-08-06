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
[Route("api/log/cod")]
public sealed class LogCodController : ControllerBase
{
    private readonly ILogCodService _svc;
    public LogCodController(ILogCodService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("log.cod.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LogDeliveryOrderDto>>>> List(
        [FromQuery] string? status, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LogDeliveryOrderDto>>.Ok(
            await _svc.ListCodDeliveriesAsync(TenantId, status, ct)));

    [HttpGet("overdue")]
    [AuthorizePermission("log.cod.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LogDeliveryOrderDto>>>> Overdue(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LogDeliveryOrderDto>>.Ok(await _svc.ListOverdueAsync(TenantId, ct)));

    [HttpGet("report")]
    [AuthorizePermission("log.cod.read")]
    public async Task<ActionResult<ApiResponse<LogCodReportDto>>> Report(CancellationToken ct)
        => Ok(ApiResponse<LogCodReportDto>.Ok(await _svc.GetReportAsync(TenantId, ct)));

    [HttpGet("handovers")]
    [AuthorizePermission("log.cod.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LogCodHandoverDto>>>> ListHandovers(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LogCodHandoverDto>>.Ok(await _svc.ListHandoversAsync(TenantId, ct)));

    [HttpGet("handovers/{id:guid}")]
    [AuthorizePermission("log.cod.read")]
    public async Task<ActionResult<ApiResponse<LogCodHandoverDetailDto>>> GetHandover(Guid id, CancellationToken ct)
        => Ok(ApiResponse<LogCodHandoverDetailDto>.Ok(await _svc.GetHandoverAsync(TenantId, id, ct)));

    [HttpPost("handovers")]
    [AuthorizePermission("log.cod.manage")]
    public async Task<ActionResult<ApiResponse<LogCodHandoverDetailDto>>> CreateHandover(
        [FromBody] LogCodHandoverCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogCodHandoverDetailDto>.Ok(await _svc.CreateHandoverAsync(TenantId, UserId, req, ct)));

    [HttpPost("handovers/{id:guid}/submit")]
    [AuthorizePermission("log.cod.manage")]
    public async Task<ActionResult<ApiResponse<LogCodHandoverDetailDto>>> Submit(Guid id, CancellationToken ct)
        => Ok(ApiResponse<LogCodHandoverDetailDto>.Ok(await _svc.SubmitHandoverAsync(TenantId, UserId, id, ct)));

    [HttpPost("handovers/{id:guid}/reconcile")]
    [AuthorizePermission("log.cod.manage")]
    public async Task<ActionResult<ApiResponse<LogCodHandoverDetailDto>>> Reconcile(
        Guid id, [FromBody] LogCodReconcileRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogCodHandoverDetailDto>.Ok(
            await _svc.ReconcileHandoverAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("handovers/{id:guid}/resolve-variance")]
    [AuthorizePermission("log.cod.manage")]
    public async Task<ActionResult<ApiResponse<LogCodHandoverDetailDto>>> ResolveVariance(
        Guid id, [FromBody] LogCodResolveVarianceRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogCodHandoverDetailDto>.Ok(
            await _svc.ResolveVarianceAsync(TenantId, UserId, id, req, ct)));
}
