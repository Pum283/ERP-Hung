using Erp.Application.DTOs;

namespace Erp.Application.Interfaces.Services;

public interface IPosShiftApproveCustomerLoyaltyRedeemService
{
    // UC_POS_049: Duyệt xác nhận ca
    Task<PosShiftApprovalResultDto> ApproveShiftClosureAsync(Guid tenantId, Guid managerUserId, PosApproveShiftRequest req, CancellationToken ct = default);

    // UC_POS_050: Gắn khách hàng vào đơn
    Task<PosOrderCustomerAssignedDto> AssignCustomerToOrderAsync(Guid tenantId, PosAssignCustomerToOrderRequest req, CancellationToken ct = default);

    // UC_POS_051: Tích điểm loyalty
    Task<PosLoyaltyEarnResultDto> EarnLoyaltyPointsAsync(Guid tenantId, PosEarnLoyaltyPointsRequest req, CancellationToken ct = default);

    // UC_POS_052: Đổi điểm / ưu đãi
    Task<PosLoyaltyRedeemResultDto> RedeemLoyaltyPointsAsync(Guid tenantId, PosRedeemLoyaltyPointsRequest req, CancellationToken ct = default);
}
