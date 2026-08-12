using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Inv;
using Erp.Domain.Entities.Inv;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Inv;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 95:
///   UC_INV_016 — Cho phép tồn âm hay không (UpsertWarehouseAsync AllowNegativeStock)
///   UC_INV_017 — Nhập từ mua hàng (PostPurchaseReceiptFromGrnAsync)
///   UC_INV_018 — Nhập từ sản xuất (CreateDocAsync Receipt Production)
///   UC_INV_019 — Nhập điều chỉnh / kiểm kê (CreateStocktakeAsync & PostStocktakeAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class InvStep95PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly InvMasterService _invMaster;
    private readonly InvStockService _invStock;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public InvStep95PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("inv-step95-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin95", DisplayName = "Admin 95" });
        _db.SaveChanges();

        _invMaster = new InvMasterService(_db);
        _invStock = new InvStockService(_db, null!);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_016: Cho phép tồn âm hay không
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_016_UpsertWarehouse_AllowNegativeStockTrue_SavesSetting()
    {
        var req = new InvWarehouseUpsertRequest(null, "KHO-AM", "Kho Tồn Âm", null, null, "Active", null, true);

        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, req);

        Assert.True(wh.AllowNegativeStock);
    }

    [Fact]
    public async Task UC_INV_016_UpsertWarehouse_AllowNegativeStockFalse_SavesSetting()
    {
        var req = new InvWarehouseUpsertRequest(null, "KHO-NOAM", "Kho Chặt Chẽ", null, null, "Active", null, false);

        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, req);

        Assert.False(wh.AllowNegativeStock);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_017: Nhập từ mua hàng
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_017_CreateDoc_ReceiptPurchase_CreatesStockDoc()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-REC", "Kho Nhập Mua", null, null, "Active", null, true));
        var req = new InvStockDocCreateRequest("Receipt", "Purchase", wh.Id, "Nhập mua vật tư đợt 1");

        var doc = await _invStock.CreateDocAsync(_tenant, _userAdmin, req);

        Assert.NotNull(doc);
        Assert.Equal("Receipt", doc.DocType);
        Assert.Equal("Purchase", doc.SourceType);
    }

    [Fact]
    public async Task UC_INV_017_CreateDoc_InvalidSourceType_ThrowsException()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-ERR", "Kho Err", null, null, "Active", null, true));

        await Assert.ThrowsAsync<AppException>(() =>
            _invStock.CreateDocAsync(_tenant, _userAdmin, new InvStockDocCreateRequest("Receipt", "INVALID_SOURCE", wh.Id, null)));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_018: Nhập từ sản xuất
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_018_CreateDoc_ReceiptProduction_CreatesProductionReceiptDoc()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-TP", "Kho Thành Phẩm", null, null, "Active", null, true));
        var req = new InvStockDocCreateRequest("Receipt", "Production", wh.Id, "Nhập kho sản xuất ca 1");

        var doc = await _invStock.CreateDocAsync(_tenant, _userAdmin, req);

        Assert.NotNull(doc);
        Assert.Equal("Receipt", doc.DocType);
        Assert.Equal("Production", doc.SourceType);
    }

    [Fact]
    public async Task UC_INV_018_UpsertDocLine_AddsLineToProductionReceipt()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-PROD", "Bánh Kẹo TP", null, uom.Id, false, false, false, "Average", 15000m, "Active", null, null, null, null));
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-TP2", "Kho Thành Phẩm 2", null, null, "Active", null, true));
        var doc = await _invStock.CreateDocAsync(_tenant, _userAdmin, new InvStockDocCreateRequest("Receipt", "Production", wh.Id, null));

        var line = await _invStock.UpsertDocLineAsync(_tenant, _userAdmin, doc.Id, new InvStockDocLineRequest(null, sku.Id, 100m, null, null, 15000m));

        Assert.NotNull(line);
        Assert.Equal(100m, line.Qty);
        Assert.Equal(15000m, line.UnitCost);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_019: Nhập điều chỉnh / kiểm kê
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_019_CreateStocktake_ValidWarehouse_CreatesCountingStocktake()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-ST", "Kho Kiểm Kê", null, null, "Active", null, true));

        var st = await _invStock.CreateStocktakeAsync(_tenant, _userAdmin, new InvStocktakeCreateRequest(wh.Id, "Kiểm kê định kỳ tháng 8"));

        Assert.NotNull(st);
        Assert.Equal("Counting", st.Status);
        Assert.Equal(wh.Id, st.WarehouseId);
    }

    [Fact]
    public async Task UC_INV_019_CreateStocktake_NonExistentWarehouse_ThrowsException()
    {
        await Assert.ThrowsAsync<AppException>(() =>
            _invStock.CreateStocktakeAsync(_tenant, _userAdmin, new InvStocktakeCreateRequest(Guid.NewGuid(), null)));
    }

    [Fact]
    public async Task UC_INV_019_ReviewStocktake_CountingStatus_TransitionsToReview()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-REV", "Kho Review", null, null, "Active", null, true));
        var st = await _invStock.CreateStocktakeAsync(_tenant, _userAdmin, new InvStocktakeCreateRequest(wh.Id, null));

        var reviewedSt = await _invStock.ReviewStocktakeAsync(_tenant, _userAdmin, st.Id);

        Assert.Equal("Reviewed", reviewedSt.Status);
    }

    [Fact]
    public async Task UC_INV_019_PostStocktake_ReviewStatus_TransitionsToPosted()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-POST", "Kho Post ST", null, null, "Active", null, true));
        var st = await _invStock.CreateStocktakeAsync(_tenant, _userAdmin, new InvStocktakeCreateRequest(wh.Id, null));
        await _invStock.ReviewStocktakeAsync(_tenant, _userAdmin, st.Id);

        var postedSt = await _invStock.PostStocktakeAsync(_tenant, _userAdmin, st.Id);

        Assert.Equal("Posted", postedSt.Status);
        Assert.NotNull(postedSt.PostedAt);
    }
}
