namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_POS_049: Duyệt xác nhận ca (Shift Closure Approval)
// ────────────────────────────────────────────────────────────────────────────

public record PosApproveShiftRequest(
    Guid ShiftId,
    bool Approve,
    decimal DiscrepancyVnd,
    string Comments
);

public record PosShiftApprovalResultDto(
    Guid ApprovalId,
    Guid ShiftId,
    string Status, // Approved | Rejected
    decimal DiscrepancyVnd,
    string ManagerComments,
    DateTimeOffset DecisionTime
);

// ────────────────────────────────────────────────────────────────────────────
// UC_POS_050: Gắn khách hàng vào đơn
// ────────────────────────────────────────────────────────────────────────────

public record PosAssignCustomerToOrderRequest(
    Guid OrderId,
    Guid CustomerId,
    string CustomerPhone
);

public record PosOrderCustomerAssignedDto(
    Guid OrderId,
    Guid CustomerId,
    string CustomerName,
    string CustomerPhone,
    int CurrentLoyaltyPoints,
    string MemberTier
);

// ────────────────────────────────────────────────────────────────────────────
// UC_POS_051: Tích điểm loyalty
// ────────────────────────────────────────────────────────────────────────────

public record PosEarnLoyaltyPointsRequest(
    Guid CustomerId,
    Guid OrderId,
    decimal OrderTotalVnd
);

public record PosLoyaltyEarnResultDto(
    Guid CustomerId,
    Guid OrderId,
    int PointsEarned,
    int NewTotalPoints
);

// ────────────────────────────────────────────────────────────────────────────
// UC_POS_052: Đổi điểm / ưu đãi
// ────────────────────────────────────────────────────────────────────────────

public record PosRedeemLoyaltyPointsRequest(
    Guid CustomerId,
    Guid OrderId,
    int PointsToRedeem
);

public record PosLoyaltyRedeemResultDto(
    Guid CustomerId,
    Guid OrderId,
    int PointsRedeemed,
    decimal DiscountValueVnd,
    int RemainingPoints
);
