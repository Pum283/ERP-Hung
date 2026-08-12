using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Inv;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Inv;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 93:
///   UC_INV_003 — Đơn vị tính & quy đổi (UpsertUomAsync & UpsertConversionAsync)
///   UC_INV_004 — Thuộc tính hàng (lô, serial, HSD) (UpsertSkuAsync flags)
///   UC_INV_005 — Giá vốn / phương pháp tính giá (UpsertSkuAsync CostingMethod)
///   UC_INV_007 — Ngưng sử dụng SKU (SetSkuStatusAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class InvStep93PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly InvMasterService _invMaster;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public InvStep93PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("inv-step93-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin93", DisplayName = "Admin 93" });
        _db.SaveChanges();

        _invMaster = new InvMasterService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_003: Đơn vị tính & quy đổi
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_003_UpsertUom_ValidRequest_CreatesUom()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "KG", "Kilogram", true));

        Assert.NotNull(uom);
        Assert.Equal("KG", uom.Code);
    }

    [Fact]
    public async Task UC_INV_003_UpsertConversion_ValidRate_SavesConversion()
    {
        var uom1 = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "THUNG", "Thùng", true));
        var uom2 = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CHAI", "Chai", true));

        var conv = await _invMaster.UpsertConversionAsync(_tenant, _userAdmin, new InvUnitConversionUpsertRequest(null, uom1.Id, uom2.Id, 24m));

        Assert.NotNull(conv);
        Assert.Equal(24m, conv.Factor);
    }

    [Fact]
    public async Task UC_INV_003_UpsertConversion_ZeroOrNegativeFactor_ThrowsException()
    {
        var uom1 = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "U1", "UOM 1", true));
        var uom2 = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "U2", "UOM 2", true));

        await Assert.ThrowsAsync<AppException>(() =>
            _invMaster.UpsertConversionAsync(_tenant, _userAdmin, new InvUnitConversionUpsertRequest(null, uom1.Id, uom2.Id, 0m)));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_004: Thuộc tính hàng (lô, serial, HSD)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_004_UpsertSku_LotAndExpiryFlags_SavesFlagsCorrectly()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "HOP", "Hộp", true));
        var req = new InvSkuUpsertRequest(null, "SKU-MED", "Thuốc A", null, uom.Id, true, false, true, "FIFO", 20000m, "Active", null, null, null, null);

        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, req);

        Assert.True(sku.TrackLot);
        Assert.False(sku.TrackSerial);
        Assert.True(sku.TrackExpiry);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_005: Giá vốn / phương pháp tính giá
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_005_UpsertSku_CostingMethodFifo_SavesMethod()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var req = new InvSkuUpsertRequest(null, "SKU-FIFO", "SP FIFO", null, uom.Id, false, false, false, "FIFO", 10000m, "Active", null, null, null, null);

        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, req);

        Assert.Equal("FIFO", sku.CostingMethod);
    }

    [Fact]
    public async Task UC_INV_005_UpsertSku_UpdateCostingMethod_UpdatesSuccessfully()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var s1 = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-MA", "SP MA", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null));
        var s2 = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(s1.Id, "SKU-MA", "SP MA", null, uom.Id, false, false, false, "FIFO", 10000m, "Active", null, null, null, null));

        Assert.Equal("FIFO", s2.CostingMethod);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_007: Ngưng sử dụng SKU
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_007_SetSkuStatus_Inactive_DeactivatesSku()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-STOP", "SP Dừng", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null));

        var deactivated = await _invMaster.SetSkuStatusAsync(_tenant, _userAdmin, sku.Id, new InvSkuStatusRequest("Inactive"));

        Assert.Equal("Inactive", deactivated.Status);
    }

    [Fact]
    public async Task UC_INV_007_SetSkuStatus_NonExistentSku_ThrowsException()
    {
        await Assert.ThrowsAsync<AppException>(() =>
            _invMaster.SetSkuStatusAsync(_tenant, _userAdmin, Guid.NewGuid(), new InvSkuStatusRequest("Inactive")));
    }

    [Fact]
    public async Task UC_INV_007_SetSkuStatus_Reactivate_ActivatesSku()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-REACT", "SP Re-Active", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null));
        await _invMaster.SetSkuStatusAsync(_tenant, _userAdmin, sku.Id, new InvSkuStatusRequest("Inactive"));

        var reactivated = await _invMaster.SetSkuStatusAsync(_tenant, _userAdmin, sku.Id, new InvSkuStatusRequest("Active"));

        Assert.Equal("Active", reactivated.Status);
    }

    [Fact]
    public async Task UC_INV_003_ListConversions_ReturnsSavedConversions()
    {
        var uom1 = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "BOX", "Hộp", true));
        var uom2 = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "PCS", "Cái", true));
        await _invMaster.UpsertConversionAsync(_tenant, _userAdmin, new InvUnitConversionUpsertRequest(null, uom1.Id, uom2.Id, 10m));

        var list = await _invMaster.ListConversionsAsync(_tenant);

        Assert.NotEmpty(list);
    }
}
