using Erp.Application.DTOs;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class PurPriceHistoryAlertPrConsolidateRfqPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PurPriceHistoryAlertPrConsolidateRfqService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _supplierId1 = Guid.NewGuid();
    private readonly Guid _supplierId2 = Guid.NewGuid();

    public PurPriceHistoryAlertPrConsolidateRfqPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pur-price-history-rfq-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "TPUR187", Name = "Tenant PUR 187" });
        _db.SaveChanges();

        _svc = new PurPriceHistoryAlertPrConsolidateRfqService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_012 & UC_PUR_013: Lịch sử giá mua & Cảnh báo tăng giá
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPurchasePriceHistory_ReturnsHistoryAndAbnormalSpikeAlert()
    {
        var list = await _svc.GetPurchasePriceHistoryAsync(_tenant, null, null);

        Assert.NotNull(list);
        Assert.NotEmpty(list);
        Assert.Contains(list, x => x.IsAbnormalSpike);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_016: Gộp nhiều nhu cầu thành PR
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ConsolidateDemandsToPr_ConsolidatesMultipleDepartmentDemands()
    {
        var demandLines = new List<PurDemandLineDto>
        {
            new(Guid.NewGuid(), "Phòng Marketing", Guid.NewGuid(), "SKU-PAPER", "Giấy In A4 70gsm", 10),
            new(Guid.NewGuid(), "Phòng Kế Toán", Guid.NewGuid(), "SKU-PAPER", "Giấy In A4 70gsm", 15)
        };

        var req = new PurConsolidateDemandsToPrRequest("PR Gộp Nhu Cầu Văn Phòng Phẩm Tháng 8", demandLines);
        var res = await _svc.ConsolidateDemandsToPrAsync(_tenant, _userId, req);

        Assert.NotNull(res);
        Assert.Equal(25, res.TotalQuantity);
        Assert.StartsWith("PR-CONS-", res.PrNumber);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_021: Tạo RFQ gửi nhiều nhà cung cấp
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateMultiSupplierRfq_SendsRfqToMultipleVendors()
    {
        var suppliers = new List<Guid> { _supplierId1, _supplierId2 };
        var items = new List<PurRfqItemDto>
        {
            new(Guid.NewGuid(), "SKU-BEANS", "Cà Phê Hạt Arabica 1KG", 100)
        };

        var req = new PurCreateMultiSupplierRfqRequest(
            "Yêu Cầu Báo Giá Cà Phê Hạt Quý 3/2026",
            suppliers,
            items,
            DateTimeOffset.UtcNow.AddDays(7)
        );

        var res = await _svc.CreateMultiSupplierRfqAsync(_tenant, _userId, req);

        Assert.NotNull(res);
        Assert.Equal(2, res.TotalSuppliersCount);
        Assert.Equal(1, res.TotalItemsCount);
        Assert.Equal("Sent", res.Status);
    }
}
