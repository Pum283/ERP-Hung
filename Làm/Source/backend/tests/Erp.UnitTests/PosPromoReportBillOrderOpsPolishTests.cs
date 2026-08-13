using Erp.Application.DTOs;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class PosPromoReportBillOrderOpsPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PosPromoReportBillOrderOpsService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _orderId1 = Guid.NewGuid();
    private readonly Guid _orderId2 = Guid.NewGuid();

    public PosPromoReportBillOrderOpsPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pos-promo-report-bill-order-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "TPOS181", Name = "Tenant POS 181" });
        _db.SaveChanges();

        _svc = new PosPromoReportBillOrderOpsService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_025: Báo cáo khuyến mại
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPromotionReportAnalytics_ReturnsUsageAnalytics()
    {
        var res = await _svc.GetPromotionReportAnalyticsAsync(_tenant);

        Assert.NotNull(res);
        Assert.True(res.TotalPromotionsApplied > 0);
        Assert.True(res.TotalDiscountGrantedVnd > 0);
        Assert.NotEmpty(res.UsageDetails);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_028: Tách bill / gộp bill
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SplitBill_SplitsItemsToNewOrder()
    {
        var req = new PosSplitBillRequest(
            _orderId1,
            new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
            "Tách bill theo yêu cầu khách hàng"
        );

        var res = await _svc.SplitBillAsync(_tenant, _userId, req);

        Assert.NotNull(res);
        Assert.Equal("Split", res.OperationType);
        Assert.Equal(2, res.TotalItemsAffected);
    }

    [Fact]
    public async Task MergeBill_MergesMultipleOrders()
    {
        var req = new PosMergeBillRequest(
            _orderId1,
            new List<Guid> { _orderId2 },
            "Gộp bill thanh toán chung"
        );

        var res = await _svc.MergeBillAsync(_tenant, _userId, req);

        Assert.NotNull(res);
        Assert.Equal("Merge", res.OperationType);
        Assert.Equal(1, res.TotalItemsAffected);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_029: Chuyển đơn giữa quầy
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task TransferOrderCounter_TransfersOrderToTargetCounter()
    {
        var req = new PosTransferOrderRequest(
            _orderId1,
            "POS01",
            "POS02",
            "Chuyển quầy phục vụ"
        );

        var res = await _svc.TransferOrderCounterAsync(_tenant, _userId, req);

        Assert.NotNull(res);
        Assert.Equal("POS01", res.FromCounterCode);
        Assert.Equal("POS02", res.ToCounterCode);
        Assert.Equal("Transferred", res.Status);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_030: Ghi chú đơn hàng & Bếp
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateOrderNotes_UpdatesCustomerAndKitchenNotes()
    {
        var req = new PosUpdateOrderNotesRequest(
            _orderId1,
            "Giao hàng trước 12h",
            "Ít đường, không cay, nhiều hành"
        );

        var res = await _svc.UpdateOrderNotesAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("Giao hàng trước 12h", res.CustomerNotes);
        Assert.Equal("Ít đường, không cay, nhiều hành", res.KitchenSpecialInstructions);
    }
}
