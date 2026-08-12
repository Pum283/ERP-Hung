using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Inv;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Fin;
using Erp.Infrastructure.Implementations.Services.Inv;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 101:
///   UC_INV_049 — Tạo phiếu kiểm kê (CreateStocktakeAsync)
///   UC_INV_050 — Nhập số đếm thực tế (CountStocktakeLineAsync)
///   UC_INV_052 — Đối chiếu lệch kiểm kê (GetStocktakeDetailAsync)
///   UC_INV_053 — Duyệt điều chỉnh sau kiểm kê (ReviewStocktakeAsync & PostStocktakeAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class InvStep101PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly InvMasterService _invMaster;
    private readonly InvStockService _invStock;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public InvStep101PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("inv-step101-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin101", DisplayName = "Admin 101" });
        _db.SaveChanges();

        var finAcc = new FinAccountingService(_db);
        var finRev = new FinRevenueService(_db, finAcc);
        _invMaster = new InvMasterService(_db);
        _invStock = new InvStockService(_db, finRev);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_049: Tạo phiếu kiểm kê
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_049_CreateStocktake_ValidWarehouse_CreatesCountingStocktake()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-ST101", "Kho ST 101", null, null, "Active", null, true));
        var req = new InvStocktakeCreateRequest(wh.Id, "Kiểm kê tháng 8");

        var created = await _invStock.CreateStocktakeAsync(_tenant, _userAdmin, req);

        Assert.NotNull(created);
        Assert.Equal("Counting", created.Status);
    }

    [Fact]
    public async Task UC_INV_049_ListStocktakes_ReturnsStocktakesList()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-ST101B", "Kho ST 101B", null, null, "Active", null, true));
        await _invStock.CreateStocktakeAsync(_tenant, _userAdmin, new InvStocktakeCreateRequest(wh.Id, null));

        var list = await _invStock.ListStocktakesAsync(_tenant);

        Assert.NotEmpty(list);
    }

    [Fact]
    public async Task UC_INV_049_CreateStocktake_NonExistentWarehouse_ThrowsException()
    {
        await Assert.ThrowsAsync<AppException>(() =>
            _invStock.CreateStocktakeAsync(_tenant, _userAdmin, new InvStocktakeCreateRequest(Guid.NewGuid(), null)));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_050: Nhập số đếm thực tế
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_050_CountStocktakeLine_ValidCount_UpdatesCountedQty()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-ST101", "SP ST 101", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null));
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-ST101C", "Kho ST 101C", null, null, "Active", null, true));
        var doc = await _invStock.CreateDocAsync(_tenant, _userAdmin, new InvStockDocCreateRequest("Receipt", "Purchase", wh.Id, null));
        await _invStock.UpsertDocLineAsync(_tenant, _userAdmin, doc.Id, new InvStockDocLineRequest(null, sku.Id, 10m, null, null, 10000m));
        await _invStock.PostDocAsync(_tenant, _userAdmin, doc.Id);

        var created = await _invStock.CreateStocktakeAsync(_tenant, _userAdmin, new InvStocktakeCreateRequest(wh.Id, null));
        var detail = await _invStock.GetStocktakeDetailAsync(_tenant, created.Id);
        var lineId = detail.Lines[0].Id;

        var countedLine = await _invStock.CountStocktakeLineAsync(_tenant, _userAdmin, created.Id, new InvStocktakeCountRequest(lineId, 12m));

        Assert.NotNull(countedLine);
        Assert.Equal(12m, countedLine.CountedQty);
        Assert.Equal(2m, countedLine.VarianceQty);
    }

    [Fact]
    public async Task UC_INV_050_CountStocktakeLine_NegativeQty_ThrowsException()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-ST101D", "SP ST 101D", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null));
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-ST101D", "Kho ST 101D", null, null, "Active", null, true));
        var doc = await _invStock.CreateDocAsync(_tenant, _userAdmin, new InvStockDocCreateRequest("Receipt", "Purchase", wh.Id, null));
        await _invStock.UpsertDocLineAsync(_tenant, _userAdmin, doc.Id, new InvStockDocLineRequest(null, sku.Id, 5m, null, null, 10000m));
        await _invStock.PostDocAsync(_tenant, _userAdmin, doc.Id);

        var created = await _invStock.CreateStocktakeAsync(_tenant, _userAdmin, new InvStocktakeCreateRequest(wh.Id, null));
        var detail = await _invStock.GetStocktakeDetailAsync(_tenant, created.Id);
        var lineId = detail.Lines[0].Id;

        await Assert.ThrowsAsync<AppException>(() =>
            _invStock.CountStocktakeLineAsync(_tenant, _userAdmin, created.Id, new InvStocktakeCountRequest(lineId, -5m)));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_052: Đối chiếu lệch kiểm kê
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_052_GetStocktakeDetail_ReturnsHeaderAndVarianceLines()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-ST101E", "Kho ST 101E", null, null, "Active", null, true));
        var created = await _invStock.CreateStocktakeAsync(_tenant, _userAdmin, new InvStocktakeCreateRequest(wh.Id, null));

        var detail = await _invStock.GetStocktakeDetailAsync(_tenant, created.Id);

        Assert.NotNull(detail);
        Assert.NotNull(detail.Header);
        Assert.NotNull(detail.Lines);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_053: Duyệt điều chỉnh sau kiểm kê
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_053_ReviewStocktake_CountingStatusAllCounted_TransitionsToReviewed()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-ST101F", "SP ST 101F", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null));
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-ST101F", "Kho ST 101F", null, null, "Active", null, true));
        var doc = await _invStock.CreateDocAsync(_tenant, _userAdmin, new InvStockDocCreateRequest("Receipt", "Purchase", wh.Id, null));
        await _invStock.UpsertDocLineAsync(_tenant, _userAdmin, doc.Id, new InvStockDocLineRequest(null, sku.Id, 10m, null, null, 10000m));
        await _invStock.PostDocAsync(_tenant, _userAdmin, doc.Id);

        var created = await _invStock.CreateStocktakeAsync(_tenant, _userAdmin, new InvStocktakeCreateRequest(wh.Id, null));
        var detail = await _invStock.GetStocktakeDetailAsync(_tenant, created.Id);
        await _invStock.CountStocktakeLineAsync(_tenant, _userAdmin, created.Id, new InvStocktakeCountRequest(detail.Lines[0].Id, 10m));

        var reviewed = await _invStock.ReviewStocktakeAsync(_tenant, _userAdmin, created.Id);

        Assert.NotNull(reviewed);
        Assert.Equal("Reviewed", reviewed.Status);
    }

    [Fact]
    public async Task UC_INV_053_PostStocktake_ReviewedStatus_TransitionsToPosted()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-ST101G", "SP ST 101G", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null));
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-ST101G", "Kho ST 101G", null, null, "Active", null, true));
        var doc = await _invStock.CreateDocAsync(_tenant, _userAdmin, new InvStockDocCreateRequest("Receipt", "Purchase", wh.Id, null));
        await _invStock.UpsertDocLineAsync(_tenant, _userAdmin, doc.Id, new InvStockDocLineRequest(null, sku.Id, 10m, null, null, 10000m));
        await _invStock.PostDocAsync(_tenant, _userAdmin, doc.Id);

        var created = await _invStock.CreateStocktakeAsync(_tenant, _userAdmin, new InvStocktakeCreateRequest(wh.Id, null));
        var detail = await _invStock.GetStocktakeDetailAsync(_tenant, created.Id);
        await _invStock.CountStocktakeLineAsync(_tenant, _userAdmin, created.Id, new InvStocktakeCountRequest(detail.Lines[0].Id, 12m));
        var reviewed = await _invStock.ReviewStocktakeAsync(_tenant, _userAdmin, created.Id);

        var posted = await _invStock.PostStocktakeAsync(_tenant, _userAdmin, reviewed.Id);

        Assert.NotNull(posted);
        Assert.Equal("Posted", posted.Status);
    }

    [Fact]
    public async Task UC_INV_053_PostStocktake_CountingStatus_ThrowsException()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-ST101H", "Kho ST 101H", null, null, "Active", null, true));
        var created = await _invStock.CreateStocktakeAsync(_tenant, _userAdmin, new InvStocktakeCreateRequest(wh.Id, null));

        await Assert.ThrowsAsync<AppException>(() =>
            _invStock.PostStocktakeAsync(_tenant, _userAdmin, created.Id));
    }

    [Fact]
    public async Task UC_INV_053_ReviewStocktake_UncountedLines_ThrowsException()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-ST101I", "SP ST 101I", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null));
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-ST101I", "Kho ST 101I", null, null, "Active", null, true));
        var doc = await _invStock.CreateDocAsync(_tenant, _userAdmin, new InvStockDocCreateRequest("Receipt", "Purchase", wh.Id, null));
        await _invStock.UpsertDocLineAsync(_tenant, _userAdmin, doc.Id, new InvStockDocLineRequest(null, sku.Id, 10m, null, null, 10000m));
        await _invStock.PostDocAsync(_tenant, _userAdmin, doc.Id);

        var created = await _invStock.CreateStocktakeAsync(_tenant, _userAdmin, new InvStocktakeCreateRequest(wh.Id, null));

        await Assert.ThrowsAsync<AppException>(() =>
            _invStock.ReviewStocktakeAsync(_tenant, _userAdmin, created.Id));
    }
}
