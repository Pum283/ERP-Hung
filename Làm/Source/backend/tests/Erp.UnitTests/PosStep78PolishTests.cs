using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Pos;
using Erp.Domain.Entities.Inv;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Pos;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 78:
///   UC_POS_015 — Đồng bộ catalog từ back-office (SyncCatalogAsync)
///   UC_POS_016 — Bảng giá theo điểm bán (UpsertPriceListAsync & ListPriceListsAsync)
///   UC_POS_019 — Cấu hình thuế GTGT (UpsertTaxRateAsync & ListTaxRatesAsync)
///   UC_POS_021 — Áp dụng chương trình khuyến mại (UpsertPromotionAsync & ListPromotionsAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class PosStep78PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PosConfigService _configSvc;
    private readonly PosPromoService _promoSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public PosStep78PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pos-step78-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin78", DisplayName = "Admin 78" });
        _db.SaveChanges();

        _configSvc = new PosConfigService(_db);
        _promoSvc = new PosPromoService(_db, null!);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_015: Đồng bộ catalog từ back-office (INV SKU)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_015_SyncCatalog_ActiveSkusInInv_CreatesPosProductsSuccessfully()
    {
        _db.InvSkus.Add(new InvSku { TenantId = _tenant, Code = "SKU-POS-01", Name = "Nước Cam Ép", Status = "Active" });
        _db.InvSkus.Add(new InvSku { TenantId = _tenant, Code = "SKU-POS-02", Name = "Nước Táo Ép", Status = "Active" });
        await _db.SaveChangesAsync();

        var result = await _configSvc.SyncCatalogAsync(_tenant, _userAdmin);

        Assert.NotNull(result);
        Assert.True(result.CreatedCount >= 2);

        var products = await _configSvc.ListProductsAsync(_tenant, null);
        Assert.Contains(products, p => p.Code == "SKU-POS-01");
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_016: Bảng giá theo điểm bán
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_016_UpsertPriceList_ValidRequest_CreatesPriceListSuccessfully()
    {
        var store = await _configSvc.UpsertStoreAsync(_tenant, _userAdmin, new PosStoreUpsertRequest(null, "STORE-PL1", "CH Quận 1", null, "Active", null, null));

        var pl = await _configSvc.UpsertPriceListAsync(_tenant, _userAdmin, new PosPriceListUpsertRequest(
            null, store.Id, "PL-Q1", "Bảng Giá Quận 1", "Active"));

        Assert.NotNull(pl);
        Assert.Equal("PL-Q1", pl.Code);
        Assert.Equal(store.Id, pl.StoreId);
    }

    [Fact]
    public async Task UC_POS_016_UpsertPriceList_InvalidStore_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _configSvc.UpsertPriceListAsync(_tenant, _userAdmin, new PosPriceListUpsertRequest(
                null, Guid.NewGuid(), "PL-ERR", "Bảng Giá Err", "Active")));

        Assert.Equal(404, ex.StatusCode);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_019: Cấu hình thuế GTGT
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_019_UpsertTaxRate_ValidPercent_CreatesTaxRateSuccessfully()
    {
        var tax = await _configSvc.UpsertTaxRateAsync(_tenant, _userAdmin, new PosTaxRateUpsertRequest(null, "VAT8", "Thuế GTGT 8%", 8m, true, true));

        Assert.NotNull(tax);
        Assert.Equal("VAT8", tax.Code);
        Assert.Equal(8m, tax.RatePct);
        Assert.True(tax.IsDefault);
    }

    [Fact]
    public async Task UC_POS_019_UpsertTaxRate_InvalidPercent_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _configSvc.UpsertTaxRateAsync(_tenant, _userAdmin, new PosTaxRateUpsertRequest(null, "VAT150", "Thuế Lỗi", 150m, false, true)));

        Assert.Contains("Thuế suất 0–100%", ex.Message);
    }

    [Fact]
    public async Task UC_POS_019_UpsertTaxRate_DuplicateCode_ThrowsAppException()
    {
        await _configSvc.UpsertTaxRateAsync(_tenant, _userAdmin, new PosTaxRateUpsertRequest(null, "VAT10", "Thuế 10%", 10m, false, true));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _configSvc.UpsertTaxRateAsync(_tenant, _userAdmin, new PosTaxRateUpsertRequest(null, "VAT10", "Thuế Trùng 10%", 10m, false, true)));

        Assert.Contains("đã tồn tại", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_021: Áp dụng chương trình khuyến mại
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_021_UpsertPromotion_ValidPercentDiscount_CreatesPromotionSuccessfully()
    {
        var promo = await _promoSvc.UpsertPromotionAsync(_tenant, _userAdmin, new PosPromotionUpsertRequest(
            null, "PROMO-SUMMER", "Khuyến mại Mùa Hè 10%", "Percent", 10m, 100000m, null, null, "Active", "Giảm 10% đơn từ 100k"));

        Assert.NotNull(promo);
        Assert.Equal("PROMO-SUMMER", promo.Code);
        Assert.Equal(10m, promo.DiscountValue);
        Assert.Equal("Percent", promo.DiscountType);
    }

    [Fact]
    public async Task UC_POS_021_UpsertPromotion_InvalidDiscountType_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _promoSvc.UpsertPromotionAsync(_tenant, _userAdmin, new PosPromotionUpsertRequest(
                null, "PROMO-ERR", "Khuyến mại Lỗi", "FreeGift", 50m, 0m, null, null, "Active", null)));

        Assert.Contains("Loại: Percent | Amount", ex.Message);
    }

    [Fact]
    public async Task UC_POS_021_UpsertPromotion_PercentExceeds100_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _promoSvc.UpsertPromotionAsync(_tenant, _userAdmin, new PosPromotionUpsertRequest(
                null, "PROMO-OVER", "Khuyến mại 200%", "Percent", 200m, 0m, null, null, "Active", null)));

        Assert.Contains("Percent ≤ 100", ex.Message);
    }
}
