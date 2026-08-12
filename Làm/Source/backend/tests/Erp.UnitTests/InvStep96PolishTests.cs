using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Inv;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Inv;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 96:
///   UC_INV_020 — Nhập chuyển đến (CreateDocAsync Receipt Transfer & ReceiveTransferAsync)
///   UC_INV_022 — Nhập theo lô / HSD / serial (UpsertDocLineAsync with LotCode & ExpiryDate)
///   UC_INV_024 — Xuất bán / giao hàng (CreateDocAsync Issue Sales & Delivery)
///   UC_INV_025 — Xuất sản xuất (CreateDocAsync Issue Production)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class InvStep96PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly InvMasterService _invMaster;
    private readonly InvStockService _invStock;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public InvStep96PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("inv-step96-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin96", DisplayName = "Admin 96" });
        _db.SaveChanges();

        var finAcc = new Erp.Infrastructure.Implementations.Services.Fin.FinAccountingService(_db);
        var finRev = new Erp.Infrastructure.Implementations.Services.Fin.FinRevenueService(_db, finAcc);
        _invMaster = new InvMasterService(_db);
        _invStock = new InvStockService(_db, finRev);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_020: Nhập chuyển đến
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_020_CreateDoc_ReceiptTransfer_CreatesDoc()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-IN", "Kho Nhập Chuyển", null, null, "Active", null, true));
        var req = new InvStockDocCreateRequest("Receipt", "TransferIn", wh.Id, "Nhập kho nhận chuyển đợt 1");

        var doc = await _invStock.CreateDocAsync(_tenant, _userAdmin, req);

        Assert.NotNull(doc);
        Assert.Equal("Receipt", doc.DocType);
        Assert.Equal("TransferIn", doc.SourceType);
    }

    [Fact]
    public async Task UC_INV_020_ReceiveTransfer_ValidTransfer_CompletesTransfer()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-TR96", "SP Chuyển 96", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null));
        var w1 = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-FROM", "Kho Đi", null, null, "Active", null, true));
        var w2 = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-TO", "Kho Đến", null, null, "Active", null, true));
        var tr = await _invStock.CreateTransferAsync(_tenant, _userAdmin, new InvTransferCreateRequest(w1.Id, w2.Id, "Chuyển kho 96"));
        await _invStock.UpsertTransferLineAsync(_tenant, _userAdmin, tr.Id, new InvTransferLineRequest(null, sku.Id, 10m, null, null));
        var shipped = await _invStock.ShipTransferAsync(_tenant, _userAdmin, tr.Id);

        var received = await _invStock.ReceiveTransferAsync(_tenant, _userAdmin, shipped.Id);

        Assert.NotNull(received);
        Assert.Equal("Completed", received.Status);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_022: Nhập theo lô / HSD / serial
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_022_UpsertDocLine_WithLotAndExpiry_SavesLineDetails()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-LOT", "SP Theo Lô", null, uom.Id, true, false, true, "Average", 50000m, "Active", null, null, null, null));
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-LOT", "Kho Lô", null, null, "Active", null, true));
        var doc = await _invStock.CreateDocAsync(_tenant, _userAdmin, new InvStockDocCreateRequest("Receipt", "Purchase", wh.Id, null));

        var expiry = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12));
        var line = await _invStock.UpsertDocLineAsync(_tenant, _userAdmin, doc.Id, new InvStockDocLineRequest(null, sku.Id, 50m, "LOT-2026-A", expiry, 50000m));

        Assert.NotNull(line);
        Assert.Equal("LOT-2026-A", line.LotCode);
        Assert.Equal(expiry, line.ExpiryDate);
    }

    [Fact]
    public async Task UC_INV_022_UpsertDocLine_LotTrackedSku_PreservesLotCode()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "HOP", "Hộp", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-LOT2", "SP Lô 2", null, uom.Id, true, false, false, "Average", 20000m, "Active", null, null, null, null));
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-LOT2", "Kho Lô 2", null, null, "Active", null, true));
        var doc = await _invStock.CreateDocAsync(_tenant, _userAdmin, new InvStockDocCreateRequest("Receipt", "Purchase", wh.Id, null));

        var line = await _invStock.UpsertDocLineAsync(_tenant, _userAdmin, doc.Id, new InvStockDocLineRequest(null, sku.Id, 20m, "BATCH-99", null, 20000m));

        Assert.Equal("BATCH-99", line.LotCode);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_024: Xuất bán / giao hàng
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_024_CreateDoc_IssueSales_CreatesSalesIssueDoc()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-SALE", "Kho Xuất Bán", null, null, "Active", null, true));
        var req = new InvStockDocCreateRequest("Issue", "Sales", wh.Id, "Xuất bán đơn SO-96-01");

        var doc = await _invStock.CreateDocAsync(_tenant, _userAdmin, req);

        Assert.NotNull(doc);
        Assert.Equal("Issue", doc.DocType);
        Assert.Equal("Sales", doc.SourceType);
    }

    [Fact]
    public async Task UC_INV_024_CreateDoc_IssueTransferOut_CreatesTransferOutDoc()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-DELIV", "Kho Giao Hàng", null, null, "Active", null, true));
        var req = new InvStockDocCreateRequest("Issue", "TransferOut", wh.Id, "Xuất giao hàng đợt 2");

        var doc = await _invStock.CreateDocAsync(_tenant, _userAdmin, req);

        Assert.NotNull(doc);
        Assert.Equal("Issue", doc.DocType);
        Assert.Equal("TransferOut", doc.SourceType);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_025: Xuất sản xuất
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_025_CreateDoc_IssueProduction_CreatesProductionIssueDoc()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-NVL", "Kho Xuất NVL", null, null, "Active", null, true));
        var req = new InvStockDocCreateRequest("Issue", "Production", wh.Id, "Xuất NVL cho xưởng 1");

        var doc = await _invStock.CreateDocAsync(_tenant, _userAdmin, req);

        Assert.NotNull(doc);
        Assert.Equal("Issue", doc.DocType);
        Assert.Equal("Production", doc.SourceType);
    }

    [Fact]
    public async Task UC_INV_025_UpsertDocLine_AddsRawMaterialsToProductionIssue()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "KG", "Kg", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-NVL", "Bột Bánh NVL", null, uom.Id, false, false, false, "Average", 15000m, "Active", null, null, null, null));
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-NVL2", "Kho NVL 2", null, null, "Active", null, true));
        var doc = await _invStock.CreateDocAsync(_tenant, _userAdmin, new InvStockDocCreateRequest("Issue", "Production", wh.Id, null));

        var line = await _invStock.UpsertDocLineAsync(_tenant, _userAdmin, doc.Id, new InvStockDocLineRequest(null, sku.Id, 200m, null, null, 15000m));

        Assert.NotNull(line);
        Assert.Equal(200m, line.Qty);
    }

    [Fact]
    public async Task UC_INV_020_CreateDoc_InvalidDocType_ThrowsException()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-ERR", "Kho Lỗi", null, null, "Active", null, true));

        await Assert.ThrowsAsync<AppException>(() =>
            _invStock.CreateDocAsync(_tenant, _userAdmin, new InvStockDocCreateRequest("INVALID_TYPE", "TransferIn", wh.Id, null)));
    }

    [Fact]
    public async Task UC_INV_024_PostDoc_DraftSalesIssue_PostsDocSuccessfully()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-P96", "SP Post 96", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null));
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-POST", "Kho Post", null, null, "Active", null, true));
        var doc = await _invStock.CreateDocAsync(_tenant, _userAdmin, new InvStockDocCreateRequest("Issue", "Sales", wh.Id, null));
        await _invStock.UpsertDocLineAsync(_tenant, _userAdmin, doc.Id, new InvStockDocLineRequest(null, sku.Id, 5m, null, null, 10000m));

        var posted = await _invStock.PostDocAsync(_tenant, _userAdmin, doc.Id);

        Assert.Equal("Posted", posted.Status);
        Assert.NotNull(posted.PostedAt);
    }
}
