using Erp.Application.DTOs;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class PosKitchenMixedPayCrossSellDrawerPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PosKitchenMixedPayCrossSellDrawerService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();
    private readonly Guid _shiftId = Guid.NewGuid();

    public PosKitchenMixedPayCrossSellDrawerPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pos-kitchen-mixed-pay-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "TPOS182", Name = "Tenant POS 182" });
        _db.SaveChanges();

        _svc = new PosKitchenMixedPayCrossSellDrawerService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_031: Gửi lệnh khu vực chế biến (KOT Ticket)
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DispatchKitchenTicket_DispatchesKotTicketSuccessfully()
    {
        var req = new PosDispatchKitchenTicketRequest(
            _orderId,
            "KITCHEN",
            new List<string> { "2x Cà Phê Sữa Đá (Ít đường)", "1x Bánh Mì Kẹp Thịt" }
        );

        var res = await _svc.DispatchKitchenTicketAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("KITCHEN", res.StationCode);
        Assert.Equal("Sent", res.Status);
        Assert.Equal(2, res.ItemSummaries.Count);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_036: Thanh toán hỗn hợp
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessMixedPayment_CalculatesSplitPaymentsCorrectly()
    {
        var payments = new List<PosPaymentSplitMethodDto>
        {
            new("Cash", 50000m),
            new("CreditCard", 100000m)
        };

        var req = new PosProcessMixedPaymentRequest(_orderId, 150000m, payments);
        var res = await _svc.ProcessMixedPaymentAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(150000m, res.TotalPaidVnd);
        Assert.Equal(0m, res.BalanceRemainingVnd);
        Assert.True(res.IsFullyPaid);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_041: Gợi ý bán kèm (Cross-sell / Upsell)
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCrossSellRecommendations_ReturnsProductRecommendations()
    {
        var list = await _svc.GetCrossSellRecommendationsAsync(_tenant, new List<Guid> { Guid.NewGuid() });

        Assert.NotNull(list);
        Assert.NotEmpty(list);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_044: Nộp tiền / rút tiền ca (Cash In / Cash Out)
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RecordCashInAndCashOut_RecordsDrawerTransactions()
    {
        var inReq = new PosCashInDrawerRequest(_shiftId, 500000m, "Bổ sung tiền lẻ đầu ca");
        var inRes = await _svc.RecordCashInAsync(_tenant, _userId, inReq);

        Assert.NotNull(inRes);
        Assert.Equal("CashIn", inRes.TransactionType);
        Assert.Equal(500000m, inRes.AmountVnd);

        var outReq = new PosCashOutDrawerRequest(_shiftId, 200000m, "Rút bớt tiền mặt cất két");
        var outRes = await _svc.RecordCashOutAsync(_tenant, _userId, outReq);

        Assert.NotNull(outRes);
        Assert.Equal("CashOut", outRes.TransactionType);
        Assert.Equal(200000m, outRes.AmountVnd);
    }
}
