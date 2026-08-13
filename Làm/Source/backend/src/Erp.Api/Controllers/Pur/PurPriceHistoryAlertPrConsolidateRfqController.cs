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
[Route("api/pur/price-history-alert-pr-consolidate-rfq")]
public sealed class PurPriceHistoryAlertPrConsolidateRfqController : ControllerBase
{
    private readonly IPurPriceHistoryAlertPrConsolidateRfqService _svc;

    public PurPriceHistoryAlertPrConsolidateRfqController(IPurPriceHistoryAlertPrConsolidateRfqService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());

    // UC_PUR_012 & UC_PUR_013: Lịch sử giá mua & Cảnh báo tăng giá
    [HttpGet("price-history")]
    [AuthorizePermission("pur.price.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PurPriceHistoryItemDto>>>> GetPurchasePriceHistory([FromQuery] Guid? productId, [FromQuery] Guid? supplierId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PurPriceHistoryItemDto>>.Ok(await _svc.GetPurchasePriceHistoryAsync(TenantId, productId, supplierId, ct)));

    // UC_PUR_016: Gộp nhiều nhu cầu thành PR
    [HttpPost("purchase-requisitions/consolidate-demands")]
    [AuthorizePermission("pur.pr.write")]
    public async Task<ActionResult<ApiResponse<PurConsolidatedPrResultDto>>> ConsolidateDemandsToPr([FromBody] PurConsolidateDemandsToPrRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurConsolidatedPrResultDto>.Ok(await _svc.ConsolidateDemandsToPrAsync(TenantId, UserId, req, ct)));

    // UC_PUR_021: Tạo RFQ gửi nhiều nhà cung cấp
    [HttpPost("request-for-quotations/multi-supplier")]
    [AuthorizePermission("pur.rfq.write")]
    public async Task<ActionResult<ApiResponse<PurMultiSupplierRfqDto>>> CreateMultiSupplierRfq([FromBody] PurCreateMultiSupplierRfqRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurMultiSupplierRfqDto>.Ok(await _svc.CreateMultiSupplierRfqAsync(TenantId, UserId, req, ct)));
}
