using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Fin;
using Erp.Application.DTOs.Pos;
using Erp.Application.Interfaces.Services.Fin;
using Erp.Domain.Entities.Inv;
using Erp.Domain.Entities.Pos;
using Erp.Infrastructure.Implementations.Services.Inv;
using Erp.Infrastructure.Implementations.Services.Pos;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.UnitTests;

file sealed class FakeFinRevenue : IFinRevenueService
{
    private static FinRevenueDocumentDto Doc() => new(
        Guid.NewGuid(), "REV-X", "PosSale", "POS", null, null,
        DateTimeOffset.UtcNow, 0, 0, 0, 0,
        null, null, null, null, null, null, null, null, "Draft", null, null);

    public Task<IReadOnlyList<FinRevenueDocumentDto>> ListAsync(
        Guid tenantId, string? kind = null, Guid? periodId = null, string? status = null,
        CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<FinRevenueDocumentDto>>([]);

    public Task<FinRevenueSummaryDto> GetSummaryAsync(
        Guid tenantId, Guid? periodId = null, CancellationToken ct = default)
        => Task.FromResult(new FinRevenueSummaryDto(null, null, 0, 0, 0, 0, 0, 0, 0, 0, 0));

    public Task<FinRevenueDocumentDto> RecognizeFromPosAsync(
        Guid tenantId, Guid userId, Guid saleId, FinRevenueRecognizeRequest? req = null,
        CancellationToken ct = default)
        => Task.FromResult(Doc());

    public Task<FinRevenueDocumentDto> RecognizeFromSalesOrderAsync(
        Guid tenantId, Guid userId, Guid orderId, FinRevenueRecognizeRequest? req = null,
        CancellationToken ct = default)
        => Task.FromResult(Doc());

    public Task<FinRevenueDocumentDto> RecognizeFromArInvoiceAsync(
        Guid tenantId, Guid userId, Guid arInvoiceId, FinRevenueRecognizeRequest? req = null,
        CancellationToken ct = default)
        => Task.FromResult(Doc());

    public Task<FinRevenueDocumentDto> RecognizeCogsAsync(
        Guid tenantId, Guid userId, Guid invStockDocId, FinRevenueRecognizeRequest? req = null,
        CancellationToken ct = default)
        => Task.FromResult(Doc());

    public Task<FinRevenueDocumentDto> VoidAsync(
        Guid tenantId, Guid userId, Guid id, string? note = null, CancellationToken ct = default)
        => Task.FromResult(Doc());
}

/// <summary>POS Cap-2 BOM deduct + stock alerts (UC_POS_054/055) — EF InMemory.</summary>
public sealed class PosBomStockTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PosSalesService _sales;
    private readonly Guid _tenant = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private readonly Guid _user = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    public PosBomStockTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pos-bom-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        var fin = new FakeFinRevenue();
        var stock = new InvStockService(_db, fin);
        _sales = new PosSalesService(_db, fin, stock);
    }

    public void Dispose() => _db.Dispose();

    private async Task<(PosStore store, InvWarehouse wh, PosProduct product, PosShift shift, InvSku sku)> SeedAsync()
    {
        var unit = new InvUnitOfMeasure { TenantId = _tenant, Code = "CAI", Name = "Cái", IsActive = true };
        _db.InvUnitsOfMeasure.Add(unit);
        var wh = new InvWarehouse
        {
            TenantId = _tenant, Code = "WH-POS", Name = "Kho POS", Status = "Active",
            AllowNegativeStock = true,
        };
        _db.InvWarehouses.Add(wh);
        var sku = new InvSku
        {
            TenantId = _tenant, Code = "NVL-A", Name = "Nguyên liệu A", Status = "Active",
            MinQty = 10, ReorderQty = 20, BaseUnitId = unit.Id,
        };
        _db.InvSkus.Add(sku);
        _db.InvStockBalances.Add(new InvStockBalance
        {
            TenantId = _tenant, WarehouseId = wh.Id, SkuId = sku.Id, QtyOnHand = 100,
        });
        var store = new PosStore
        {
            TenantId = _tenant, Code = "ST1", Name = "Store 1", Status = "Active", WarehouseId = wh.Id,
        };
        _db.PosStores.Add(store);
        var product = new PosProduct
        {
            TenantId = _tenant, Code = "SP1", Name = "Sản phẩm 1", Status = "Active",
        };
        _db.PosProducts.Add(product);
        _db.PosBomLines.Add(new PosBomLine
        {
            TenantId = _tenant, ProductId = product.Id,
            MaterialCode = "NVL-A", MaterialName = "NVL A", Qty = 2, Unit = "cai",
        });
        var shift = new PosShift
        {
            TenantId = _tenant, Code = "SH-1", StoreId = store.Id, CashierUserId = _user,
            Status = "Open", OpenedAt = DateTimeOffset.UtcNow, OpeningCash = 0, CreatedBy = _user,
        };
        _db.PosShifts.Add(shift);
        await _db.SaveChangesAsync();
        return (store, wh, product, shift, sku);
    }

    [Fact]
    public async Task PaySale_DeductsBomViaInvIssue()
    {
        var (_, wh, product, shift, sku) = await SeedAsync();
        var sale = await _sales.OpenSaleAsync(_tenant, _user, new PosSaleOpenRequest(shift.Id, null, null));
        await _sales.UpsertSaleLineAsync(_tenant, _user, sale.Id, new PosSaleLineUpsertRequest(
            null, product.Id, null, null, 3, 50_000, 0));
        var detail = await _sales.GetSaleDetailAsync(_tenant, sale.Id);
        await _sales.PaySaleAsync(_tenant, _user, sale.Id, new PosSalePayRequest("Cash", detail.Sale.TotalAmount, null));

        var issue = await _db.InvStockDocs.FirstOrDefaultAsync(
            x => x.TenantId == _tenant && x.RefModule == "POS" && x.RefId == sale.Id && x.DocType == "Issue");
        Assert.NotNull(issue);
        Assert.Equal("Posted", issue!.Status);
        Assert.Equal("Sales", issue.SourceType);
        Assert.Equal(wh.Id, issue.WarehouseId);

        var line = await _db.InvStockDocLines.FirstAsync(x => x.DocId == issue.Id);
        Assert.Equal(sku.Id, line.SkuId);
        Assert.Equal(6m, line.Qty);

        var onHand = await _db.InvStockBalances.Where(x => x.SkuId == sku.Id && x.WarehouseId == wh.Id)
            .SumAsync(x => x.QtyOnHand);
        Assert.Equal(94m, onHand);
    }

    [Fact]
    public async Task PaySale_IdempotentBomDeduct()
    {
        var (_, _, product, shift, _) = await SeedAsync();
        var sale = await _sales.OpenSaleAsync(_tenant, _user, new PosSaleOpenRequest(shift.Id, null, null));
        await _sales.UpsertSaleLineAsync(_tenant, _user, sale.Id, new PosSaleLineUpsertRequest(
            null, product.Id, null, null, 1, 10_000, 0));
        var detail = await _sales.GetSaleDetailAsync(_tenant, sale.Id);
        await _sales.PaySaleAsync(_tenant, _user, sale.Id, new PosSalePayRequest("Cash", detail.Sale.TotalAmount, null));
        Assert.Equal(1, await _db.InvStockDocs.CountAsync(x => x.RefModule == "POS" && x.RefId == sale.Id));
    }

    [Fact]
    public async Task PaySale_NoBom_SkipsIssue()
    {
        var (_, _, _, shift, _) = await SeedAsync();
        var product = new PosProduct
        {
            TenantId = _tenant, Code = "SP-NOBOM", Name = "No BOM", Status = "Active",
        };
        _db.PosProducts.Add(product);
        await _db.SaveChangesAsync();
        var sale = await _sales.OpenSaleAsync(_tenant, _user, new PosSaleOpenRequest(shift.Id, null, null));
        await _sales.UpsertSaleLineAsync(_tenant, _user, sale.Id, new PosSaleLineUpsertRequest(
            null, product.Id, null, null, 1, 5_000, 0));
        var detail = await _sales.GetSaleDetailAsync(_tenant, sale.Id);
        await _sales.PaySaleAsync(_tenant, _user, sale.Id, new PosSalePayRequest("Cash", detail.Sale.TotalAmount, null));
        Assert.Equal(0, await _db.InvStockDocs.CountAsync(x => x.RefModule == "POS" && x.RefId == sale.Id));
    }

    [Fact]
    public async Task PaySale_MissingSku_Throws()
    {
        var (_, _, product, shift, _) = await SeedAsync();
        var bom = await _db.PosBomLines.FirstAsync(x => x.ProductId == product.Id);
        bom.MaterialCode = "UNKNOWN";
        await _db.SaveChangesAsync();
        var sale = await _sales.OpenSaleAsync(_tenant, _user, new PosSaleOpenRequest(shift.Id, null, null));
        await _sales.UpsertSaleLineAsync(_tenant, _user, sale.Id, new PosSaleLineUpsertRequest(
            null, product.Id, null, null, 1, 10_000, 0));
        var detail = await _sales.GetSaleDetailAsync(_tenant, sale.Id);
        await Assert.ThrowsAsync<AppException>(() =>
            _sales.PaySaleAsync(_tenant, _user, sale.Id, new PosSalePayRequest("Cash", detail.Sale.TotalAmount, null)));
    }

    [Fact]
    public async Task StockAlerts_BelowMin()
    {
        var (store, wh, _, _, sku) = await SeedAsync();
        sku.MinQty = 50;
        sku.ReorderQty = 80;
        var bal = await _db.InvStockBalances.FirstAsync(x => x.SkuId == sku.Id);
        bal.QtyOnHand = 40;
        await _db.SaveChangesAsync();

        var alerts = await _sales.ListStockAlertsAsync(_tenant, store.Id);
        Assert.Contains(alerts, a => a.SkuId == sku.Id && a.AlertType == "BelowMin");
        Assert.Equal(wh.Id, alerts[0].WarehouseId);
    }

    [Fact]
    public async Task StockAlerts_OutOfStock()
    {
        var (store, _, _, _, sku) = await SeedAsync();
        sku.MinQty = null;
        sku.ReorderQty = null;
        var bal = await _db.InvStockBalances.FirstAsync(x => x.SkuId == sku.Id);
        bal.QtyOnHand = 0;
        await _db.SaveChangesAsync();
        var alerts = await _sales.ListStockAlertsAsync(_tenant, store.Id);
        Assert.Contains(alerts, a => a.AlertType == "OutOfStock");
    }

    [Fact]
    public async Task StockAlerts_EmptyWhenHealthy()
    {
        var (store, _, _, _, sku) = await SeedAsync();
        sku.MinQty = 1;
        sku.ReorderQty = 2;
        await _db.SaveChangesAsync();
        var alerts = await _sales.ListStockAlertsAsync(_tenant, store.Id);
        Assert.Empty(alerts);
    }

    [Fact]
    public async Task UpsertStore_WarehouseReflectedInDto()
    {
        var cfg = new PosConfigService(_db);
        var wh = new InvWarehouse
        {
            TenantId = _tenant, Code = "WH2", Name = "Kho 2", Status = "Active",
        };
        _db.InvWarehouses.Add(wh);
        await _db.SaveChangesAsync();
        var dto = await cfg.UpsertStoreAsync(_tenant, _user, new PosStoreUpsertRequest(
            null, "ST-WH", "Store WH", null, "Active", wh.Id));
        Assert.Equal(wh.Id, dto.WarehouseId);
        Assert.Equal("Kho 2", dto.WarehouseName);
    }
}
