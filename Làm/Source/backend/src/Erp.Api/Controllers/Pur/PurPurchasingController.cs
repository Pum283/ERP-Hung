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
[Route("api/pur/vendors")]
public sealed class PurVendorController : ControllerBase
{
    private readonly IPurPurchasingService _svc;
    public PurVendorController(IPurPurchasingService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("pur.vendor.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PurVendorDto>>>> List(
        [FromQuery] string? q, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PurVendorDto>>.Ok(await _svc.ListVendorsAsync(TenantId, q, ct)));

    [HttpPost]
    [AuthorizePermission("pur.vendor.manage")]
    public async Task<ActionResult<ApiResponse<PurVendorDto>>> Upsert(
        [FromBody] PurVendorUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurVendorDto>.Ok(await _svc.UpsertVendorAsync(TenantId, UserId, req, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("pur.vendor.read")]
    public async Task<ActionResult<ApiResponse<PurVendorDetailDto>>> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PurVendorDetailDto>.Ok(await _svc.GetVendorDetailAsync(TenantId, id, ct)));

    [HttpPost("{id:guid}/contacts")]
    [AuthorizePermission("pur.vendor.manage")]
    public async Task<ActionResult<ApiResponse<PurVendorContactDto>>> UpsertContact(
        Guid id, [FromBody] PurVendorContactUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurVendorContactDto>.Ok(await _svc.UpsertContactAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/products")]
    [AuthorizePermission("pur.vendor.manage")]
    public async Task<ActionResult<ApiResponse<PurVendorProductDto>>> UpsertProduct(
        Guid id, [FromBody] PurVendorProductUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurVendorProductDto>.Ok(await _svc.UpsertVendorProductAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/prices")]
    [AuthorizePermission("pur.vendor.manage")]
    public async Task<ActionResult<ApiResponse<PurVendorPriceDto>>> UpsertPrice(
        Guid id, [FromBody] PurVendorPriceUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurVendorPriceDto>.Ok(await _svc.UpsertVendorPriceAsync(TenantId, UserId, id, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/pur/prs")]
public sealed class PurPrController : ControllerBase
{
    private readonly IPurPurchasingService _svc;
    public PurPrController(IPurPurchasingService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("pur.pr.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PurPurchaseRequestDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PurPurchaseRequestDto>>.Ok(await _svc.ListPrsAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("pur.pr.manage")]
    public async Task<ActionResult<ApiResponse<PurPurchaseRequestDto>>> Upsert(
        [FromBody] PurPurchaseRequestUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurPurchaseRequestDto>.Ok(await _svc.UpsertPrAsync(TenantId, UserId, req, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("pur.pr.read")]
    public async Task<ActionResult<ApiResponse<PurPrDetailDto>>> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PurPrDetailDto>.Ok(await _svc.GetPrDetailAsync(TenantId, id, ct)));

    [HttpPost("{id:guid}/lines")]
    [AuthorizePermission("pur.pr.manage")]
    public async Task<ActionResult<ApiResponse<PurPrLineDto>>> UpsertLine(
        Guid id, [FromBody] PurPrLineUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurPrLineDto>.Ok(await _svc.UpsertPrLineAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/submit")]
    [AuthorizePermission("pur.pr.manage")]
    public async Task<ActionResult<ApiResponse<PurPurchaseRequestDto>>> Submit(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PurPurchaseRequestDto>.Ok(await _svc.SubmitPrAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/approve")]
    [AuthorizePermission("pur.pr.approve")]
    public async Task<ActionResult<ApiResponse<PurPurchaseRequestDto>>> Approve(
        Guid id, [FromBody] PurPrDecisionRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurPurchaseRequestDto>.Ok(await _svc.ApprovePrAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/reject")]
    [AuthorizePermission("pur.pr.approve")]
    public async Task<ActionResult<ApiResponse<PurPurchaseRequestDto>>> Reject(
        Guid id, [FromBody] PurPrDecisionRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurPurchaseRequestDto>.Ok(await _svc.RejectPrAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/return")]
    [AuthorizePermission("pur.pr.approve")]
    public async Task<ActionResult<ApiResponse<PurPurchaseRequestDto>>> Return(
        Guid id, [FromBody] PurPrDecisionRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurPurchaseRequestDto>.Ok(await _svc.ReturnPrAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/create-po")]
    [AuthorizePermission("pur.po.manage")]
    public async Task<ActionResult<ApiResponse<PurPurchaseOrderDto>>> CreatePo(
        Guid id, [FromBody] PurCreatePoFromPrRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurPurchaseOrderDto>.Ok(await _svc.CreatePoFromPrAsync(TenantId, UserId, id, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/pur/pos")]
public sealed class PurPoController : ControllerBase
{
    private readonly IPurPurchasingService _svc;
    public PurPoController(IPurPurchasingService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("pur.po.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PurPurchaseOrderDto>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PurPurchaseOrderDto>>.Ok(await _svc.ListPosAsync(TenantId, ct)));

    [HttpPost]
    [AuthorizePermission("pur.po.manage")]
    public async Task<ActionResult<ApiResponse<PurPurchaseOrderDto>>> Upsert(
        [FromBody] PurPurchaseOrderCreateRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurPurchaseOrderDto>.Ok(await _svc.UpsertPoAsync(TenantId, UserId, req, ct)));

    [HttpGet("{id:guid}")]
    [AuthorizePermission("pur.po.read")]
    public async Task<ActionResult<ApiResponse<PurPoDetailDto>>> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PurPoDetailDto>.Ok(await _svc.GetPoDetailAsync(TenantId, id, ct)));

    [HttpPost("{id:guid}/lines")]
    [AuthorizePermission("pur.po.manage")]
    public async Task<ActionResult<ApiResponse<PurPoLineDto>>> UpsertLine(
        Guid id, [FromBody] PurPoLineUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurPoLineDto>.Ok(await _svc.UpsertPoLineAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/submit")]
    [AuthorizePermission("pur.po.manage")]
    public async Task<ActionResult<ApiResponse<PurPurchaseOrderDto>>> Submit(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PurPurchaseOrderDto>.Ok(await _svc.SubmitPoAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/approve")]
    [AuthorizePermission("pur.po.approve")]
    public async Task<ActionResult<ApiResponse<PurPurchaseOrderDto>>> Approve(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PurPurchaseOrderDto>.Ok(await _svc.ApprovePoAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/send")]
    [AuthorizePermission("pur.po.manage")]
    public async Task<ActionResult<ApiResponse<PurPurchaseOrderDto>>> Send(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PurPurchaseOrderDto>.Ok(await _svc.SendPoAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/revise")]
    [AuthorizePermission("pur.po.manage")]
    public async Task<ActionResult<ApiResponse<PurPurchaseOrderDto>>> Revise(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PurPurchaseOrderDto>.Ok(await _svc.RevisePoAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/close")]
    [AuthorizePermission("pur.po.manage")]
    public async Task<ActionResult<ApiResponse<PurPurchaseOrderDto>>> Close(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PurPurchaseOrderDto>.Ok(await _svc.ClosePoAsync(TenantId, UserId, id, ct)));

    [HttpPost("{id:guid}/cancel")]
    [AuthorizePermission("pur.po.manage")]
    public async Task<ActionResult<ApiResponse<PurPurchaseOrderDto>>> Cancel(
        Guid id, [FromBody] PurPoCancelRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurPurchaseOrderDto>.Ok(await _svc.CancelPoAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/print")]
    [AuthorizePermission("pur.po.manage")]
    public async Task<ActionResult<ApiResponse<PurPurchaseOrderDto>>> Print(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PurPurchaseOrderDto>.Ok(await _svc.PrintPoAsync(TenantId, UserId, id, ct)));
}
