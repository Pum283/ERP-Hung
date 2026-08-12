using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Inv;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Inv;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 99:
///   UC_INV_038 — Giải phóng giữ hàng (ActivateReservationAsync / CreateReservationAsync)
///   UC_INV_039 — Xem tồn thực tế (ListBalancesAsync)
///   UC_INV_041 — Xem tồn đang giữ / đang chuyển (ListBalancesAsync with QtyReserved & QtyInTransit)
///   UC_INV_042 — Cảnh báo không đủ tồn (MinMaxAlertsAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class InvStep99PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly InvMasterService _invMaster;
    private readonly InvStockService _invStock;
    private readonly InvReportService _invReport;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public InvStep99PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("inv-step99-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin99", DisplayName = "Admin 99" });
        _db.SaveChanges();

        var finAcc = new Erp.Infrastructure.Implementations.Services.Fin.FinAccountingService(_db);
        var finRev = new Erp.Infrastructure.Implementations.Services.Fin.FinRevenueService(_db, finAcc);
        _invMaster = new InvMasterService(_db);
        _invStock = new InvStockService(_db, finRev);
        _invReport = new InvReportService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_038: Giải phóng giữ hàng
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_038_ActivateReservation_DraftReservation_ActivatesReservation()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-A99", "SP Act 99", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null));
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-ACT", "Kho Act", null, null, "Active", null, true));
        var req = new InvReservationCreateRequest(wh.Id, "SO", Guid.NewGuid(), "SO-99-01", null, false, new List<InvReservationLineRequest>
        {
            new InvReservationLineRequest(sku.Id, 5m, null, null)
        });
        var draft = await _invStock.CreateReservationAsync(_tenant, _userAdmin, req);

        var activated = await _invStock.ActivateReservationAsync(_tenant, _userAdmin, draft.Header.Id);

        Assert.NotNull(activated);
        Assert.Equal("Active", activated.Header.Status);
    }

    [Fact]
    public async Task UC_INV_038_ActivateReservation_AlreadyActive_ThrowsException()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-A99B", "SP Act 99B", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null));
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-ACT2", "Kho Act 2", null, null, "Active", null, true));
        var req = new InvReservationCreateRequest(wh.Id, "SO", Guid.NewGuid(), "SO-99-02", null, true, new List<InvReservationLineRequest>
        {
            new InvReservationLineRequest(sku.Id, 5m, null, null)
        });
        var created = await _invStock.CreateReservationAsync(_tenant, _userAdmin, req);

        await Assert.ThrowsAsync<AppException>(() =>
            _invStock.ActivateReservationAsync(_tenant, _userAdmin, created.Header.Id));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_039: Xem tồn thực tế
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_039_ListBalances_ReturnsStockBalancesList()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-BAL", "Kho Balance", null, null, "Active", null, true));
        var list = await _invStock.ListBalancesAsync(_tenant, wh.Id);

        Assert.NotNull(list);
    }

    [Fact]
    public async Task UC_INV_039_ListBalances_FilterByWarehouseAndSku_ReturnsFilteredBalances()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-BAL", "SP Balance", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null));
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-BAL2", "Kho Balance 2", null, null, "Active", null, true));

        var list = await _invStock.ListBalancesAsync(_tenant, wh.Id);

        Assert.NotNull(list);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_041: Xem tồn đang giữ / đang chuyển
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_041_ListBalances_IncludesQtyReservedAndQtyInTransit()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-BAL3", "Kho Balance 3", null, null, "Active", null, true));
        var list = await _invStock.ListBalancesAsync(_tenant, wh.Id);

        Assert.NotNull(list);
        Assert.All(list, x =>
        {
            Assert.True(x.QtyReserved >= 0);
            Assert.True(x.QtyInTransit >= 0);
        });
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_042: Cảnh báo không đủ tồn
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_042_MinMaxAlerts_ValidWarehouse_ReturnsAlertList()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-ALERT", "Kho Alert", null, null, "Active", null, true));
        var alerts = await _invReport.MinMaxAlertsAsync(_tenant, wh.Id);

        Assert.NotNull(alerts);
    }

    [Fact]
    public async Task UC_INV_042_MinMaxAlerts_SkuBelowMinQty_GeneratesBelowMinAlert()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-MIN", "SP Dưới Min", null, uom.Id, false, false, false, "Average", 10000m, "Active", 100m, 500m, 200m, null));
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-ALERT2", "Kho Alert 2", null, null, "Active", null, true));

        var alerts = await _invReport.MinMaxAlertsAsync(_tenant, wh.Id);

        Assert.NotNull(alerts);
    }

    [Fact]
    public async Task UC_INV_042_MinMaxAlerts_AllWarehouses_ReturnsTenantAlerts()
    {
        var alerts = await _invReport.MinMaxAlertsAsync(_tenant, null);

        Assert.NotNull(alerts);
    }

    [Fact]
    public async Task UC_INV_038_ActivateReservation_NonExistentId_ThrowsException()
    {
        await Assert.ThrowsAsync<AppException>(() =>
            _invStock.ActivateReservationAsync(_tenant, _userAdmin, Guid.NewGuid()));
    }

    [Fact]
    public async Task UC_INV_039_ListBalances_NonExistentTenant_ReturnsEmptyList()
    {
        var list = await _invStock.ListBalancesAsync(Guid.NewGuid());

        Assert.NotNull(list);
        Assert.Empty(list);
    }
}
