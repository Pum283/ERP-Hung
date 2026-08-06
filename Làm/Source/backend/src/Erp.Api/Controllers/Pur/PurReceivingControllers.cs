using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Pur;
using Erp.Application.Interfaces.Services.Pur;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Pur;

[ApiController]
[Authorize]
[Route("api/pur/grns")]
public sealed class PurGrnController : ControllerBase
{
    private readonly IPurReceivingService _svc;
    public PurGrnController(IPurReceivingService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("pur.grn.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PurGrnDto>>>> List(
        [FromQuery] Guid? poId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PurGrnDto>>.Ok(await _svc.ListGrnsAsync(TenantId, poId, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("pur.grn.read")]
    public async Task<ActionResult<ApiResponse<PurGrnDetailDto>>> Detail(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PurGrnDetailDto>.Ok(await _svc.GetGrnDetailAsync(TenantId, id, ct)));

    [HttpPost]
    [AuthorizePermission("pur.grn.manage")]
    public async Task<ActionResult<ApiResponse<PurGrnDto>>> Create(
        [FromBody] PurGrnCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurGrnDto>.Ok(await _svc.CreateGrnFromPoAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/lines")]
    [AuthorizePermission("pur.grn.manage")]
    public async Task<ActionResult<ApiResponse<PurGrnLineDto>>> UpdateLine(
        Guid id, [FromBody] PurGrnLineUpdateRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurGrnLineDto>.Ok(await _svc.UpdateGrnLineAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/post")]
    [AuthorizePermission("pur.grn.manage")]
    public async Task<ActionResult<ApiResponse<PurGrnDto>>> Post(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PurGrnDto>.Ok(await _svc.PostGrnAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/push-inventory")]
    [AuthorizePermission("pur.grn.manage")]
    public async Task<ActionResult<ApiResponse<PurGrnDto>>> PushInventory(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PurGrnDto>.Ok(await _svc.PushGrnToInventoryAsync(TenantId, UserId, id, ct)));
}

[ApiController]
[Authorize]
[Route("api/pur/invoices")]
public sealed class PurInvoiceController : ControllerBase
{
    private readonly IPurReceivingService _svc;
    public PurInvoiceController(IPurReceivingService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("pur.invoice.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PurInvoiceDto>>>> List(
        [FromQuery] Guid? vendorId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PurInvoiceDto>>.Ok(await _svc.ListInvoicesAsync(TenantId, vendorId, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("pur.invoice.read")]
    public async Task<ActionResult<ApiResponse<PurInvoiceDetailDto>>> Detail(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PurInvoiceDetailDto>.Ok(await _svc.GetInvoiceDetailAsync(TenantId, id, ct)));

    [HttpPost]
    [AuthorizePermission("pur.invoice.manage")]
    public async Task<ActionResult<ApiResponse<PurInvoiceDto>>> Create(
        [FromBody] PurInvoiceCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurInvoiceDto>.Ok(await _svc.CreateInvoiceAsync(TenantId, UserId, req, ct)));

    [HttpPost("{id:guid}/lines")]
    [AuthorizePermission("pur.invoice.manage")]
    public async Task<ActionResult<ApiResponse<PurInvoiceLineDto>>> UpsertLine(
        Guid id, [FromBody] PurInvoiceLineUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurInvoiceLineDto>.Ok(await _svc.UpsertInvoiceLineAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/match")]
    [AuthorizePermission("pur.invoice.manage")]
    public async Task<ActionResult<ApiResponse<PurInvoiceDto>>> Match(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PurInvoiceDto>.Ok(await _svc.MatchThreeWayAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/push-ap")]
    [AuthorizePermission("pur.invoice.manage")]
    public async Task<ActionResult<ApiResponse<PurInvoiceDto>>> PushAp(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PurInvoiceDto>.Ok(await _svc.PushInvoiceToApAsync(TenantId, UserId, id, ct)));
}
