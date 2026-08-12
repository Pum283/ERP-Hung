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
/// Unit tests cho Bước 84:
///   UC_POS_059 — Đồng bộ doanh thu ca sang FIN (SyncShiftRevenueToFinAsync)
///   UC_POS_061 — Doanh thu theo giờ / ngày / ca (RevenueByTimeAsync)
///   UC_POS_062 — Doanh thu theo sản phẩm (RevenueByProductAsync)
///   UC_POS_063 — Doanh thu theo thu ngân (RevenueByCashierAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class PosStep84PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PosConfigService _configSvc;
    private readonly PosSalesService _salesSvc;
    private readonly PosReportService _reportSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    private sealed class MockFinRevenue : IFinRevenueService
    {
        public int RecognizedCount { get; private set; }
        public Task<IReadOnlyList<FinRevenueDocumentDto>> ListAsync(
            Guid tenantId, string? kind = null, Guid? periodId = null, string? status = null, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<FinRevenueDocumentDto>>(Array.Empty<FinRevenueDocumentDto>());
        public Task<FinRevenueSummaryDto> GetSummaryAsync(Guid tenantId, Guid? periodId = null, CancellationToken ct = default)
            => throw new AppException("FIN chưa sẵn sàng.");
        public Task<FinRevenueDocumentDto> RecognizeFromPosAsync(Guid tenantId, Guid userId, Guid saleId, FinRevenueRecognizeRequest? req = null, CancellationToken ct = default)
        {
            RecognizedCount++;
            return Task.FromResult<FinRevenueDocumentDto>(null!);
        }
        public Task<FinRevenueDocumentDto> RecognizeFromSalesOrderAsync(Guid tenantId, Guid userId, Guid orderId, FinRevenueRecognizeRequest? req = null, CancellationToken ct = default)
            => throw new AppException("FIN chưa sẵn sàng.");
        public Task<FinRevenueDocumentDto> RecognizeFromArInvoiceAsync(Guid tenantId, Guid userId, Guid arInvoiceId, FinRevenueRecognizeRequest? req = null, CancellationToken ct = default)
            => throw new AppException("FIN chưa sẵn sàng.");
        public Task<FinRevenueDocumentDto> RecognizeCogsAsync(Guid tenantId, Guid userId, Guid invStockDocId, FinRevenueRecognizeRequest? req = null, CancellationToken ct = default)
            => throw new AppException("FIN chưa sẵn sàng.");
        public Task<FinRevenueDocumentDto> VoidAsync(Guid tenantId, Guid userId, Guid id, string? note = null, CancellationToken ct = default)
            => throw new AppException("FIN chưa sẵn sàng.");
    }

    public PosStep84PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pos-step84-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin84", DisplayName = "Admin 84" });
        _db.InvWarehouses.Add(new InvWarehouse { TenantId = _tenant, Code = "WH-84", Name = "Kho 84", Status = "Active" });
        _db.SaveChanges();

        _configSvc = new PosConfigService(_db);
        _salesSvc = new PosSalesService(_db, new MockFinRevenue(), null!);
        _reportSvc = new PosReportService(_db);
    }

    public void Dispose() => _db.Dispose();

    private async Task<(PosStoreDto store, PosShiftDto shift, PosSaleDto sale)> CreateClosedShiftWithPaidSaleAsync()
    {
        var store = await _configSvc.UpsertStoreAsync(_tenant, _userAdmin, new PosStoreUpsertRequest(null, "STORE-84", "CH POS 84", null, "Active", null, null));
        var shift = await _salesSvc.OpenShiftAsync(_tenant, _userAdmin, new PosShiftOpenRequest(store.Id, null, 500000m, null));
        var sale = await _salesSvc.OpenSaleAsync(_tenant, _userAdmin, new PosSaleOpenRequest(shift.Id, "Bàn 1", null));
        var prod = await _configSvc.UpsertProductAsync(_tenant, _userAdmin, new PosProductUpsertRequest(null, null, "P-84", "Món Ăn 84", "Cái", "Active", 1));
        await _salesSvc.UpsertSaleLineAsync(_tenant, _userAdmin, sale.Id, new PosSaleLineUpsertRequest(null, prod.Id, prod.Code, prod.Name, 2, 60000m, 0m));
        await _salesSvc.PaySaleAsync(_tenant, _userAdmin, sale.Id, new PosSalePayRequest("Cash", 120000m, null));
        var closedShift = await _salesSvc.CloseShiftAsync(_tenant, _userAdmin, shift.Id, new PosShiftCloseRequest(620000m, "Đóng ca 84"));

        var updatedSale = (await _salesSvc.GetSaleDetailAsync(_tenant, sale.Id)).Sale;
        return (store, closedShift, updatedSale);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_059: Đồng bộ doanh thu ca sang FIN
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_059_SyncShiftRevenueToFin_ClosedShift_Success()
    {
        var (_, shift, _) = await CreateClosedShiftWithPaidSaleAsync();

        var res = await _salesSvc.SyncShiftRevenueToFinAsync(_tenant, _userAdmin, shift.Id);

        Assert.NotNull(res);
        Assert.Equal(shift.Id, res.ShiftId);
        Assert.True(res.PaidSaleCount >= 1);
    }

    [Fact]
    public async Task UC_POS_059_SyncShiftRevenueToFin_OpenShiftWithNoPaidSales_ReturnsZeroSynced()
    {
        var store = await _configSvc.UpsertStoreAsync(_tenant, _userAdmin, new PosStoreUpsertRequest(null, "STORE-84B", "CH POS 84B", null, "Active", null, null));
        var shift = await _salesSvc.OpenShiftAsync(_tenant, _userAdmin, new PosShiftOpenRequest(store.Id, null, 200000m, null));

        var res = await _salesSvc.SyncShiftRevenueToFinAsync(_tenant, _userAdmin, shift.Id);
        Assert.Equal(0, res.PaidSaleCount);
        Assert.Equal(0, res.SyncedCount);
    }

    [Fact]
    public async Task UC_POS_059_SyncShiftRevenueToFin_NoPaidSales_ReturnsZeroSynced()
    {
        var store = await _configSvc.UpsertStoreAsync(_tenant, _userAdmin, new PosStoreUpsertRequest(null, "STORE-84C", "CH POS 84C", null, "Active", null, null));
        var shift = await _salesSvc.OpenShiftAsync(_tenant, _userAdmin, new PosShiftOpenRequest(store.Id, null, 200000m, null));
        var closedShift = await _salesSvc.CloseShiftAsync(_tenant, _userAdmin, shift.Id, new PosShiftCloseRequest(200000m, "Ca rỗng"));

        var res = await _salesSvc.SyncShiftRevenueToFinAsync(_tenant, _userAdmin, closedShift.Id);

        Assert.Equal(0, res.PaidSaleCount);
        Assert.Equal(0, res.SyncedCount);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_061: Doanh thu theo giờ / ngày / ca
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_061_RevenueByTime_HourlyGrain_ReturnsHourlyRow()
    {
        await CreateClosedShiftWithPaidSaleAsync();
        var from = DateTimeOffset.UtcNow.AddDays(-1);
        var to = DateTimeOffset.UtcNow.AddDays(1);

        var rows = await _reportSvc.RevenueByTimeAsync(_tenant, from, to, "hour");

        Assert.NotEmpty(rows);
        Assert.True(rows[0].SaleCount >= 1);
    }

    [Fact]
    public async Task UC_POS_061_RevenueByTime_DailyGrain_ReturnsDailyRow()
    {
        await CreateClosedShiftWithPaidSaleAsync();
        var from = DateTimeOffset.UtcNow.AddDays(-1);
        var to = DateTimeOffset.UtcNow.AddDays(1);

        var rows = await _reportSvc.RevenueByTimeAsync(_tenant, from, to, "day");

        Assert.NotEmpty(rows);
        Assert.True(rows[0].Revenue > 0);
    }

    [Fact]
    public async Task UC_POS_061_RevenueByTime_ShiftGrain_ReturnsShiftRow()
    {
        await CreateClosedShiftWithPaidSaleAsync();
        var from = DateTimeOffset.UtcNow.AddDays(-1);
        var to = DateTimeOffset.UtcNow.AddDays(1);

        var rows = await _reportSvc.RevenueByTimeAsync(_tenant, from, to, "shift");

        Assert.NotEmpty(rows);
        Assert.NotNull(rows[0].ShiftId);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_062: Doanh thu theo sản phẩm
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_062_RevenueByProduct_ReturnsGroupedProducts()
    {
        await CreateClosedShiftWithPaidSaleAsync();
        var from = DateTimeOffset.UtcNow.AddDays(-1);
        var to = DateTimeOffset.UtcNow.AddDays(1);

        var rows = await _reportSvc.RevenueByProductAsync(_tenant, from, to);

        Assert.NotEmpty(rows);
        Assert.Equal("P-84", rows[0].ProductCode);
        Assert.Equal(120000m, rows[0].Revenue);
    }

    [Fact]
    public async Task UC_POS_062_RevenueByProduct_NoSales_ReturnsEmptyList()
    {
        var from = DateTimeOffset.UtcNow.AddDays(-10);
        var to = DateTimeOffset.UtcNow.AddDays(-5);

        var rows = await _reportSvc.RevenueByProductAsync(_tenant, from, to);

        Assert.Empty(rows);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_063: Doanh thu theo thu ngân
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_063_RevenueByCashier_ReturnsCashierRow()
    {
        await CreateClosedShiftWithPaidSaleAsync();
        var from = DateTimeOffset.UtcNow.AddDays(-1);
        var to = DateTimeOffset.UtcNow.AddDays(1);

        var rows = await _reportSvc.RevenueByCashierAsync(_tenant, from, to);

        Assert.NotEmpty(rows);
        Assert.Equal(_userAdmin, rows[0].CashierUserId);
        Assert.True(rows[0].Revenue >= 120000m);
    }

    [Fact]
    public async Task UC_POS_063_RevenueByCashier_NoSales_ReturnsEmptyList()
    {
        var from = DateTimeOffset.UtcNow.AddDays(-10);
        var to = DateTimeOffset.UtcNow.AddDays(-5);

        var rows = await _reportSvc.RevenueByCashierAsync(_tenant, from, to);

        Assert.Empty(rows);
    }
}
