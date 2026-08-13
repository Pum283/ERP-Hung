using Erp.Application.DTOs;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class PurBlacklistImportLegalPricelistPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PurBlacklistImportLegalPricelistService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _supplierId = Guid.NewGuid();

    public PurBlacklistImportLegalPricelistPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pur-blacklist-import-legal-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "TPUR186", Name = "Tenant PUR 186" });
        _db.SaveChanges();

        _svc = new PurBlacklistImportLegalPricelistService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_006: Blacklist / ngưng dùng
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task BlacklistSupplier_SuspendsSupplierSuccessfully()
    {
        var req = new PurBlacklistSupplierRequest(_supplierId, "Tỷ lệ hàng giao lỗi quá cao > 5%", "6");
        var res = await _svc.BlacklistSupplierAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.True(res.IsBlacklisted);
        Assert.Equal("Blacklisted", res.Status);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_007: Import danh sách nhà cung cấp
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ImportSuppliersBatch_ProcessesBatchImportRows()
    {
        var rows = new List<PurImportSupplierRowDto>
        {
            new("SUP-010", "Công Ty Nông Sản Sạch An Giang", "0312999888", "0909123456", "contact@angiang.vn", "CAT-FOOD"),
            new("SUP-011", "Công Ty Bao Bì Giấy Hùng Phát", "0312777666", "0908765432", "info@hungphat.vn", "CAT-PACKAGING")
        };

        var req = new PurBatchImportSuppliersRequest(rows);
        var res = await _svc.ImportSuppliersBatchAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(2, res.TotalProcessed);
        Assert.Equal(2, res.TotalSuccess);
        Assert.Equal(0, res.TotalFailed);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_008: Hồ sơ pháp lý
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveSupplierLegalDocument_SavesDocumentAndDeterminesStatus()
    {
        var req = new PurSaveSupplierLegalDocumentRequest(
            _supplierId,
            "BusinessLicense",
            "GPKD-0318889999",
            DateTimeOffset.UtcNow.AddYears(-1),
            DateTimeOffset.UtcNow.AddYears(4),
            "https://cdn.erp.com/docs/gpkd.pdf"
        );

        var res = await _svc.SaveSupplierLegalDocumentAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("GPKD-0318889999", res.DocumentNumber);
        Assert.Equal("Valid", res.Status);

        var list = await _svc.GetSupplierLegalDocumentsAsync(_tenant, _supplierId);
        Assert.NotEmpty(list);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_011: Hiệu lực bảng giá mua
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SavePurchasePricelistValidity_SavesPricelistPeriod()
    {
        var items = new List<PurPricelistItemDto>
        {
            new(Guid.NewGuid(), "SKU-MILK", "Sữa Tươi 1L", 24500m)
        };

        var req = new PurSavePurchasePricelistValidityRequest(
            _supplierId,
            "PL-PUR-2026-H2",
            "Bảng giá ưu đãi 6 tháng cuối năm 2026",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMonths(6),
            items
        );

        var res = await _svc.SavePurchasePricelistValidityAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("PL-PUR-2026-H2", res.PricelistCode);
        Assert.True(res.IsActive);
        Assert.Single(res.Items);

        var list = await _svc.GetPurchasePricelistsAsync(_tenant, _supplierId);
        Assert.NotEmpty(list);
    }
}
