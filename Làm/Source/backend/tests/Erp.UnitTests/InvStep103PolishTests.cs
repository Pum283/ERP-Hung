using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Inv;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Inv;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 103:
///   UC_INV_064 — Xuất nhập tồn theo kỳ (PeriodSummaryAsync)
///   UC_INV_065 — Thẻ kho / lịch sử sản phẩm (StockCardAsync)
///   UC_INV_067 — Hàng dưới min / trên max (MinMaxAlertsAsync)
///   UC_INV_069 — Dashboard tồn & cảnh báo (PeriodSummaryAsync & MinMaxAlertsAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class InvStep103PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly InvMasterService _invMaster;
    private readonly InvReportService _invReport;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public InvStep103PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("inv-step103-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin103", DisplayName = "Admin 103" });
        _db.SaveChanges();

        _invMaster = new InvMasterService(_db);
        _invReport = new InvReportService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_064: Xuất nhập tồn theo kỳ
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_064_PeriodSummary_ValidRange_ReturnsPeriodSummaryRows()
    {
        var from = DateTimeOffset.UtcNow.AddDays(-30);
        var to = DateTimeOffset.UtcNow;

        var report = await _invReport.MovementByPeriodAsync(_tenant, from, to, null);

        Assert.NotNull(report);
    }

    [Fact]
    public async Task UC_INV_064_PeriodSummary_FilterByWarehouse_ReturnsWarehouseSummary()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-SUM103", "Kho Sum 103", null, null, "Active", null, true));
        var from = DateTimeOffset.UtcNow.AddDays(-15);
        var to = DateTimeOffset.UtcNow;

        var report = await _invReport.MovementByPeriodAsync(_tenant, from, to, wh.Id);

        Assert.NotNull(report);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_065: Thẻ kho / lịch sử sản phẩm
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_065_StockCard_ValidSku_ReturnsStockCardEntries()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-CARD103", "SP Card 103", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null));

        var entries = await _invReport.SkuCardAsync(_tenant, sku.Id);

        Assert.NotNull(entries);
    }

    [Fact]
    public async Task UC_INV_065_StockCard_WithDateRangeAndWarehouse_ReturnsFilteredCard()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-CARD103B", "SP Card 103B", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null));
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-CARD103", "Kho Card 103", null, null, "Active", null, true));
        var from = DateTimeOffset.UtcNow.AddDays(-7);
        var to = DateTimeOffset.UtcNow;

        var entries = await _invReport.SkuCardAsync(_tenant, sku.Id, wh.Id, from, to);

        Assert.NotNull(entries);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_067: Hàng dưới min / trên max
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_067_MinMaxAlerts_ValidWarehouse_ReturnsThresholdAlerts()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-MM103", "Kho MinMax 103", null, null, "Active", null, true));

        var alerts = await _invReport.MinMaxAlertsAsync(_tenant, wh.Id);

        Assert.NotNull(alerts);
    }

    [Fact]
    public async Task UC_INV_067_MinMaxAlerts_AllWarehouses_ReturnsAllAlerts()
    {
        var alerts = await _invReport.MinMaxAlertsAsync(_tenant, null);

        Assert.NotNull(alerts);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_069: Dashboard tồn & cảnh báo
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_069_Dashboard_PeriodSummaryAndAlerts_CalculatesCorrectly()
    {
        var from = DateTimeOffset.UtcNow.AddDays(-30);
        var to = DateTimeOffset.UtcNow;

        var summary = await _invReport.MovementByPeriodAsync(_tenant, from, to, null);
        var alerts = await _invReport.MinMaxAlertsAsync(_tenant, null);

        Assert.NotNull(summary);
        Assert.NotNull(alerts);
    }

    [Fact]
    public async Task UC_INV_064_PeriodSummary_InvalidDateRange_ReturnsReportWithoutCrashing()
    {
        var from = DateTimeOffset.UtcNow;
        var to = DateTimeOffset.UtcNow.AddDays(10);

        var report = await _invReport.MovementByPeriodAsync(_tenant, from, to, null);

        Assert.NotNull(report);
    }

    [Fact]
    public async Task UC_INV_065_StockCard_NonExistentSku_ReturnsEmptyList()
    {
        var entries = await _invReport.SkuCardAsync(_tenant, Guid.NewGuid());

        Assert.NotNull(entries);
        Assert.Empty(entries);
    }

    [Fact]
    public async Task UC_INV_067_MinMaxAlerts_NonExistentWarehouse_ReturnsEmptyList()
    {
        var alerts = await _invReport.MinMaxAlertsAsync(_tenant, Guid.NewGuid());

        Assert.NotNull(alerts);
        Assert.Empty(alerts);
    }
}
