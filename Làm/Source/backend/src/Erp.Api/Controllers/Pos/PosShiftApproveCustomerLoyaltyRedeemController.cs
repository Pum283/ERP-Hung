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
[Route("api/pos/shift-approve-customer-loyalty-redeem")]
public sealed class PosShiftApproveCustomerLoyaltyRedeemController : ControllerBase
{
    private readonly IPosShiftApproveCustomerLoyaltyRedeemService _svc;

    public PosShiftApproveCustomerLoyaltyRedeemController(IPosShiftApproveCustomerLoyaltyRedeemService svc)
    {
        _svc = svc;
    }

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);
    private Guid ManagerUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());

    // UC_POS_049: Duyệt xác nhận ca
    [HttpPost("shifts/approve")]
    [AuthorizePermission("pos.shift.approve")]
    public async Task<ActionResult<ApiResponse<PosShiftApprovalResultDto>>> ApproveShiftClosure([FromBody] PosApproveShiftRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosShiftApprovalResultDto>.Ok(await _svc.ApproveShiftClosureAsync(TenantId, ManagerUserId, req, ct)));

    // UC_POS_050: Gắn khách hàng vào đơn
    [HttpPost("orders/assign-customer")]
    [AuthorizePermission("pos.checkout.write")]
    public async Task<ActionResult<ApiResponse<PosOrderCustomerAssignedDto>>> AssignCustomerToOrder([FromBody] PosAssignCustomerToOrderRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosOrderCustomerAssignedDto>.Ok(await _svc.AssignCustomerToOrderAsync(TenantId, req, ct)));

    // UC_POS_051: Tích điểm loyalty
    [HttpPost("loyalty/earn")]
    [AuthorizePermission("pos.loyalty.write")]
    public async Task<ActionResult<ApiResponse<PosLoyaltyEarnResultDto>>> EarnLoyaltyPoints([FromBody] PosEarnLoyaltyPointsRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosLoyaltyEarnResultDto>.Ok(await _svc.EarnLoyaltyPointsAsync(TenantId, req, ct)));

    // UC_POS_052: Đổi điểm / ưu đãi
    [HttpPost("loyalty/redeem")]
    [AuthorizePermission("pos.loyalty.write")]
    public async Task<ActionResult<ApiResponse<PosLoyaltyRedeemResultDto>>> RedeemLoyaltyPoints([FromBody] PosRedeemLoyaltyPointsRequest req, CancellationToken ct)
        => Ok(ApiResponse<PosLoyaltyRedeemResultDto>.Ok(await _svc.RedeemLoyaltyPointsAsync(TenantId, req, ct)));
}
