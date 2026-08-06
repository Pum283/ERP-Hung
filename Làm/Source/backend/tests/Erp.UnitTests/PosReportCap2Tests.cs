using Erp.Application.Common.Exceptions;
using Erp.Domain.Entities.Inv;
using Erp.Domain.Entities.Pos;
using Erp.Infrastructure.Implementations.Services.Pos;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.UnitTests;

/// <summary>UC_POS_065/066/067 — top SP, so sánh điểm bán, cost variance — EF InMemory.</summary>
public sealed class PosReportCap2Tests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PosReportService _svc;
    private readonly Guid _tenant = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
    private readonly Guid _user = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
    private readonly DateTimeOffset _from = DateTimeOffset.UtcNow.AddDays(-1);
    private readonly DateTimeOffset _to = DateTimeOffset.UtcNow.AddDays(1);

    public PosReportCap2Tests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pos-report-cap2-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new PosReportService(_db);
    }

    public void Dispose() => _db.Dispose();

    private PosStore AddStore(string code)
    {
        var s = new PosStore { TenantId = _tenant, Code = code, Name = "Store " + code, Status = "Active" };
        _db.PosStores.Add(s);
        return s;
    }

    private PosSale AddPaidSale(Guid storeId, decimal total, decimal discount = 0)
    {
        var sale = new PosSale
        {
            TenantId = _tenant, CreatedBy = _user, Code = "S" + Guid.NewGuid().ToString("N")[..6],
            ShiftId = Guid.NewGuid(), StoreId = storeId, Status = "Paid",
            SubTotal = total, TotalAmount = total, DiscountAmount = discount,
            PaidAmount = total, PaidAt = DateTimeOffset.UtcNow,
        };
        _db.PosSales.Add(sale);
        return sale;
    }

    private void AddLine(Guid saleId, Guid? productId, string code, string name, decimal qty, decimal amount)
        => _db.PosSaleLines.Add(new PosSaleLine
        {
            TenantId = _tenant, CreatedBy = _user, SaleId = saleId,
            ProductId = productId, ProductCode = code, ProductName = name,
            Quantity = qty, UnitPrice = qty == 0 ? 0 : amount / qty, LineAmount = amount,
            Status = "Active", LineNo = 1,
        });

    // ── UC_POS_066 top products ──

    [Fact]
    public async Task TopProducts_RanksByQty()
    {
        var store = AddStore("ST1");
        var s = AddPaidSale(store.Id, 300_000);
        AddLine(s.Id, null, "CF", "Cà phê", 10, 200_000);
        AddLine(s.Id, null, "TR", "Trà", 3, 100_000);
        await _db.SaveChangesAsync();

        var rows = await _svc.TopProductsAsync(_tenant, _from, _to, 10, "qty");
        Assert.Equal(2, rows.Count);
        Assert.Equal("CF", rows[0].ProductCode);
        Assert.Equal(1, rows[0].Rank);
        Assert.Equal("TR", rows[1].ProductCode);
        Assert.Equal(2, rows[1].Rank);
    }

    [Fact]
    public async Task TopProducts_RanksByRevenue()
    {
        var store = AddStore("ST1");
        var s = AddPaidSale(store.Id, 300_000);
        AddLine(s.Id, null, "CF", "Cà phê", 10, 100_000);
        AddLine(s.Id, null, "BANH", "Bánh", 2, 200_000);
        await _db.SaveChangesAsync();

        var rows = await _svc.TopProductsAsync(_tenant, _from, _to, 10, "revenue");
        Assert.Equal("BANH", rows[0].ProductCode);
    }

    [Fact]
    public async Task TopProducts_LimitsTopN()
    {
        var store = AddStore("ST1");
        var s = AddPaidSale(store.Id, 100_000);
        for (var i = 0; i < 5; i++)
            AddLine(s.Id, null, $"P{i}", $"SP {i}", i + 1, 10_000);
        await _db.SaveChangesAsync();

        var rows = await _svc.TopProductsAsync(_tenant, _from, _to, 3, "qty");
        Assert.Equal(3, rows.Count);
    }

    [Fact]
    public async Task TopProducts_RejectsBadArgs()
    {
        await Assert.ThrowsAsync<AppException>(() => _svc.TopProductsAsync(_tenant, _from, _to, 10, "xxx"));
        await Assert.ThrowsAsync<AppException>(() => _svc.TopProductsAsync(_tenant, _from, _to, 0, "qty"));
    }

    // ── UC_POS_067 store compare ──

    [Fact]
    public async Task CompareStores_ComputesShareAndAvgTicket()
    {
        var a = AddStore("A");
        var b = AddStore("B");
        AddPaidSale(a.Id, 300_000);
        AddPaidSale(a.Id, 450_000);
        AddPaidSale(b.Id, 250_000);
        await _db.SaveChangesAsync();

        var rows = await _svc.CompareStoresAsync(_tenant, _from, _to);
        Assert.Equal(2, rows.Count);
        Assert.Equal("A", rows[0].StoreCode);
        Assert.Equal(750_000, rows[0].Revenue);
        Assert.Equal(375_000, rows[0].AvgTicket);
        Assert.Equal(75, rows[0].RevenueSharePercent);
        Assert.Equal(25, rows[1].RevenueSharePercent);
    }

    [Fact]
    public async Task CompareStores_EmptyWhenNoSales()
    {
        AddStore("A");
        await _db.SaveChangesAsync();
        var rows = await _svc.CompareStoresAsync(_tenant, _from, _to);
        Assert.Empty(rows);
    }

    // ── UC_POS_065 cost variance ──

    private async Task<(PosStore store, PosProduct product, InvSku sku)> SeedBomAsync(decimal stdCost)
    {
        var store = AddStore("ST1");
        var product = new PosProduct
        {
            TenantId = _tenant, Code = "CF", Name = "Cà phê", Status = "Active",
        };
        _db.PosProducts.Add(product);
        var sku = new InvSku
        {
            TenantId = _tenant, Code = "NVL-CF", Name = "Hạt cà phê", Status = "Active",
            StandardCost = stdCost,
        };
        _db.InvSkus.Add(sku);
        _db.PosBomLines.Add(new PosBomLine
        {
            TenantId = _tenant, CreatedBy = _user, ProductId = product.Id,
            MaterialCode = "NVL-CF", MaterialName = "Hạt cà phê", Qty = 0.02m, Unit = "kg",
        });
        await _db.SaveChangesAsync();
        return (store, product, sku);
    }

    [Fact]
    public async Task CostVariance_TheoreticalFromBom()
    {
        var (store, product, _) = await SeedBomAsync(500_000);
        var s = AddPaidSale(store.Id, 300_000);
        AddLine(s.Id, product.Id, "CF", "Cà phê", 10, 300_000);
        await _db.SaveChangesAsync();

        var r = await _svc.CostVarianceAsync(_tenant, _from, _to);
        Assert.Single(r.Rows);
        var row = r.Rows[0];
        Assert.Equal("NVL-CF", row.MaterialCode);
        Assert.Equal(0.2m, row.TheoreticalQty);       // 0.02 × 10
        Assert.Equal(100_000, row.TheoreticalCost);   // 0.2 × 500k
        Assert.Equal(0, row.ActualQty);               // chưa có phiếu xuất
    }

    [Fact]
    public async Task CostVariance_ActualFromInvIssue()
    {
        var (store, product, sku) = await SeedBomAsync(500_000);
        var s = AddPaidSale(store.Id, 300_000);
        AddLine(s.Id, product.Id, "CF", "Cà phê", 10, 300_000);
        var doc = new InvStockDoc
        {
            TenantId = _tenant, Code = "ISS-1", DocType = "Issue", SourceType = "Sales",
            Status = "Posted", RefModule = "POS", RefId = s.Id, RefCode = s.Code,
            WarehouseId = Guid.NewGuid(),
        };
        _db.InvStockDocs.Add(doc);
        _db.InvStockDocLines.Add(new InvStockDocLine
        {
            TenantId = _tenant, DocId = doc.Id, SkuId = sku.Id,
            SkuCode = "NVL-CF", SkuName = "Hạt cà phê", Qty = 0.25m, UnitCost = 520_000,
        });
        await _db.SaveChangesAsync();

        var r = await _svc.CostVarianceAsync(_tenant, _from, _to);
        var row = r.Rows[0];
        Assert.Equal(0.25m, row.ActualQty);
        Assert.Equal(130_000, row.ActualCost);        // 0.25 × 520k
        Assert.Equal(30_000, row.VarianceCost);       // 130k − 100k
        Assert.Equal(30, row.VariancePercent);
        Assert.Equal(30_000, r.TotalVarianceCost);
    }

    [Fact]
    public async Task CostVariance_FallbackStandardCostWhenUnitCostZero()
    {
        var (store, product, sku) = await SeedBomAsync(500_000);
        var s = AddPaidSale(store.Id, 300_000);
        AddLine(s.Id, product.Id, "CF", "Cà phê", 10, 300_000);
        var doc = new InvStockDoc
        {
            TenantId = _tenant, Code = "ISS-2", DocType = "Issue", SourceType = "Sales",
            Status = "Posted", RefModule = "POS", RefId = s.Id, RefCode = s.Code,
            WarehouseId = Guid.NewGuid(),
        };
        _db.InvStockDocs.Add(doc);
        _db.InvStockDocLines.Add(new InvStockDocLine
        {
            TenantId = _tenant, DocId = doc.Id, SkuId = sku.Id,
            SkuCode = "NVL-CF", SkuName = "Hạt cà phê", Qty = 0.2m, UnitCost = 0,
        });
        await _db.SaveChangesAsync();

        var r = await _svc.CostVarianceAsync(_tenant, _from, _to);
        var row = r.Rows[0];
        Assert.Equal(100_000, row.ActualCost);        // 0.2 × 500k std
        Assert.Equal(0, row.VarianceCost);
    }

    [Fact]
    public async Task CostVariance_EmptyWhenNoData()
    {
        var r = await _svc.CostVarianceAsync(_tenant, _from, _to);
        Assert.Empty(r.Rows);
        Assert.Equal(0, r.TotalTheoreticalCost);
    }

    // ── UC_POS_069/072 chain live + target ──

    [Fact]
    public async Task ChainLive_ComputesTodayMonthAndAttainment()
    {
        var a = AddStore("A");
        a.MonthlyRevenueTarget = 1_000_000;
        var b = AddStore("B");
        AddPaidSale(a.Id, 300_000);
        AddPaidSale(a.Id, 200_000);
        AddPaidSale(b.Id, 100_000);
        _db.PosShifts.Add(new PosShift
        {
            TenantId = _tenant, CreatedBy = _user, Code = "CA1",
            StoreId = a.Id, CashierUserId = _user, Status = "Open",
        });
        await _db.SaveChangesAsync();

        var r = await _svc.ChainLiveAsync(_tenant);
        Assert.Equal(2, r.StoreCount);
        Assert.Equal(1, r.OpenShiftCount);
        Assert.Equal(600_000, r.TotalTodayRevenue);
        Assert.Equal(600_000, r.TotalMonthRevenue);

        var rowA = r.Rows.Single(x => x.StoreCode == "A");
        Assert.Equal(1, rowA.OpenShiftCount);
        Assert.Equal(2, rowA.TodaySaleCount);
        Assert.Equal(500_000, rowA.TodayRevenue);
        Assert.Equal(50, rowA.TargetAttainmentPercent);   // 500k / 1tr

        var rowB = r.Rows.Single(x => x.StoreCode == "B");
        Assert.Equal(0, rowB.TargetAttainmentPercent);    // chưa đặt target
        Assert.Equal(0, rowB.MonthlyTarget);
    }

    [Fact]
    public async Task ChainLive_SkipsInactiveStores()
    {
        var a = AddStore("A");
        a.Status = "Inactive";
        await _db.SaveChangesAsync();

        var r = await _svc.ChainLiveAsync(_tenant);
        Assert.Equal(0, r.StoreCount);
        Assert.Empty(r.Rows);
    }

    [Fact]
    public async Task ChainLive_ElapsedPercentWithinRange()
    {
        AddStore("A");
        await _db.SaveChangesAsync();
        var r = await _svc.ChainLiveAsync(_tenant);
        var row = r.Rows.Single();
        Assert.InRange(row.MonthElapsedPercent, 0, 100);
    }

    [Fact]
    public async Task UpsertStore_SetsAndValidatesTarget()
    {
        var cfg = new PosConfigService(_db);
        var created = await cfg.UpsertStoreAsync(_tenant, _user,
            new Erp.Application.DTOs.Pos.PosStoreUpsertRequest(
                null, "T1", "Target store", null, null, null, 5_000_000));
        Assert.Equal(5_000_000, created.MonthlyRevenueTarget);

        // null = giữ nguyên target cũ
        var kept = await cfg.UpsertStoreAsync(_tenant, _user,
            new Erp.Application.DTOs.Pos.PosStoreUpsertRequest(
                created.Id, "T1", "Target store", null, null, null, null));
        Assert.Equal(5_000_000, kept.MonthlyRevenueTarget);

        await Assert.ThrowsAsync<AppException>(() => cfg.UpsertStoreAsync(_tenant, _user,
            new Erp.Application.DTOs.Pos.PosStoreUpsertRequest(
                created.Id, "T1", "Target store", null, null, null, -1)));
    }

    // ── CSV export cho 3 report mới ──

    [Fact]
    public async Task ExportCsv_SupportsNewKinds()
    {
        var store = AddStore("ST1");
        var s = AddPaidSale(store.Id, 100_000);
        AddLine(s.Id, null, "CF", "Cà phê", 2, 100_000);
        await _db.SaveChangesAsync();

        var top = await _svc.ExportCsvAsync(_tenant, "top-products", _from, _to);
        Assert.Contains("Rank,ProductCode", top);
        Assert.Contains("CF", top);

        var stores = await _svc.ExportCsvAsync(_tenant, "stores", _from, _to);
        Assert.Contains("StoreCode,StoreName", stores);
        Assert.Contains("ST1", stores);

        var cost = await _svc.ExportCsvAsync(_tenant, "cost-variance", _from, _to);
        Assert.Contains("MaterialCode", cost);
        Assert.Contains("TOTAL", cost);
    }
}
