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
[Route("api/pur/quotation-compare-award-po-confirm")]
public sealed class PurQuotationCompareAwardPoConfirmController : ControllerBase
{
    private readonly IPurQuotationCompareAwardPoConfirmService _svc;

    public PurQuotationCompareAwardPoConfirmController(IPurQuotationCompareAwardPoConfirmService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());

    // UC_PUR_022: Nhập báo giá từ NCC
    [HttpPost("quotations")]
    [AuthorizePermission("pur.rfq.write")]
    public async Task<ActionResult<ApiResponse<PurVendorQuotationDto>>> SubmitVendorQuotation([FromBody] PurSubmitVendorQuotationRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurVendorQuotationDto>.Ok(await _svc.SubmitVendorQuotationAsync(TenantId, req, ct)));

    [HttpGet("rfq/{rfqId:guid}/quotations")]
    [AuthorizePermission("pur.rfq.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PurVendorQuotationDto>>>> GetQuotationsByRfq([FromRoute] Guid rfqId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PurVendorQuotationDto>>.Ok(await _svc.GetQuotationsByRfqAsync(TenantId, rfqId, ct)));

    // UC_PUR_023 & UC_PUR_024: So sánh & Chọn NCC thắng
    [HttpPost("quotations/award-winner")]
    [AuthorizePermission("pur.rfq.write")]
    public async Task<ActionResult<ApiResponse<PurAwardQuotationWinnerResultDto>>> AwardQuotationWinner([FromBody] PurAwardQuotationWinnerRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurAwardQuotationWinnerResultDto>.Ok(await _svc.AwardQuotationWinnerAsync(TenantId, UserId, req, ct)));

    // UC_PUR_029: Xác nhận PO từ NCC
    [HttpPost("purchase-orders/confirm")]
    [AuthorizePermission("pur.po.write")]
    public async Task<ActionResult<ApiResponse<PurVendorPoConfirmationDto>>> ConfirmVendorPo([FromBody] PurConfirmVendorPoRequest req, CancellationToken ct)
        => Ok(ApiResponse<PurVendorPoConfirmationDto>.Ok(await _svc.ConfirmVendorPoAsync(TenantId, req, ct)));
}
