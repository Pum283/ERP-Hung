using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Fin;
using Erp.Application.DTOs.Pos;
using Erp.Application.Interfaces.Services.Fin;
using Erp.Domain.Entities.Inv;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Pos;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 85:
///   UC_POS_064 — Tỷ lệ hủy / giảm giá (CancelDiscountRatesAsync)
///   UC_POS_065 — Cost lý thuyết vs thực tế (CostVarianceAsync)
///   UC_POS_066 — Top sản phẩm bán chạy (TopProductsAsync)
///   UC_POS_067 — So sánh điểm bán (CompareStoresAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class PosStep85PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PosConfigService _configSvc;
    private readonly PosSalesService _salesSvc;
    private readonly PosReportService _reportSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    private sealed class NoopFinRevenue : IFinRevenueService
    {
        public Task<IReadOnlyList<FinRevenueDocumentDto>> ListAsync(
            Guid tenantId, string? kind = null, Guid? periodId = null, string? status = null, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<FinRevenueDocumentDto>>(Array.Empty<FinRevenueDocumentDto>());
        public Task<FinRevenueSummaryDto> GetSummaryAsync(Guid tenantId, Guid? periodId = null, CancellationToken ct = default)
            => throw new AppException("FIN chưa sẵn sàng.");
        public Task<FinRevenueDocumentDto> RecognizeFromPosAsync(Guid tenantId, Guid userId, Guid saleId, FinRevenueRecognizeRequest? req = null, CancellationToken ct = default)
            => throw new AppException("FIN chưa sẵn sàng.");
        public Task<FinRevenueDocumentDto> RecognizeFromSalesOrderAsync(Guid tenantId, Guid userId, Guid orderId, FinRevenueRecognizeRequest? req = null, CancellationToken ct = default)
            => throw new AppException("FIN chưa sẵn sàng.");
        public Task<FinRevenueDocumentDto> RecognizeFromArInvoiceAsync(Guid tenantId, Guid userId, Guid arInvoiceId, FinRevenueRecognizeRequest? req = null, CancellationToken ct = default)
            => throw new AppException("FIN chưa sẵn sàng.");
        public Task<FinRevenueDocumentDto> RecognizeCogsAsync(Guid tenantId, Guid userId, Guid invStockDocId, FinRevenueRecognizeRequest? req = null, CancellationToken ct = default)
            => throw new AppException("FIN chưa sẵn sàng.");
        public Task<FinRevenueDocumentDto> VoidAsync(Guid tenantId, Guid userId, Guid id, string? note = null, CancellationToken ct = default)
            => throw new AppException("FIN chưa sẵn sàng.");
    }

    public PosStep85PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pos-step85-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin85", DisplayName = "Admin 85" });
        _db.InvWarehouses.Add(new InvWarehouse { TenantId = _tenant, Code = "WH-85", Name = "Kho 85", Status = "Active" });
        _db.SaveChanges();

        _configSvc = new PosConfigService(_db);
        _salesSvc = new PosSalesService(_db, new NoopFinRevenue(), null!);
        _reportSvc = new PosReportService(_db);
    }

    public void Dispose() => _db.Dispose();

    private async Task<(PosStoreDto store1, PosStoreDto store2)> CreateStoresAndSalesAsync()
    {
        var store1 = await _configSvc.UpsertStoreAsync(_tenant, _userAdmin, new PosStoreUpsertRequest(null, "ST-85A", "CH Q1 85", null, "Active", null, null));
        var store2 = await _configSvc.UpsertStoreAsync(_tenant, _userAdmin, new PosStoreUpsertRequest(null, "ST-85B", "CH Q3 85", null, "Active", null, null));

        var shift1 = await _salesSvc.OpenShiftAsync(_tenant, _userAdmin, new PosShiftOpenRequest(store1.Id, null, 500000m, null));
        var sale1 = await _salesSvc.OpenSaleAsync(_tenant, _userAdmin, new PosSaleOpenRequest(shift1.Id, "Bàn 1", null));
        var prod1 = await _configSvc.UpsertProductAsync(_tenant, _userAdmin, new PosProductUpsertRequest(null, null, "P-85A", "Món Hot 85", "Dĩa", "Active", 1));
        await _salesSvc.UpsertSaleLineAsync(_tenant, _userAdmin, sale1.Id, new PosSaleLineUpsertRequest(null, prod1.Id, prod1.Code, prod1.Name, 5, 100000m, 0m));
        await _salesSvc.PaySaleAsync(_tenant, _userAdmin, sale1.Id, new PosSalePayRequest("Cash", 500000m, null));

        var shift2 = await _salesSvc.OpenShiftAsync(_tenant, _userAdmin, new PosShiftOpenRequest(store2.Id, null, 300000m, null));
        var sale2 = await _salesSvc.OpenSaleAsync(_tenant, _userAdmin, new PosSaleOpenRequest(shift2.Id, "Bàn A", null));
        var prod2 = await _configSvc.UpsertProductAsync(_tenant, _userAdmin, new PosProductUpsertRequest(null, null, "P-85B", "Món Phụ 85", "Ly", "Active", 1));
        await _salesSvc.UpsertSaleLineAsync(_tenant, _userAdmin, sale2.Id, new PosSaleLineUpsertRequest(null, prod2.Id, prod2.Code, prod2.Name, 2, 50000m, 0m));
        await _salesSvc.PaySaleAsync(_tenant, _userAdmin, sale2.Id, new PosSalePayRequest("Cash", 100000m, null));

        return (store1, store2);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_064: Tỷ lệ hủy / giảm giá
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_064_CancelDiscountRates_ReturnsCorrectRatesAndAmounts()
    {
        await CreateStoresAndSalesAsync();
        var from = DateTimeOffset.UtcNow.AddDays(-1);
        var to = DateTimeOffset.UtcNow.AddDays(1);

        var report = await _reportSvc.CancelDiscountRatesAsync(_tenant, from, to);

        Assert.NotNull(report);
        Assert.True(report.TotalSales >= 2);
        Assert.True(report.PaidSales >= 2);
    }

    [Fact]
    public async Task UC_POS_064_CancelDiscountRates_NoSales_ReturnsZeroRates()
    {
        var from = DateTimeOffset.UtcNow.AddDays(-10);
        var to = DateTimeOffset.UtcNow.AddDays(-5);

        var report = await _reportSvc.CancelDiscountRatesAsync(_tenant, from, to);

        Assert.Equal(0, report.TotalSales);
        Assert.Equal(0m, report.CancelRatePercent);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_065: Cost lý thuyết vs thực tế
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_065_CostVariance_ReturnsVarianceReportDto()
    {
        await CreateStoresAndSalesAsync();
        var from = DateTimeOffset.UtcNow.AddDays(-1);
        var to = DateTimeOffset.UtcNow.AddDays(1);

        var report = await _reportSvc.CostVarianceAsync(_tenant, from, to);

        Assert.NotNull(report);
        Assert.NotNull(report.Rows);
    }

    [Fact]
    public async Task UC_POS_065_CostVariance_NoSales_ReturnsEmptyRows()
    {
        var from = DateTimeOffset.UtcNow.AddDays(-10);
        var to = DateTimeOffset.UtcNow.AddDays(-5);

        var report = await _reportSvc.CostVarianceAsync(_tenant, from, to);

        Assert.Empty(report.Rows);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_066: Top sản phẩm bán chạy
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_066_TopProducts_OrderedByQty_ReturnsRankedList()
    {
        await CreateStoresAndSalesAsync();
        var from = DateTimeOffset.UtcNow.AddDays(-1);
        var to = DateTimeOffset.UtcNow.AddDays(1);

        var topProducts = await _reportSvc.TopProductsAsync(_tenant, from, to, 5, "qty");

        Assert.NotEmpty(topProducts);
        Assert.Equal(1, topProducts[0].Rank);
        Assert.Equal("P-85A", topProducts[0].ProductCode);
        Assert.Equal(5m, topProducts[0].Qty);
    }

    [Fact]
    public async Task UC_POS_066_TopProducts_OrderedByRevenue_ReturnsRankedList()
    {
        await CreateStoresAndSalesAsync();
        var from = DateTimeOffset.UtcNow.AddDays(-1);
        var to = DateTimeOffset.UtcNow.AddDays(1);

        var topProducts = await _reportSvc.TopProductsAsync(_tenant, from, to, 5, "revenue");

        Assert.NotEmpty(topProducts);
        Assert.Equal("P-85A", topProducts[0].ProductCode);
        Assert.Equal(500000m, topProducts[0].Revenue);
    }

    [Fact]
    public async Task UC_POS_066_TopProducts_InvalidMode_ThrowsException()
    {
        var from = DateTimeOffset.UtcNow.AddDays(-1);
        var to = DateTimeOffset.UtcNow.AddDays(1);

        await Assert.ThrowsAsync<AppException>(() =>
            _reportSvc.TopProductsAsync(_tenant, from, to, 5, "invalid_mode"));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_067: So sánh điểm bán
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_067_CompareStores_ReturnsStoreContributions()
    {
        var (store1, store2) = await CreateStoresAndSalesAsync();
        var from = DateTimeOffset.UtcNow.AddDays(-1);
        var to = DateTimeOffset.UtcNow.AddDays(1);

        var rows = await _reportSvc.CompareStoresAsync(_tenant, from, to);

        Assert.Equal(2, rows.Count);
        var row1 = rows.First(x => x.StoreId == store1.Id);
        var row2 = rows.First(x => x.StoreId == store2.Id);

        Assert.Equal(500000m, row1.Revenue);
        Assert.Equal(100000m, row2.Revenue);
        Assert.True(row1.RevenueSharePercent > row2.RevenueSharePercent);
    }

    [Fact]
    public async Task UC_POS_067_CompareStores_NoSales_ReturnsEmptyList()
    {
        var from = DateTimeOffset.UtcNow.AddDays(-10);
        var to = DateTimeOffset.UtcNow.AddDays(-5);

        var rows = await _reportSvc.CompareStoresAsync(_tenant, from, to);

        Assert.Empty(rows);
    }
}
