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
[Route("api/pos/history-replenish-receive-audit")]
public sealed class PosHistoryReplenishReceiveAuditController : ControllerBase
{
    private readonly IPosHistoryReplenishReceiveAuditService _svc;

    public PosHistoryReplenishReceiveAuditController(IPosHistoryReplenishReceiveAuditService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());

    // UC_POS_053: Tra cứu lịch sử mua
    [HttpGet("customers/{customerId:guid}/purchase-history")]
    [AuthorizePermission("pos.customer.read")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PosCustomerPurchaseHistoryItemDto>>>> GetCustomerPurchaseHistory([FromRoute] Guid customerId, CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<PosCustomerPurchaseHistoryItemDto>>.Ok(await _svc.GetCustomerPurchaseHistoryAsync(TenantId, customerId, ct)));

    // UC_POS_056: Tạo đề nghị nhập hàng
    [HttpPost("replenishment-requests")]
    [AuthorizePermission("pos.inventory.write")]
    public async Task<ActionResult<ApiResponse<PosReplenishmentRequestDto>>> CreateReplenishmentRequest([FromBody] PosCreateReplenishmentRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosReplenishmentRequestDto>.Ok(await _svc.CreateReplenishmentRequestAsync(TenantId, UserId, req, ct)));

    // UC_POS_057: Nhận hàng từ kho trung tâm
    [HttpPost("central-transfers/receive")]
    [AuthorizePermission("pos.inventory.write")]
    public async Task<ActionResult<ApiResponse<PosReceiveTransferResultDto>>> ReceiveTransferShipment([FromBody] PosReceiveTransferShipmentRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosReceiveTransferResultDto>.Ok(await _svc.ReceiveTransferShipmentAsync(TenantId, UserId, req, ct)));

    // UC_POS_058: Kiểm kê nhanh
    [HttpPost("quick-audits")]
    [AuthorizePermission("pos.inventory.write")]
    public async Task<ActionResult<ApiResponse<PosQuickAuditResultDto>>> SubmitQuickAudit([FromBody] PosSubmitQuickAuditRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosQuickAuditResultDto>.Ok(await _svc.SubmitQuickAuditAsync(TenantId, UserId, req, ct)));
}
