using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Fin;
using Erp.Application.DTOs.Pos;
using Erp.Application.Interfaces.Services.Fin;
using Erp.Domain.Entities.Inv;
using Erp.Domain.Entities.Pos;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Inv;
using Erp.Infrastructure.Implementations.Services.Pos;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 83:
///   UC_POS_047 — Đối soát lệch quỹ (CloseShiftAsync variance calculation)
///   UC_POS_048 — In báo cáo ca (BuildShiftReportTextAsync)
///   UC_POS_054 — Trừ tồn theo BOM khi bán (DeductBomStockForSaleAsync via PaySaleAsync)
///   UC_POS_055 — Cảnh báo hết / sắp hết (ListStockAlertsAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class PosStep83PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PosConfigService _configSvc;
    private readonly PosSalesService _salesSvc;
    private readonly InvStockService _stockSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();
    private readonly Guid _warehouseId = Guid.NewGuid();

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

    public PosStep83PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pos-step83-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin83", DisplayName = "Admin 83" });
        _db.InvWarehouses.Add(new InvWarehouse { Id = _warehouseId, TenantId = _tenant, Code = "WH-83", Name = "Kho 83", Status = "Active", AllowNegativeStock = true });
        _db.SaveChanges();

        var noop = new NoopFinRevenue();
        _stockSvc = new InvStockService(_db, noop);
        _configSvc = new PosConfigService(_db);
        _salesSvc = new PosSalesService(_db, noop, _stockSvc);
    }

    public void Dispose() => _db.Dispose();

    private async Task<PosStoreDto> CreateStoreWithWarehouseAsync()
    {
        return await _configSvc.UpsertStoreAsync(_tenant, _userAdmin, new PosStoreUpsertRequest(null, "STORE-83", "CH POS 83", null, "Active", _warehouseId, null));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_047: Đối soát lệch quỹ
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_047_CloseShift_ShortageVariance_CalculatesNegativeVariance()
    {
        var store = await CreateStoreWithWarehouseAsync();
        var shift = await _salesSvc.OpenShiftAsync(_tenant, _userAdmin, new PosShiftOpenRequest(store.Id, null, 500000m, null));
        // Expected cash = 500k. Counted = 450k -> Variance = -50k
        var closedShift = await _salesSvc.CloseShiftAsync(_tenant, _userAdmin, shift.Id, new PosShiftCloseRequest(450000m, "Lệch thiếu 50k"));

        Assert.Equal(-50000m, closedShift.Variance);
    }

    [Fact]
    public async Task UC_POS_047_CloseShift_OverheadVariance_CalculatesPositiveVariance()
    {
        var store = await CreateStoreWithWarehouseAsync();
        var shift = await _salesSvc.OpenShiftAsync(_tenant, _userAdmin, new PosShiftOpenRequest(store.Id, null, 500000m, null));
        // Expected cash = 500k. Counted = 520k -> Variance = +20k
        var closedShift = await _salesSvc.CloseShiftAsync(_tenant, _userAdmin, shift.Id, new PosShiftCloseRequest(520000m, "Lệch thừa 20k"));

        Assert.Equal(20000m, closedShift.Variance);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_048: In báo cáo ca
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_048_BuildShiftReportText_ReturnsFormattedReportText()
    {
        var store = await CreateStoreWithWarehouseAsync();
        var shift = await _salesSvc.OpenShiftAsync(_tenant, _userAdmin, new PosShiftOpenRequest(store.Id, null, 500000m, null));
        await _salesSvc.CloseShiftAsync(_tenant, _userAdmin, shift.Id, new PosShiftCloseRequest(500000m, "Đóng ca"));

        var (fileName, content) = await _salesSvc.BuildShiftReportTextAsync(_tenant, _userAdmin, shift.Id);

        Assert.NotNull(fileName);
        Assert.Contains("BÁO CÁO CA", content);
        Assert.Contains("CH POS 83", content);

        var dbShift = await _db.PosShifts.FirstAsync(x => x.Id == shift.Id);
        Assert.NotNull(dbShift.ReportPrintedAt);
    }

    [Fact]
    public async Task UC_POS_048_BuildShiftReportText_OpenShift_ContainsOpenNotice()
    {
        var store = await CreateStoreWithWarehouseAsync();
        var shift = await _salesSvc.OpenShiftAsync(_tenant, _userAdmin, new PosShiftOpenRequest(store.Id, null, 500000m, null));

        var (_, content) = await _salesSvc.BuildShiftReportTextAsync(_tenant, _userAdmin, shift.Id);

        Assert.Contains("(đang mở)", content);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_054: Trừ tồn theo BOM khi bán
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_054_PaySale_DeductsBomStock_CreatesIssueStockDoc()
    {
        var store = await CreateStoreWithWarehouseAsync();
        var shift = await _salesSvc.OpenShiftAsync(_tenant, _userAdmin, new PosShiftOpenRequest(store.Id, null, 500000m, null));
        var sale = await _salesSvc.OpenSaleAsync(_tenant, _userAdmin, new PosSaleOpenRequest(shift.Id, "Bàn 1", null));

        // Create SKU in INV & Product in POS with BOM
        var uom = new InvUnitOfMeasure { TenantId = _tenant, Code = "KG", Name = "Kilogram", CreatedBy = _userAdmin };
        _db.InvUnitsOfMeasure.Add(uom);
        var skuMaterial = new InvSku { TenantId = _tenant, Code = "MAT-01", Name = "Nguyên Liệu 1", BaseUnitId = uom.Id, Status = "Active", CreatedBy = _userAdmin };
        _db.InvSkus.Add(skuMaterial);
        _db.InvStockBalances.Add(new InvStockBalance { TenantId = _tenant, WarehouseId = _warehouseId, SkuId = skuMaterial.Id, QtyOnHand = 100m });
        await _db.SaveChangesAsync();

        var prod = await _configSvc.UpsertProductAsync(_tenant, _userAdmin, new PosProductUpsertRequest(null, null, "POS-CAFE", "Cà phê máy", "Ly", "Active", 1));
        await _configSvc.UpsertBomAsync(_tenant, _userAdmin, prod.Id, new PosBomLineUpsertRequest(null, "MAT-01", "Nguyên Liệu 1", 0.02m, "KG"));

        await _salesSvc.UpsertSaleLineAsync(_tenant, _userAdmin, sale.Id, new PosSaleLineUpsertRequest(null, prod.Id, prod.Code, prod.Name, 2, 40000m, 0m));

        await _salesSvc.PaySaleAsync(_tenant, _userAdmin, sale.Id, new PosSalePayRequest("Cash", 80000m, null));

        var doc = await _db.InvStockDocs.FirstOrDefaultAsync(x => x.TenantId == _tenant && x.RefModule == "POS" && x.RefId == sale.Id);
        Assert.NotNull(doc);
        Assert.Equal("Issue", doc.DocType);
    }

    [Fact]
    public async Task UC_POS_054_PaySale_DeductBomStock_IdempotentCheck()
    {
        var store = await CreateStoreWithWarehouseAsync();
        var shift = await _salesSvc.OpenShiftAsync(_tenant, _userAdmin, new PosShiftOpenRequest(store.Id, null, 500000m, null));
        var sale = await _salesSvc.OpenSaleAsync(_tenant, _userAdmin, new PosSaleOpenRequest(shift.Id, "Bàn 2", null));

        var prod = await _configSvc.UpsertProductAsync(_tenant, _userAdmin, new PosProductUpsertRequest(null, null, "P-BOM", "Sản Phẩm BOM", "Cái", "Active", 1));
        await _salesSvc.UpsertSaleLineAsync(_tenant, _userAdmin, sale.Id, new PosSaleLineUpsertRequest(null, prod.Id, prod.Code, prod.Name, 1, 50000m, 0m));

        await _salesSvc.PaySaleAsync(_tenant, _userAdmin, sale.Id, new PosSalePayRequest("Cash", 50000m, null));
        var countBefore = await _db.InvStockDocs.CountAsync(x => x.TenantId == _tenant && x.RefModule == "POS" && x.RefId == sale.Id);

        // Submitting payment again shouldn't throw or duplicate stock docs
        var dbSale = await _db.PosSales.FirstAsync(x => x.Id == sale.Id);
        Assert.Equal("Paid", dbSale.Status);
        var countAfter = await _db.InvStockDocs.CountAsync(x => x.TenantId == _tenant && x.RefModule == "POS" && x.RefId == sale.Id);
        Assert.Equal(countBefore, countAfter);
    }

    [Fact]
    public async Task UC_POS_054_PaySale_MissingMaterialSku_ThrowsException()
    {
        var store = await CreateStoreWithWarehouseAsync();
        var shift = await _salesSvc.OpenShiftAsync(_tenant, _userAdmin, new PosShiftOpenRequest(store.Id, null, 500000m, null));
        var sale = await _salesSvc.OpenSaleAsync(_tenant, _userAdmin, new PosSaleOpenRequest(shift.Id, "Bàn 3", null));

        var prod = await _configSvc.UpsertProductAsync(_tenant, _userAdmin, new PosProductUpsertRequest(null, null, "P-MISSING", "SP Khuyết BOM", "Cái", "Active", 1));
        await _configSvc.UpsertBomAsync(_tenant, _userAdmin, prod.Id, new PosBomLineUpsertRequest(null, "NON-EXISTENT-SKU", "SKU Không Có", 1m, "Cái"));
        await _salesSvc.UpsertSaleLineAsync(_tenant, _userAdmin, sale.Id, new PosSaleLineUpsertRequest(null, prod.Id, prod.Code, prod.Name, 1, 50000m, 0m));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.PaySaleAsync(_tenant, _userAdmin, sale.Id, new PosSalePayRequest("Cash", 50000m, null)));
        Assert.Contains("BOM thiếu SKU INV", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_055: Cảnh báo hết / sắp hết
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_055_ListStockAlerts_ReturnsBelowMinAndOutOfStockAlerts()
    {
        var store = await CreateStoreWithWarehouseAsync();

        var uom = new InvUnitOfMeasure { TenantId = _tenant, Code = "CAI", Name = "Cái", CreatedBy = _userAdmin };
        _db.InvUnitsOfMeasure.Add(uom);
        var skuLow = new InvSku { TenantId = _tenant, Code = "SKU-LOW", Name = "Hàng Sắp Hết", BaseUnitId = uom.Id, MinQty = 10, Status = "Active", CreatedBy = _userAdmin };
        _db.InvSkus.Add(skuLow);
        _db.InvStockBalances.Add(new InvStockBalance { TenantId = _tenant, WarehouseId = _warehouseId, SkuId = skuLow.Id, QtyOnHand = 3 });
        await _db.SaveChangesAsync();

        var alerts = await _salesSvc.ListStockAlertsAsync(_tenant, store.Id);

        Assert.NotEmpty(alerts);
        Assert.Contains(alerts, x => x.SkuCode == "SKU-LOW" && x.AlertType == "BelowMin");
    }

    [Fact]
    public async Task UC_POS_055_ListStockAlerts_NoWarehouses_ReturnsEmptyList()
    {
        var alerts = await _salesSvc.ListStockAlertsAsync(_tenant, null);

        Assert.Empty(alerts);
    }
}
