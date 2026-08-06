using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Inv;
using Erp.Application.Interfaces.Services.Inv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Inv;

[ApiController]
[Authorize]
[Route("api/inv/balances")]
public sealed class InvBalanceController : ControllerBase
{
    private readonly IInvStockService _svc;
    public InvBalanceController(IInvStockService svc) => _svc = svc;
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("inv.stock.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvBalanceDto>>>> List(
        [FromQuery] Guid? warehouseId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<InvBalanceDto>>.Ok(await _svc.ListBalancesAsync(TenantId, warehouseId, ct)));
}

[ApiController]
[Authorize]
[Route("api/inv/docs")]
public sealed class InvStockDocController : ControllerBase
{
    private readonly IInvStockService _svc;
    public InvStockDocController(IInvStockService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("inv.stock.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvStockDocDto>>>> List(
        [FromQuery] string? docType, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<InvStockDocDto>>.Ok(await _svc.ListDocsAsync(TenantId, docType, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("inv.stock.read")]
    public async Task<ActionResult<ApiResponse<InvStockDocDetailDto>>> Detail(Guid id, CancellationToken ct)
        => Ok(ApiResponse<InvStockDocDetailDto>.Ok(await _svc.GetDocDetailAsync(TenantId, id, ct)));

    [HttpPost]
    [AuthorizePermission("inv.stock.manage")]
    public async Task<ActionResult<ApiResponse<InvStockDocDto>>> Create(
        [FromBody] InvStockDocCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvStockDocDto>.Ok(await _svc.CreateDocAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/lines")]
    [AuthorizePermission("inv.stock.manage")]
    public async Task<ActionResult<ApiResponse<InvStockDocLineDto>>> UpsertLine(
        Guid id, [FromBody] InvStockDocLineRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvStockDocLineDto>.Ok(await _svc.UpsertDocLineAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/post")]
    [AuthorizePermission("inv.stock.manage")]
    public async Task<ActionResult<ApiResponse<InvStockDocDto>>> Post(Guid id, CancellationToken ct)
        => Ok(ApiResponse<InvStockDocDto>.Ok(await _svc.PostDocAsync(TenantId, UserId, id, ct)));

    [HttpPost("suggest-lots")]
    [AuthorizePermission("inv.stock.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvLotPickDto>>>> SuggestLots(
        [FromBody] InvSuggestLotsRequest req, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<InvLotPickDto>>.Ok(await _svc.SuggestLotsAsync(TenantId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/inv/reservations")]
public sealed class InvReservationController : ControllerBase
{
    private readonly IInvStockService _svc;
    public InvReservationController(IInvStockService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("inv.stock.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvReservationDto>>>> List(
        [FromQuery] string? status, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<InvReservationDto>>.Ok(await _svc.ListReservationsAsync(TenantId, status, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("inv.stock.read")]
    public async Task<ActionResult<ApiResponse<InvReservationDetailDto>>> Detail(Guid id, CancellationToken ct)
        => Ok(ApiResponse<InvReservationDetailDto>.Ok(await _svc.GetReservationDetailAsync(TenantId, id, ct)));

    [HttpPost]
    [AuthorizePermission("inv.stock.manage")]
    public async Task<ActionResult<ApiResponse<InvReservationDetailDto>>> Create(
        [FromBody] InvReservationCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvReservationDetailDto>.Ok(await _svc.CreateReservationAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/activate")]
    [AuthorizePermission("inv.stock.manage")]
    public async Task<ActionResult<ApiResponse<InvReservationDetailDto>>> Activate(Guid id, CancellationToken ct)
        => Ok(ApiResponse<InvReservationDetailDto>.Ok(await _svc.ActivateReservationAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/release")]
    [AuthorizePermission("inv.stock.manage")]
    public async Task<ActionResult<ApiResponse<InvReservationDetailDto>>> Release(Guid id, CancellationToken ct)
        => Ok(ApiResponse<InvReservationDetailDto>.Ok(await _svc.ReleaseReservationAsync(TenantId, UserId, id, ct)));

    [HttpGet("~/api/inv/atp-alerts")]
    [AuthorizePermission("inv.stock.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvAtpAlertRowDto>>>> AtpAlerts(
        [FromQuery] Guid? warehouseId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<InvAtpAlertRowDto>>.Ok(await _svc.AtpAlertsAsync(TenantId, warehouseId, ct)));
}

[ApiController]
[Authorize]
[Route("api/inv/transfers")]
public sealed class InvTransferController : ControllerBase
{
    private readonly IInvStockService _svc;
    public InvTransferController(IInvStockService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("inv.stock.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvTransferDto>>>> List(
        [FromQuery] string? status, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<InvTransferDto>>.Ok(await _svc.ListTransfersAsync(TenantId, status, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("inv.stock.read")]
    public async Task<ActionResult<ApiResponse<InvTransferDetailDto>>> Detail(Guid id, CancellationToken ct)
        => Ok(ApiResponse<InvTransferDetailDto>.Ok(await _svc.GetTransferDetailAsync(TenantId, id, ct)));

    [HttpPost]
    [AuthorizePermission("inv.stock.manage")]
    public async Task<ActionResult<ApiResponse<InvTransferDto>>> Create(
        [FromBody] InvTransferCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvTransferDto>.Ok(await _svc.CreateTransferAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/lines")]
    [AuthorizePermission("inv.stock.manage")]
    public async Task<ActionResult<ApiResponse<InvTransferLineDto>>> UpsertLine(
        Guid id, [FromBody] InvTransferLineRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvTransferLineDto>.Ok(await _svc.UpsertTransferLineAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/ship")]
    [AuthorizePermission("inv.stock.manage")]
    public async Task<ActionResult<ApiResponse<InvTransferDto>>> Ship(Guid id, CancellationToken ct)
        => Ok(ApiResponse<InvTransferDto>.Ok(await _svc.ShipTransferAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/receive")]
    [AuthorizePermission("inv.stock.manage")]
    public async Task<ActionResult<ApiResponse<InvTransferDto>>> Receive(Guid id, CancellationToken ct)
        => Ok(ApiResponse<InvTransferDto>.Ok(await _svc.ReceiveTransferAsync(TenantId, UserId, id, ct)));
}

[ApiController]
[Authorize]
[Route("api/inv/stocktakes")]
public sealed class InvStocktakeController : ControllerBase
{
    private readonly IInvStockService _svc;
    public InvStocktakeController(IInvStockService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("inv.stocktake.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvStocktakeDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<InvStocktakeDto>>.Ok(await _svc.ListStocktakesAsync(TenantId, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("inv.stocktake.read")]
    public async Task<ActionResult<ApiResponse<InvStocktakeDetailDto>>> Detail(Guid id, CancellationToken ct)
        => Ok(ApiResponse<InvStocktakeDetailDto>.Ok(await _svc.GetStocktakeDetailAsync(TenantId, id, ct)));

    [HttpPost]
    [AuthorizePermission("inv.stocktake.manage")]
    public async Task<ActionResult<ApiResponse<InvStocktakeDto>>> Create(
        [FromBody] InvStocktakeCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvStocktakeDto>.Ok(await _svc.CreateStocktakeAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/count")]
    [AuthorizePermission("inv.stocktake.manage")]
    public async Task<ActionResult<ApiResponse<InvStocktakeLineDto>>> Count(
        Guid id, [FromBody] InvStocktakeCountRequest req, CancellationToken ct)
        => Ok(ApiResponse<InvStocktakeLineDto>.Ok(await _svc.CountStocktakeLineAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/review")]
    [AuthorizePermission("inv.stocktake.manage")]
    public async Task<ActionResult<ApiResponse<InvStocktakeDto>>> Review(Guid id, CancellationToken ct)
        => Ok(ApiResponse<InvStocktakeDto>.Ok(await _svc.ReviewStocktakeAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/post")]
    [AuthorizePermission("inv.stocktake.manage")]
    public async Task<ActionResult<ApiResponse<InvStocktakeDto>>> Post(Guid id, CancellationToken ct)
        => Ok(ApiResponse<InvStocktakeDto>.Ok(await _svc.PostStocktakeAsync(TenantId, UserId, id, ct)));
}
