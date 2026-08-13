using Erp.Application.DTOs;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class PosShiftApproveCustomerLoyaltyRedeemPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PosShiftApproveCustomerLoyaltyRedeemService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _managerId = Guid.NewGuid();
    private readonly Guid _shiftId = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();

    public PosShiftApproveCustomerLoyaltyRedeemPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pos-shift-approve-customer-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "TPOS183", Name = "Tenant POS 183" });
        _db.SaveChanges();

        _svc = new PosShiftApproveCustomerLoyaltyRedeemService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_049: Duyệt xác nhận ca
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ApproveShiftClosure_ApprovesShiftClosureSuccessfully()
    {
        var req = new PosApproveShiftRequest(_shiftId, true, 0m, "Khớp két 100%");
        var res = await _svc.ApproveShiftClosureAsync(_tenant, _managerId, req);

        Assert.NotNull(res);
        Assert.Equal("Approved", res.Status);
        Assert.Equal(0m, res.DiscrepancyVnd);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_050: Gắn khách hàng vào đơn
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AssignCustomerToOrder_AssignsCustomerToSale()
    {
        var req = new PosAssignCustomerToOrderRequest(_orderId, _customerId, "0909123456");
        var res = await _svc.AssignCustomerToOrderAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("0909123456", res.CustomerPhone);
        Assert.True(res.CurrentLoyaltyPoints >= 0);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_051: Tích điểm loyalty
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EarnLoyaltyPoints_CalculatesPointsEarnedForOrderTotal()
    {
        // Order 150.000đ => 15 điểm
        var req = new PosEarnLoyaltyPointsRequest(_customerId, _orderId, 150000m);
        var res = await _svc.EarnLoyaltyPointsAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(15, res.PointsEarned);
        Assert.True(res.NewTotalPoints > 15);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_052: Đổi điểm / ưu đãi
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RedeemLoyaltyPoints_RedeemsPointsForDiscount()
    {
        // Redeem 50 điểm => Giảm 50.000 VNĐ
        var req = new PosRedeemLoyaltyPointsRequest(_customerId, _orderId, 50);
        var res = await _svc.RedeemLoyaltyPointsAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(50, res.PointsRedeemed);
        Assert.Equal(50000m, res.DiscountValueVnd);
    }
}
