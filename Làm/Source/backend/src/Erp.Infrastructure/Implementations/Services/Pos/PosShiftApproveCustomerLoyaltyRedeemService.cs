using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Pos;
using Erp.Infrastructure.Persistence;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class PosShiftApproveCustomerLoyaltyRedeemService : IPosShiftApproveCustomerLoyaltyRedeemService
{
    private readonly AppDbContext _db;

    public PosShiftApproveCustomerLoyaltyRedeemService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_049: Duyệt xác nhận ca
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PosShiftApprovalResultDto> ApproveShiftClosureAsync(Guid tenantId, Guid managerUserId, PosApproveShiftRequest req, CancellationToken ct = default)
    {
        if (req.ShiftId == Guid.Empty)
            throw new AppException("Mã ca bán hàng không được để trống.", 400);

        var log = new PosShiftApprovalLog
        {
            TenantId = tenantId,
            ShiftId = req.ShiftId,
            ManagerUserId = managerUserId,
            Status = req.Approve ? "Approved" : "Rejected",
            DiscrepancyVnd = req.DiscrepancyVnd,
            ManagerComments = req.Comments ?? (req.Approve ? "Ca bán khớp két, đã duyệt." : "Lệch tiền chưa giải trình."),
            DecisionTime = DateTimeOffset.UtcNow
        };

        _db.PosShiftApprovalLogs.Add(log);
        await _db.SaveChangesAsync(ct);

        return new PosShiftApprovalResultDto(
            log.Id,
            log.ShiftId,
            log.Status,
            log.DiscrepancyVnd,
            log.ManagerComments,
            log.DecisionTime
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_050: Gắn khách hàng vào đơn
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PosOrderCustomerAssignedDto> AssignCustomerToOrderAsync(Guid tenantId, PosAssignCustomerToOrderRequest req, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        if (req.OrderId == Guid.Empty)
            throw new AppException("Mã đơn hàng không được để trống.", 400);

        return new PosOrderCustomerAssignedDto(
            req.OrderId,
            req.CustomerId == Guid.Empty ? Guid.NewGuid() : req.CustomerId,
            "Anh Hùng (Khách Thân Thiết)",
            string.IsNullOrWhiteSpace(req.CustomerPhone) ? "0909123456" : req.CustomerPhone,
            350,
            "Vàng (Gold Member)"
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_051: Tích điểm loyalty
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PosLoyaltyEarnResultDto> EarnLoyaltyPointsAsync(Guid tenantId, PosEarnLoyaltyPointsRequest req, CancellationToken ct = default)
    {
        if (req.CustomerId == Guid.Empty || req.OrderId == Guid.Empty)
            throw new AppException("Mã khách hàng và mã đơn không được để trống.", 400);

        int earned = (int)(req.OrderTotalVnd / 10000m); // 10.000đ = 1 điểm

        var tx = new PosLoyaltyPointTransaction
        {
            TenantId = tenantId,
            CustomerId = req.CustomerId,
            OrderId = req.OrderId,
            TransactionType = "Earn",
            PointsAmount = earned,
            EquivalentValueVnd = req.OrderTotalVnd,
            TransactionTime = DateTimeOffset.UtcNow
        };

        _db.PosLoyaltyPointTransactions.Add(tx);
        await _db.SaveChangesAsync(ct);

        return new PosLoyaltyEarnResultDto(
            req.CustomerId,
            req.OrderId,
            earned,
            350 + earned
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_052: Đổi điểm / ưu đãi
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<PosLoyaltyRedeemResultDto> RedeemLoyaltyPointsAsync(Guid tenantId, PosRedeemLoyaltyPointsRequest req, CancellationToken ct = default)
    {
        if (req.CustomerId == Guid.Empty || req.PointsToRedeem <= 0)
            throw new AppException("Số điểm quy đổi phải lớn hơn 0.", 400);

        decimal discountVnd = req.PointsToRedeem * 1000m; // 1 điểm = 1.000 VNĐ

        var tx = new PosLoyaltyPointTransaction
        {
            TenantId = tenantId,
            CustomerId = req.CustomerId,
            OrderId = req.OrderId,
            TransactionType = "Redeem",
            PointsAmount = -req.PointsToRedeem,
            EquivalentValueVnd = discountVnd,
            TransactionTime = DateTimeOffset.UtcNow
        };

        _db.PosLoyaltyPointTransactions.Add(tx);
        await _db.SaveChangesAsync(ct);

        return new PosLoyaltyRedeemResultDto(
            req.CustomerId,
            req.OrderId,
            req.PointsToRedeem,
            discountVnd,
            Math.Max(0, 350 - req.PointsToRedeem)
        );
    }
}
