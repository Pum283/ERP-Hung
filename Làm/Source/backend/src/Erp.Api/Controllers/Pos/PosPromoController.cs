using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Pos;
using Erp.Application.Interfaces.Services.Pos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Pos;

[ApiController]
[Authorize]
[Route("api/pos/promotions")]
public sealed class PosPromotionController : ControllerBase
{
    private readonly IPosPromoService _svc;
    public PosPromotionController(IPosPromoService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("pos.promo.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PosPromotionDto>>>> List(
        [FromQuery] string? q, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PosPromotionDto>>.Ok(await _svc.ListPromotionsAsync(TenantId, q, ct)));

    [HttpPost]
    [AuthorizePermission("pos.promo.manage")]
    public async Task<ActionResult<ApiResponse<PosPromotionDto>>> Upsert(
        [FromBody] PosPromotionUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosPromotionDto>.Ok(await _svc.UpsertPromotionAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/pos/vouchers")]
public sealed class PosVoucherController : ControllerBase
{
    private readonly IPosPromoService _svc;
    public PosVoucherController(IPosPromoService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    [AuthorizePermission("pos.promo.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PosVoucherDto>>>> List(
        [FromQuery] Guid? promotionId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PosVoucherDto>>.Ok(await _svc.ListVouchersAsync(TenantId, promotionId, ct)));

    [HttpPost]
    [AuthorizePermission("pos.promo.manage")]
    public async Task<ActionResult<ApiResponse<PosVoucherDto>>> Upsert(
        [FromBody] PosVoucherUpsertRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosVoucherDto>.Ok(await _svc.UpsertVoucherAsync(TenantId, UserId, req, ct)));
}

[ApiController]
[Authorize]
[Route("api/pos/sales")]
public sealed class PosSalePromoController : ControllerBase
{
    private readonly IPosPromoService _svc;
    public PosSalePromoController(IPosPromoService svc) => _svc = svc;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpPost("{id:guid}/promo/apply")]
    [AuthorizePermission("pos.sale.manage")]
    public async Task<ActionResult<ApiResponse<PosSaleDto>>> ApplyPromo(
        Guid id, [FromBody] PosApplyPromotionRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosSaleDto>.Ok(await _svc.ApplyPromotionAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/promo/voucher")]
    [AuthorizePermission("pos.sale.manage")]
    public async Task<ActionResult<ApiResponse<PosSaleDto>>> ApplyVoucher(
        Guid id, [FromBody] PosApplyVoucherRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosSaleDto>.Ok(await _svc.ApplyVoucherAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/promo/manual")]
    [AuthorizePermission("pos.sale.manage")]
    public async Task<ActionResult<ApiResponse<PosSaleDto>>> RequestManual(
        Guid id, [FromBody] PosManualDiscountRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosSaleDto>.Ok(await _svc.RequestManualDiscountAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/promo/decide")]
    [AuthorizePermission("pos.promo.manage")]
    public async Task<ActionResult<ApiResponse<PosSaleDto>>> Decide(
        Guid id, [FromBody] PosDecideDiscountRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosSaleDto>.Ok(await _svc.DecideManualDiscountAsync(TenantId, UserId, id, req, ct)));

    [HttpPost("{id:guid}/promo/clear")]
    [AuthorizePermission("pos.sale.manage")]
    public async Task<ActionResult<ApiResponse<PosSaleDto>>> Clear(Guid id, CancellationToken ct)
        => Ok(ApiResponse<PosSaleDto>.Ok(await _svc.ClearDiscountAsync(TenantId, UserId, id, ct)));
}
