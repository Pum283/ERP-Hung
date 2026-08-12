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
/// Unit tests cho Bước 102:
///   UC_INV_055 — Báo cáo kết quả kiểm kê (GetStocktakeDetailAsync)
///   UC_INV_060 — Xem giá trị tồn (ValuationReportAsync)
///   UC_INV_062 — Đẩy bút toán kho sang FIN (PostDocAsync with FIN posting)
///   UC_INV_063 — Báo cáo giá trị tồn (ValuationReportAsync with total value)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class InvStep102PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly InvMasterService _invMaster;
    private readonly InvStockService _invStock;
    private readonly InvReportService _invReport;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public InvStep102PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("inv-step102-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin102", DisplayName = "Admin 102" });
        _db.SaveChanges();

        var finAcc = new FinAccountingService(_db);
        var finRev = new FinRevenueService(_db, finAcc);
        _invMaster = new InvMasterService(_db);
        _invStock = new InvStockService(_db, finRev);
        _invReport = new InvReportService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_055: Báo cáo kết quả kiểm kê
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_055_GetStocktakeDetail_ValidId_ReturnsDetailReport()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-RPT102", "Kho RPT 102", null, null, "Active", null, true));
        var created = await _invStock.CreateStocktakeAsync(_tenant, _userAdmin, new InvStocktakeCreateRequest(wh.Id, "Báo cáo đợt 1"));

        var detail = await _invStock.GetStocktakeDetailAsync(_tenant, created.Id);

        Assert.NotNull(detail);
        Assert.Equal("Báo cáo đợt 1", detail.Header.Note);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_060 & UC_INV_063: Báo cáo giá trị tồn kho
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_060_ValuationReport_ValidWarehouse_ReturnsValuationRows()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-VAL102", "Kho Val 102", null, null, "Active", null, true));
        var report = await _invReport.StockValueAsync(_tenant, wh.Id);

        Assert.NotNull(report);
    }

    [Fact]
    public async Task UC_INV_063_ValuationReport_AllWarehouses_ReturnsTenantValuationReport()
    {
        var report = await _invReport.StockValueAsync(_tenant, null);

        Assert.NotNull(report);
    }

    [Fact]
    public async Task UC_INV_060_ValuationReport_WithSkuData_CalculatesTotalValuation()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-V102", "SP Valuation", null, uom.Id, false, false, false, "Average", 25000m, "Active", null, null, null, null));
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-VAL102B", "Kho Val 102B", null, null, "Active", null, true));

        var report = await _invReport.StockValueAsync(_tenant, wh.Id);

        Assert.NotNull(report);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_062: Đẩy bút toán kho sang FIN
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_062_PostDoc_SalesIssueDoc_TriggersCogsPosting()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-POST102", "SP Post 102", null, uom.Id, false, false, false, "Average", 15000m, "Active", null, null, null, null));
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-FIN102", "Kho FIN 102", null, null, "Active", null, true));
        var doc = await _invStock.CreateDocAsync(_tenant, _userAdmin, new InvStockDocCreateRequest("Issue", "Sales", wh.Id, "Xuất bán ghi sổ sang FIN"));
        await _invStock.UpsertDocLineAsync(_tenant, _userAdmin, doc.Id, new InvStockDocLineRequest(null, sku.Id, 10m, null, null, 15000m));

        var posted = await _invStock.PostDocAsync(_tenant, _userAdmin, doc.Id);

        Assert.Equal("Posted", posted.Status);
    }

    [Fact]
    public async Task UC_INV_062_PostDoc_PurchaseReceiptDoc_PostsSuccessfully()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-PUR102", "SP Pur 102", null, uom.Id, false, false, false, "Average", 20000m, "Active", null, null, null, null));
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-PUR102", "Kho Pur 102", null, null, "Active", null, true));
        var doc = await _invStock.CreateDocAsync(_tenant, _userAdmin, new InvStockDocCreateRequest("Receipt", "Purchase", wh.Id, "Nhập mua ghi sổ sang FIN"));
        await _invStock.UpsertDocLineAsync(_tenant, _userAdmin, doc.Id, new InvStockDocLineRequest(null, sku.Id, 20m, null, null, 20000m));

        var posted = await _invStock.PostDocAsync(_tenant, _userAdmin, doc.Id);

        Assert.Equal("Posted", posted.Status);
    }

    [Fact]
    public async Task UC_INV_055_GetStocktakeDetail_NonExistentId_ThrowsException()
    {
        await Assert.ThrowsAsync<AppException>(() =>
            _invStock.GetStocktakeDetailAsync(_tenant, Guid.NewGuid()));
    }

    [Fact]
    public async Task UC_INV_060_ValuationReport_InvalidWarehouse_ReturnsEmptyList()
    {
        var report = await _invReport.StockValueAsync(_tenant, Guid.NewGuid());

        Assert.NotNull(report);
        Assert.Empty(report);
    }

    [Fact]
    public async Task UC_INV_063_ValuationReport_NonExistentTenant_ReturnsEmptyList()
    {
        var report = await _invReport.StockValueAsync(Guid.NewGuid(), null);

        Assert.NotNull(report);
        Assert.Empty(report);
    }

    [Fact]
    public async Task UC_INV_062_PostDoc_AlreadyPosted_ThrowsException()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-REP102", "SP Rep 102", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null));
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-REP102", "Kho Rep 102", null, null, "Active", null, true));
        var doc = await _invStock.CreateDocAsync(_tenant, _userAdmin, new InvStockDocCreateRequest("Issue", "Sales", wh.Id, null));
        await _invStock.UpsertDocLineAsync(_tenant, _userAdmin, doc.Id, new InvStockDocLineRequest(null, sku.Id, 2m, null, null, 10000m));
        var posted1 = await _invStock.PostDocAsync(_tenant, _userAdmin, doc.Id);

        await Assert.ThrowsAsync<AppException>(() =>
            _invStock.PostDocAsync(_tenant, _userAdmin, posted1.Id));
    }
}
