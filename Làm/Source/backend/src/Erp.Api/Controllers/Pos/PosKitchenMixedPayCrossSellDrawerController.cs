using System.Security.Claims;
using Erp.Api.Filters;
using Erp.Application.Common.Models;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/pos/kitchen-mixed-pay-cross-sell-drawer")]
public sealed class PosKitchenMixedPayCrossSellDrawerController : ControllerBase
{
    private readonly IPosKitchenMixedPayCrossSellDrawerService _svc;

    public PosKitchenMixedPayCrossSellDrawerController(IPosKitchenMixedPayCrossSellDrawerService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());

    // UC_POS_031: Gửi lệnh khu vực chế biến (KOT Ticket)
    [HttpPost("kitchen-tickets/dispatch")]
    [AuthorizePermission("pos.checkout.write")]
    public async Task<ActionResult<ApiResponse<PosKitchenOrderTicketDto>>> DispatchKitchenTicket([FromBody] PosDispatchKitchenTicketRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosKitchenOrderTicketDto>.Ok(await _svc.DispatchKitchenTicketAsync(TenantId, req, ct)));

    // UC_POS_036: Thanh toán hỗn hợp
    [HttpPost("payments/mixed")]
    [AuthorizePermission("pos.checkout.write")]
    public async Task<ActionResult<ApiResponse<PosMixedPaymentResultDto>>> ProcessMixedPayment([FromBody] PosProcessMixedPaymentRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosMixedPaymentResultDto>.Ok(await _svc.ProcessMixedPaymentAsync(TenantId, req, ct)));

    // UC_POS_041: Gợi ý bán kèm (Cross-sell / Upsell)
    [HttpPost("recommendations/cross-sell")]
    [AuthorizePermission("pos.cart.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PosCrossSellRecommendationDto>>>> GetCrossSellRecommendations([FromBody] IReadOnlyList<Guid> currentCartProductIds, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PosCrossSellRecommendationDto>>.Ok(await _svc.GetCrossSellRecommendationsAsync(TenantId, currentCartProductIds, ct)));

    // UC_POS_044: Nộp tiền / rút tiền ca (Cash In / Cash Out)
    [HttpPost("cash-drawer/cash-in")]
    [AuthorizePermission("pos.shift.write")]
    public async Task<ActionResult<ApiResponse<PosShiftCashTransactionDto>>> RecordCashIn([FromBody] PosCashInDrawerRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosShiftCashTransactionDto>.Ok(await _svc.RecordCashInAsync(TenantId, UserId, req, ct)));

    [HttpPost("cash-drawer/cash-out")]
    [AuthorizePermission("pos.shift.write")]
    public async Task<ActionResult<ApiResponse<PosShiftCashTransactionDto>>> RecordCashOut([FromBody] PosCashOutDrawerRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosShiftCashTransactionDto>.Ok(await _svc.RecordCashOutAsync(TenantId, UserId, req, ct)));
}
