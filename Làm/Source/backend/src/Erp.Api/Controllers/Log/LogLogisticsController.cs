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
[Route("api/log/carriers")]
public sealed class LogCarrierController : ControllerBase
{
    private readonly ILogLogisticsService _svc;
    public LogCarrierController(ILogLogisticsService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("log.carrier.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LogCarrierDto>>>> List(
        [FromQuery] string? q, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LogCarrierDto>>.Ok(await _svc.ListCarriersAsync(TenantId, q, ct)));

    [HttpPost]
    [AuthorizePermission("log.carrier.manage")]
    public async Task<ActionResult<ApiResponse<LogCarrierDto>>> Upsert(
        [FromBody] LogCarrierUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogCarrierDto>.Ok(await _svc.UpsertCarrierAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/log/deliveries")]
public sealed class LogDeliveryController : ControllerBase
{
    private readonly ILogLogisticsService _svc;
    private readonly ILogCodService _cod;
    public LogDeliveryController(ILogLogisticsService svc, ILogCodService cod)
    {
        _svc = svc;
        _cod = cod;
    }
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("log.delivery.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LogDeliveryOrderDto>>>> List(
        [FromQuery] string? q, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<LogDeliveryOrderDto>>.Ok(await _svc.ListDeliveriesAsync(TenantId, q, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("log.delivery.read")]
    public async Task<ActionResult<ApiResponse<LogDeliveryDetailDto>>> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponse<LogDeliveryDetailDto>.Ok(await _svc.GetDeliveryDetailAsync(TenantId, id, ct)));

    [HttpPost]
    [AuthorizePermission("log.delivery.manage")]
    public async Task<ActionResult<ApiResponse<LogDeliveryOrderDto>>> Upsert(
        [FromBody] LogDeliveryUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogDeliveryOrderDto>.Ok(await _svc.UpsertDeliveryAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/lines")]
    [AuthorizePermission("log.delivery.manage")]
    public async Task<ActionResult<ApiResponse<LogDeliveryLineDto>>> UpsertLine(
        Guid id, [FromBody] LogDeliveryLineUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogDeliveryLineDto>.Ok(await _svc.UpsertLineAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/confirm")]
    [AuthorizePermission("log.delivery.manage")]
    public async Task<ActionResult<ApiResponse<LogDeliveryOrderDto>>> Confirm(Guid id, CancellationToken ct)
        => Ok(ApiResponse<LogDeliveryOrderDto>.Ok(await _svc.ConfirmAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/split")]
    [AuthorizePermission("log.delivery.manage")]
    public async Task<ActionResult<ApiResponse<LogDeliveryOrderDto>>> Split(
        Guid id, [FromBody] LogSplitBatchRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogDeliveryOrderDto>.Ok(await _svc.SplitBatchAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/start-pick")]
    [AuthorizePermission("log.delivery.manage")]
    public async Task<ActionResult<ApiResponse<LogDeliveryOrderDto>>> StartPick(Guid id, CancellationToken ct)
        => Ok(ApiResponse<LogDeliveryOrderDto>.Ok(await _svc.StartPickAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/confirm-pick")]
    [AuthorizePermission("log.delivery.manage")]
    public async Task<ActionResult<ApiResponse<LogDeliveryOrderDto>>> ConfirmPick(
        Guid id, [FromBody] LogPickRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogDeliveryOrderDto>.Ok(await _svc.ConfirmPickAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/dispatch")]
    [AuthorizePermission("log.delivery.manage")]
    public async Task<ActionResult<ApiResponse<LogDeliveryOrderDto>>> Dispatch(Guid id, CancellationToken ct)
        => Ok(ApiResponse<LogDeliveryOrderDto>.Ok(await _svc.DispatchAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/waybill")]
    [AuthorizePermission("log.delivery.manage")]
    public async Task<ActionResult<ApiResponse<LogDeliveryOrderDto>>> Waybill(Guid id, CancellationToken ct)
        => Ok(ApiResponse<LogDeliveryOrderDto>.Ok(await _svc.PrintWaybillAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/assign")]
    [AuthorizePermission("log.delivery.manage")]
    public async Task<ActionResult<ApiResponse<LogDeliveryOrderDto>>> Assign(
        Guid id, [FromBody] LogAssignRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogDeliveryOrderDto>.Ok(await _svc.AssignAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/status")]
    [AuthorizePermission("log.delivery.manage")]
    public async Task<ActionResult<ApiResponse<LogDeliveryOrderDto>>> Status(
        Guid id, [FromBody] LogStatusRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogDeliveryOrderDto>.Ok(await _svc.UpdateStatusAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/cancel")]
    [AuthorizePermission("log.delivery.manage")]
    public async Task<ActionResult<ApiResponse<LogDeliveryOrderDto>>> Cancel(
        Guid id, [FromBody] LogStatusRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogDeliveryOrderDto>.Ok(await _svc.CancelAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/return")]
    [AuthorizePermission("log.delivery.manage")]
    public async Task<ActionResult<ApiResponse<LogDeliveryOrderDto>>> Return(
        Guid id, [FromBody] LogStatusRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogDeliveryOrderDto>.Ok(await _svc.ReturnAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/fail")]
    [AuthorizePermission("log.delivery.manage")]
    public async Task<ActionResult<ApiResponse<LogDeliveryOrderDto>>> Fail(
        Guid id, [FromBody] LogFailRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogDeliveryOrderDto>.Ok(await _svc.FailAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/cod/mark")]
    [AuthorizePermission("log.cod.manage")]
    public async Task<ActionResult<ApiResponse<LogDeliveryOrderDto>>> MarkCod(
        Guid id, [FromBody] LogCodMarkRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogDeliveryOrderDto>.Ok(await _cod.MarkCodAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/cod/amount")]
    [AuthorizePermission("log.cod.manage")]
    public async Task<ActionResult<ApiResponse<LogDeliveryOrderDto>>> SetCodAmount(
        Guid id, [FromBody] LogCodAmountRequest req, CancellationToken ct)
        => Ok(ApiResponse<LogDeliveryOrderDto>.Ok(await _cod.SetCodAmountAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/cod/collect")]
    [AuthorizePermission("log.cod.manage")]
    public async Task<ActionResult<ApiResponse<LogDeliveryOrderDto>>> CollectCod(
        Guid id, [FromBody] LogCodCollectRequest? req, CancellationToken ct)
        => Ok(ApiResponse<LogDeliveryOrderDto>.Ok(
            await _cod.ConfirmCollectedAsync(TenantId, UserId, id, req ?? new LogCodCollectRequest(null), ct)));
}
