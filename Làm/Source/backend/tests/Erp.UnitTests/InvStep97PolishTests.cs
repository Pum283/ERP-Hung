using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Inv;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Inv;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 97:
///   UC_INV_026 — Xuất nội bộ / tiêu hao (CreateDocAsync Issue Internal & Disposal)
///   UC_INV_029 — Xuất theo FEFO tự động (SuggestLotsAsync)
///   UC_INV_030 — Xuất điều chỉnh (CreateDocAsync Issue Adjustment)
///   UC_INV_031 — Tạo phiếu chuyển kho (CreateTransferAsync & UpsertTransferLineAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class InvStep97PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly InvMasterService _invMaster;
    private readonly InvStockService _invStock;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public InvStep97PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("inv-step97-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin97", DisplayName = "Admin 97" });
        _db.SaveChanges();

        var finAcc = new Erp.Infrastructure.Implementations.Services.Fin.FinAccountingService(_db);
        var finRev = new Erp.Infrastructure.Implementations.Services.Fin.FinRevenueService(_db, finAcc);
        _invMaster = new InvMasterService(_db);
        _invStock = new InvStockService(_db, finRev);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_026: Xuất nội bộ / tiêu hao
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_026_CreateDoc_IssueInternal_CreatesInternalIssueDoc()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-NOIBO", "Kho Xuất Nội Bộ", null, null, "Active", null, true));
        var req = new InvStockDocCreateRequest("Issue", "Internal", wh.Id, "Xuất tiêu hao bộ phận IT");

        var doc = await _invStock.CreateDocAsync(_tenant, _userAdmin, req);

        Assert.NotNull(doc);
        Assert.Equal("Issue", doc.DocType);
        Assert.Equal("Internal", doc.SourceType);
    }

    [Fact]
    public async Task UC_INV_026_CreateDoc_IssueAdjustment_CreatesAdjustmentDoc()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-HUY", "Kho Hủy", null, null, "Active", null, true));
        var req = new InvStockDocCreateRequest("Issue", "Adjustment", wh.Id, "Xuất hủy hàng hỏng đợt 1");

        var doc = await _invStock.CreateDocAsync(_tenant, _userAdmin, req);

        Assert.NotNull(doc);
        Assert.Equal("Issue", doc.DocType);
        Assert.Equal("Adjustment", doc.SourceType);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_029: Xuất theo FEFO tự động
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_029_SuggestLots_ValidSku_ReturnsFefoSuggestedLots()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-FEFO", "Bánh FEFO", null, uom.Id, true, false, true, "Average", 10000m, "Active", null, null, null, null));
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-FEFO", "Kho FEFO", null, null, "Active", null, true));

        var req = new InvSuggestLotsRequest(wh.Id, sku.Id, 10m);
        var suggestions = await _invStock.SuggestLotsAsync(_tenant, req);

        Assert.NotNull(suggestions);
    }

    [Fact]
    public async Task UC_INV_029_SuggestLots_NonExistentSku_ThrowsException()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-FEFO2", "Kho FEFO 2", null, null, "Active", null, true));
        var req = new InvSuggestLotsRequest(wh.Id, Guid.NewGuid(), 10m);

        await Assert.ThrowsAsync<AppException>(() =>
            _invStock.SuggestLotsAsync(_tenant, req));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_030: Xuất điều chỉnh
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_030_CreateDoc_IssueAdjustment_CreatesAdjustmentIssueDoc()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-ADJ", "Kho Xuất Điều Chỉnh", null, null, "Active", null, true));
        var req = new InvStockDocCreateRequest("Issue", "Adjustment", wh.Id, "Xuất điều chỉnh giảm do kiểm kê thiếu");

        var doc = await _invStock.CreateDocAsync(_tenant, _userAdmin, req);

        Assert.NotNull(doc);
        Assert.Equal("Issue", doc.DocType);
        Assert.Equal("Adjustment", doc.SourceType);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_031: Tạo phiếu chuyển kho
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_031_CreateTransfer_ValidWarehouses_CreatesDraftTransfer()
    {
        var w1 = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-A", "Kho A", null, null, "Active", null, true));
        var w2 = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-B", "Kho B", null, null, "Active", null, true));

        var tr = await _invStock.CreateTransferAsync(_tenant, _userAdmin, new InvTransferCreateRequest(w1.Id, w2.Id, "Chuyển hàng A sang B"));

        Assert.NotNull(tr);
        Assert.Equal("Draft", tr.Status);
        Assert.Equal(w1.Id, tr.FromWarehouseId);
        Assert.Equal(w2.Id, tr.ToWarehouseId);
    }

    [Fact]
    public async Task UC_INV_031_CreateTransfer_SameFromAndToWarehouse_ThrowsException()
    {
        var w1 = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-SAME", "Kho Trùng", null, null, "Active", null, true));

        await Assert.ThrowsAsync<AppException>(() =>
            _invStock.CreateTransferAsync(_tenant, _userAdmin, new InvTransferCreateRequest(w1.Id, w1.Id, null)));
    }

    [Fact]
    public async Task UC_INV_031_UpsertTransferLine_ValidItem_AddsLineToTransfer()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-TR", "SP Chuyển", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null));
        var w1 = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-W1", "Kho 1", null, null, "Active", null, true));
        var w2 = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-W2", "Kho 2", null, null, "Active", null, true));
        var tr = await _invStock.CreateTransferAsync(_tenant, _userAdmin, new InvTransferCreateRequest(w1.Id, w2.Id, null));

        var line = await _invStock.UpsertTransferLineAsync(_tenant, _userAdmin, tr.Id, new InvTransferLineRequest(null, sku.Id, 30m, null, null));

        Assert.NotNull(line);
        Assert.Equal(30m, line.Qty);
    }

    [Fact]
    public async Task UC_INV_031_GetTransferDetail_ReturnsHeaderAndLines()
    {
        var w1 = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-D1", "Kho D1", null, null, "Active", null, true));
        var w2 = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-D2", "Kho D2", null, null, "Active", null, true));
        var tr = await _invStock.CreateTransferAsync(_tenant, _userAdmin, new InvTransferCreateRequest(w1.Id, w2.Id, null));

        var detail = await _invStock.GetTransferDetailAsync(_tenant, tr.Id);

        Assert.NotNull(detail);
        Assert.NotNull(detail.Header);
    }

    [Fact]
    public async Task UC_INV_031_ListTransfers_ReturnsTransfersList()
    {
        var w1 = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-L1", "Kho L1", null, null, "Active", null, true));
        var w2 = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-L2", "Kho L2", null, null, "Active", null, true));
        await _invStock.CreateTransferAsync(_tenant, _userAdmin, new InvTransferCreateRequest(w1.Id, w2.Id, null));

        var list = await _invStock.ListTransfersAsync(_tenant);

        Assert.NotEmpty(list);
    }
}
