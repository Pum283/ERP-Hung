using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Pos;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Pos;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 77:
///   UC_POS_009 — Danh mục nhóm sản phẩm (UpsertCategoryAsync & ListCategoriesAsync)
///   UC_POS_010 — Danh mục sản phẩm bán (UpsertProductAsync & ListProductsAsync)
///   UC_POS_012 — BOM / định mức nguyên liệu (UpsertBomAsync & ListBomAsync)
///   UC_POS_014 — Ngưng bán sản phẩm tạm thời (SetProductStatusAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class PosStep77PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PosConfigService _configSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public PosStep77PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pos-step77-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin77", DisplayName = "Admin 77" });
        _db.SaveChanges();

        _configSvc = new PosConfigService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_009: Danh mục nhóm sản phẩm
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_009_UpsertCategory_ValidRequest_CreatesCategorySuccessfully()
    {
        var cat = await _configSvc.UpsertCategoryAsync(_tenant, _userAdmin, new PosCategoryUpsertRequest(null, "CAT-BEV", "Đồ Uống", 1, true));

        Assert.NotNull(cat);
        Assert.Equal("CAT-BEV", cat.Code);
        Assert.Equal("Đồ Uống", cat.Name);
        Assert.True(cat.IsActive);
    }

    [Fact]
    public async Task UC_POS_009_UpsertCategory_DuplicateCode_ThrowsAppException()
    {
        await _configSvc.UpsertCategoryAsync(_tenant, _userAdmin, new PosCategoryUpsertRequest(null, "CAT-DUP", "Nhóm 1", 1, true));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _configSvc.UpsertCategoryAsync(_tenant, _userAdmin, new PosCategoryUpsertRequest(null, "CAT-DUP", "Nhóm 2", 2, true)));

        Assert.Contains("đã tồn tại", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_010: Danh mục sản phẩm bán
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_010_UpsertProduct_ValidCategory_CreatesProductSuccessfully()
    {
        var cat = await _configSvc.UpsertCategoryAsync(_tenant, _userAdmin, new PosCategoryUpsertRequest(null, "CAT-FOOD", "Đồ Ăn", 2, true));
        var p = await _configSvc.UpsertProductAsync(_tenant, _userAdmin, new PosProductUpsertRequest(null, cat.Id, "FOOD-01", "Bánh Mì Thịt", "Ổ", "Active", 1));

        Assert.NotNull(p);
        Assert.Equal("FOOD-01", p.Code);
        Assert.Equal(cat.Id, p.CategoryId);
        Assert.Equal("Active", p.Status);
    }

    [Fact]
    public async Task UC_POS_010_UpsertProduct_NonExistentCategory_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _configSvc.UpsertProductAsync(_tenant, _userAdmin, new PosProductUpsertRequest(null, Guid.NewGuid(), "PROD-99", "SP 99", "Cái", "Active", 1)));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC_POS_010_UpsertProduct_InvalidStatus_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _configSvc.UpsertProductAsync(_tenant, _userAdmin, new PosProductUpsertRequest(null, null, "PROD-ERR", "SP Lỗi", "Cái", "InvalidStatus", 1)));

        Assert.Contains("Trạng thái SP không hợp lệ", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_012: BOM / định mức nguyên liệu
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_012_UpsertBom_ValidProduct_CreatesBomLineSuccessfully()
    {
        var p = await _configSvc.UpsertProductAsync(_tenant, _userAdmin, new PosProductUpsertRequest(null, null, "DRINK-CF", "Cà Phê Sữa", "Ly", "Active", 1));
        var bom = await _configSvc.UpsertBomAsync(_tenant, _userAdmin, p.Id, new PosBomLineUpsertRequest(null, "MAT-COFFEE", "Bột Cà Phê", 0.02m, "kg"));

        Assert.NotNull(bom);
        Assert.Equal("MAT-COFFEE", bom.MaterialCode);
        Assert.Equal(0.02m, bom.Qty);
    }

    [Fact]
    public async Task UC_POS_012_UpsertBom_ZeroQty_ThrowsAppException()
    {
        var p = await _configSvc.UpsertProductAsync(_tenant, _userAdmin, new PosProductUpsertRequest(null, null, "DRINK-TEA", "Trà Chanh", "Ly", "Active", 2));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _configSvc.UpsertBomAsync(_tenant, _userAdmin, p.Id, new PosBomLineUpsertRequest(null, "MAT-LEMON", "Chanh Tươi", 0m, "qua")));

        Assert.Contains("Định mức phải > 0", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_014: Ngưng bán sản phẩm tạm thời
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_014_SetProductStatus_ActiveToSuspended_UpdatesStatusSuccessfully()
    {
        var p = await _configSvc.UpsertProductAsync(_tenant, _userAdmin, new PosProductUpsertRequest(null, null, "PROD-SUS", "Món Tạm Hết", "Dĩa", "Active", 1));

        var updated = await _configSvc.SetProductStatusAsync(_tenant, _userAdmin, p.Id, "Suspended");

        Assert.Equal("Suspended", updated.Status);
    }

    [Fact]
    public async Task UC_POS_014_SetProductStatus_InvalidStatus_ThrowsAppException()
    {
        var p = await _configSvc.UpsertProductAsync(_tenant, _userAdmin, new PosProductUpsertRequest(null, null, "PROD-SUS2", "Món Tạm Hết 2", "Dĩa", "Active", 2));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _configSvc.SetProductStatusAsync(_tenant, _userAdmin, p.Id, "DeletedPermanent"));

        Assert.Contains("Trạng thái SP không hợp lệ", ex.Message);
    }

    [Fact]
    public async Task UC_POS_014_SetProductStatus_NonExistentProduct_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _configSvc.SetProductStatusAsync(_tenant, _userAdmin, Guid.NewGuid(), "Suspended"));

        Assert.Equal(404, ex.StatusCode);
    }
}
