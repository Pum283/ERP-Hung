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
[Route("api/pos/promo-report-bill-order-ops")]
public sealed class PosPromoReportBillOrderOpsController : ControllerBase
{
    private readonly IPosPromoReportBillOrderOpsService _svc;

    public PosPromoReportBillOrderOpsController(IPosPromoReportBillOrderOpsService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());

    // UC_POS_025: Báo cáo khuyến mại
    [HttpGet("promotion-analytics")]
    [AuthorizePermission("pos.report.read")]
    public async Task<ActionResult<ApiResponse<PosPromotionReportAnalyticsDto>>> GetPromotionReportAnalytics(CancellationToken ct)
        => Ok(ApiResponse<PosPromotionReportAnalyticsDto>.Ok(await _svc.GetPromotionReportAnalyticsAsync(TenantId, ct)));

    // UC_POS_028: Tách bill / gộp bill
    [HttpPost("bills/split")]
    [AuthorizePermission("pos.checkout.write")]
    public async Task<ActionResult<ApiResponse<PosBillOperationResultDto>>> SplitBill([FromBody] PosSplitBillRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosBillOperationResultDto>.Ok(await _svc.SplitBillAsync(TenantId, UserId, req, ct)));

    [HttpPost("bills/merge")]
    [AuthorizePermission("pos.checkout.write")]
    public async Task<ActionResult<ApiResponse<PosBillOperationResultDto>>> MergeBill([FromBody] PosMergeBillRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosBillOperationResultDto>.Ok(await _svc.MergeBillAsync(TenantId, UserId, req, ct)));

    // UC_POS_029: Chuyển đơn giữa quầy
    [HttpPost("orders/transfer-counter")]
    [AuthorizePermission("pos.checkout.write")]
    public async Task<ActionResult<ApiResponse<PosOrderTransferResultDto>>> TransferOrderCounter([FromBody] PosTransferOrderRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosOrderTransferResultDto>.Ok(await _svc.TransferOrderCounterAsync(TenantId, UserId, req, ct)));

    // UC_POS_030: Ghi chú đơn hàng & Bếp
    [HttpPost("orders/notes")]
    [AuthorizePermission("pos.checkout.write")]
    public async Task<ActionResult<ApiResponse<PosOrderNotesDto>>> UpdateOrderNotes([FromBody] PosUpdateOrderNotesRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosOrderNotesDto>.Ok(await _svc.UpdateOrderNotesAsync(TenantId, req, ct)));
}
