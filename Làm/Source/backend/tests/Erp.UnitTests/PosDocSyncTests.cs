using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Fin;
using Erp.Application.Interfaces.Services.Fin;
using Erp.Domain.Entities.Inv;
using Erp.Domain.Entities.Pos;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Inv;
using Erp.Infrastructure.Implementations.Services.Pos;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.UnitTests;

/// <summary>UC_POS_015/037/048 — sync catalog INV→POS thật · hóa đơn text thật · báo cáo ca thật.</summary>
public sealed class PosDocSyncTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PosConfigService _config;
    private readonly PosSalesService _sales;
    private readonly Guid _tenant = Guid.Parse("cccccccc-dddd-eeee-ffff-000000000000");
    private readonly Guid _user = Guid.Parse("33333333-4444-5555-6666-777777777777");

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

    public PosDocSyncTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pos-doc-sync-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        var noop = new NoopFinRevenue();
        _config = new PosConfigService(_db);
        _sales = new PosSalesService(_db, noop, new InvStockService(_db, noop));
    }

    public void Dispose() => _db.Dispose();

    private InvUnitOfMeasure SeedUom()
    {
        var uom = new InvUnitOfMeasure { TenantId = _tenant, Code = "CAI", Name = "Cái", CreatedBy = _user };
        _db.InvUnitsOfMeasure.Add(uom);
        return uom;
    }

    private InvSku AddSku(Guid uomId, string code, string name, string status = "Active")
    {
        var sku = new InvSku
        {
            TenantId = _tenant, Code = code, Name = name,
            BaseUnitId = uomId, Status = status, CreatedBy = _user,
        };
        _db.InvSkus.Add(sku);
        return sku;
    }

    // ── UC_POS_015 sync catalog INV → POS ──

    [Fact]
    public async Task SyncCatalog_CreatesMissingProductsFromActiveSkus()
    {
        var uom = SeedUom();
        AddSku(uom.Id, "SP-A", "SP A");
        AddSku(uom.Id, "SP-B", "SP B");
        await _db.SaveChangesAsync();

        var r = await _config.SyncCatalogAsync(_tenant, _user);

        Assert.Equal(2, r.CreatedCount);
        Assert.Equal(2, r.ProductCount);
        var p = await _db.PosProducts.SingleAsync(x => x.Code == "SP-A");
        Assert.Equal("SP A", p.Name);
        Assert.Equal("CAI", p.Unit);
        Assert.Equal("Active", p.Status);
        Assert.NotNull(p.SyncedAt);
    }

    [Fact]
    public async Task SyncCatalog_UpdatesRenamedProduct()
    {
        var uom = SeedUom();
        AddSku(uom.Id, "SP-A", "Tên mới back-office");
        _db.PosProducts.Add(new PosProduct { TenantId = _tenant, Code = "SP-A", Name = "Tên cũ", CreatedBy = _user });
        await _db.SaveChangesAsync();

        var r = await _config.SyncCatalogAsync(_tenant, _user);

        Assert.Equal(0, r.CreatedCount);
        Assert.Equal(1, r.UpdatedCount);
        Assert.Equal("Tên mới back-office", (await _db.PosProducts.SingleAsync(x => x.Code == "SP-A")).Name);
    }

    [Fact]
    public async Task SyncCatalog_SuspendsWhenSkuInactive()
    {
        var uom = SeedUom();
        AddSku(uom.Id, "SP-A", "SP A", status: "Inactive");
        _db.PosProducts.Add(new PosProduct { TenantId = _tenant, Code = "SP-A", Name = "SP A", Status = "Active", CreatedBy = _user });
        await _db.SaveChangesAsync();

        var r = await _config.SyncCatalogAsync(_tenant, _user);

        Assert.Equal(1, r.SuspendedCount);
        Assert.Equal("Suspended", (await _db.PosProducts.SingleAsync(x => x.Code == "SP-A")).Status);
    }

    [Fact]
    public async Task SyncCatalog_DoesNotCreateFromInactiveSku()
    {
        var uom = SeedUom();
        AddSku(uom.Id, "SP-X", "SP X", status: "Inactive");
        await _db.SaveChangesAsync();

        var r = await _config.SyncCatalogAsync(_tenant, _user);

        Assert.Equal(0, r.CreatedCount);
        Assert.Equal(0, await _db.PosProducts.CountAsync());
    }

    // ── UC_POS_037 hóa đơn text ──

    private (PosShift Shift, PosSale Sale) SeedPaidSale()
    {
        var store = new PosStore { TenantId = _tenant, Code = "ST1", Name = "Pum's Quận 1", Address = "1 Lê Lợi", CreatedBy = _user };
        _db.PosStores.Add(store);
        _db.Users.Add(new AppUser { Id = _user, TenantId = _tenant, Username = "thungan", DisplayName = "Thu Ngân A" });
        var shift = new PosShift
        {
            TenantId = _tenant, Code = "CA-01", StoreId = store.Id, CashierUserId = _user,
            OpeningCash = 500_000, Status = "Open", CreatedBy = _user,
        };
        _db.PosShifts.Add(shift);
        var sale = new PosSale
        {
            TenantId = _tenant, Code = "HD-001", ShiftId = shift.Id, StoreId = store.Id,
            Status = "Paid", SubTotal = 120_000, DiscountAmount = 20_000, DiscountSource = "Voucher",
            AppliedVoucherCode = "SALE20", TaxAmount = 8_000, TotalAmount = 108_000, PaidAmount = 108_000,
            PaidAt = DateTimeOffset.UtcNow, CreatedBy = _user,
        };
        _db.PosSales.Add(sale);
        _db.PosSaleLines.Add(new PosSaleLine
        {
            TenantId = _tenant, SaleId = sale.Id, ProductCode = "CF01", ProductName = "Cà phê sữa",
            Quantity = 2, UnitPrice = 60_000, LineAmount = 120_000, Status = "Active", LineNo = 1, CreatedBy = _user,
        });
        _db.PosSalePayments.Add(new PosSalePayment
        {
            TenantId = _tenant, SaleId = sale.Id, Code = "PAY-1", Amount = 108_000, Method = "Cash", CreatedBy = _user,
        });
        return (shift, sale);
    }

    [Fact]
    public async Task BuildReceipt_ContainsStoreLinesTotalsAndPayments()
    {
        var (_, sale) = SeedPaidSale();
        await _db.SaveChangesAsync();

        var (fileName, content) = await _sales.BuildReceiptTextAsync(_tenant, _user, sale.Id);

        Assert.Equal("HD-001-hoadon.txt", fileName);
        Assert.Contains("Pum's Quận 1", content);
        Assert.Contains("HÓA ĐƠN BÁN LẺ", content);
        Assert.Contains("Cà phê sữa", content);
        Assert.Contains($"2 x {60_000m:N0}", content);
        Assert.Contains("voucher SALE20", content);
        Assert.Contains("TỔNG CỘNG", content);
        Assert.Contains($"{108_000m:N0}", content);
        Assert.Contains("Cash", content);
        Assert.NotNull((await _db.PosSales.SingleAsync(x => x.Id == sale.Id)).ReceiptPrintedAt);
    }

    [Fact]
    public async Task BuildReceipt_RejectsUnpaidSale()
    {
        var (_, sale) = SeedPaidSale();
        sale.Status = "Open";
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<AppException>(() => _sales.BuildReceiptTextAsync(_tenant, _user, sale.Id));
    }

    // ── UC_POS_048 báo cáo ca ──

    [Fact]
    public async Task BuildShiftReport_ContainsRevenueCashAndVariance()
    {
        var (shift, _) = SeedPaidSale();
        shift.Status = "Closed";
        shift.ClosedAt = DateTimeOffset.UtcNow;
        shift.ExpectedCash = 608_000;
        shift.ClosingCashCounted = 600_000;
        shift.Variance = -8_000;
        await _db.SaveChangesAsync();

        var (fileName, content) = await _sales.BuildShiftReportTextAsync(_tenant, _user, shift.Id);

        Assert.Equal("CA-01-baocao-ca.txt", fileName);
        Assert.Contains("BÁO CÁO CA CA-01", content);
        Assert.Contains("Pum's Quận 1", content);
        Assert.Contains("Thu Ngân A", content);
        Assert.Contains("Đơn đã thanh toán : 1/1", content);
        Assert.Contains($"{108_000m:N0}", content);
        Assert.Contains("Cash", content);
        Assert.Contains($"{500_000m:N0}", content);
        Assert.Contains($"{-8_000m:N0}", content);
        Assert.NotNull((await _db.PosShifts.SingleAsync(x => x.Id == shift.Id)).ReportPrintedAt);
    }

    [Fact]
    public async Task BuildShiftReport_WorksForOpenShiftWithoutSales()
    {
        var store = new PosStore { TenantId = _tenant, Code = "ST2", Name = "Pum's Quận 2", CreatedBy = _user };
        _db.PosStores.Add(store);
        var shift = new PosShift
        {
            TenantId = _tenant, Code = "CA-02", StoreId = store.Id, CashierUserId = _user,
            OpeningCash = 0, Status = "Open", CreatedBy = _user,
        };
        _db.PosShifts.Add(shift);
        await _db.SaveChangesAsync();

        var (_, content) = await _sales.BuildShiftReportTextAsync(_tenant, _user, shift.Id);

        Assert.Contains("Đơn đã thanh toán : 0/0", content);
        Assert.Contains("(đang mở)", content);
    }
}
