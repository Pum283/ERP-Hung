using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Inv;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Inv;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 94:
///   UC_INV_008 — Import / export danh mục SP (ExportSkusCsvAsync & ImportSkusCsvAsync)
///   UC_INV_011 — Tạo kho (UpsertWarehouseAsync)
///   UC_INV_014 — Gán thủ kho / quyền (UpsertKeeperAsync)
///   UC_INV_015 — Cấu hình FEFO / FIFO (UpsertWarehouseAsync PickPolicy)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class InvStep94PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly InvMasterService _invMaster;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public InvStep94PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("inv-step94-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin94", DisplayName = "Admin 94" });
        _db.SaveChanges();

        _invMaster = new InvMasterService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_008: Import / export danh mục SP
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_008_ExportSkusCsv_ReturnsCsvHeaderAndData()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-EXP-01", "Sản Phẩm Export", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null));

        var csv = await _invMaster.ExportSkusCsvAsync(_tenant);

        Assert.NotNull(csv);
        Assert.Contains("Code,Name", csv);
        Assert.Contains("SKU-EXP-01", csv);
    }

    [Fact]
    public async Task UC_INV_008_ImportSkusCsv_ValidCsv_ImportsSkus()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var csv = $"Code,Name,GroupCode,UomCode\nSKU-IMP-01,Hàng Import 1,,{uom.Code}\nSKU-IMP-02,Hàng Import 2,,{uom.Code}";

        var result = await _invMaster.ImportSkusCsvAsync(_tenant, _userAdmin, new InvImportRequest(csv));

        Assert.NotNull(result);
        Assert.True(result.Success > 0);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_011: Tạo kho
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_011_UpsertWarehouse_ValidRequest_CreatesWarehouse()
    {
        var req = new InvWarehouseUpsertRequest(null, "KHO-HN", "Kho Hà Nội", null, "123 Phố Huế", "Active", "Fefo", false);

        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, req);

        Assert.NotNull(wh);
        Assert.Equal("KHO-HN", wh.Code);
        Assert.Equal("Kho Hà Nội", wh.Name);
    }

    [Fact]
    public async Task UC_INV_011_UpsertWarehouse_EmptyCode_ThrowsException()
    {
        await Assert.ThrowsAsync<AppException>(() =>
            _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "", "Kho Tên", null, null, "Active", null, false)));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_014: Gán thủ kho / quyền
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_014_UpsertKeeper_ValidUser_AssignsKeeperToWarehouse()
    {
        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-HCM", "Kho TP HCM", null, null, "Active", null, false));
        var userKeeper = Guid.NewGuid();
        _db.Users.Add(new AppUser { Id = userKeeper, TenantId = _tenant, Username = "keeper1", DisplayName = "Thủ Kho 1" });
        await _db.SaveChangesAsync();

        var keeper = await _invMaster.UpsertKeeperAsync(_tenant, _userAdmin, wh.Id, new InvWarehouseKeeperUpsertRequest(null, userKeeper, "Keeper", true));

        Assert.NotNull(keeper);
        Assert.Equal("Keeper", keeper.Role);
        Assert.True(keeper.IsActive);

        var detail = await _invMaster.GetWarehouseDetailAsync(_tenant, wh.Id);
        Assert.Single(detail.Keepers);
    }

    [Fact]
    public async Task UC_INV_014_UpsertKeeper_NonExistentWarehouse_ThrowsException()
    {
        await Assert.ThrowsAsync<AppException>(() =>
            _invMaster.UpsertKeeperAsync(_tenant, _userAdmin, Guid.NewGuid(), new InvWarehouseKeeperUpsertRequest(null, Guid.NewGuid(), "Keeper", true)));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_015: Cấu hình FEFO / FIFO
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_015_UpsertWarehouse_PickPolicyFefo_SavesPickingPolicy()
    {
        var req = new InvWarehouseUpsertRequest(null, "KHO-DUOC", "Kho Dược Khẩu", null, null, "Active", "Fefo", false);

        var wh = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, req);

        Assert.Equal("Fefo", wh.PickPolicy);
    }

    [Fact]
    public async Task UC_INV_015_UpsertWarehouse_UpdatePickPolicy_UpdatesPolicy()
    {
        var w1 = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "KHO-SL", "Kho Siêu Thị", null, null, "Active", "Fifo", false));
        var w2 = await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(w1.Id, "KHO-SL", "Kho Siêu Thị", null, null, "Active", "Fefo", false));

        Assert.Equal("Fefo", w2.PickPolicy);
    }

    [Fact]
    public async Task UC_INV_011_ListWarehouses_ReturnsSavedWarehouses()
    {
        await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "K1", "Kho 1", null, null, "Active", null, false));
        await _invMaster.UpsertWarehouseAsync(_tenant, _userAdmin, new InvWarehouseUpsertRequest(null, "K2", "Kho 2", null, null, "Active", null, false));

        var list = await _invMaster.ListWarehousesAsync(_tenant);

        Assert.True(list.Count >= 2);
    }

    [Fact]
    public async Task UC_INV_008_ExportSkusCsv_EmptyCatalog_ReturnsHeaderOnly()
    {
        var csv = await _invMaster.ExportSkusCsvAsync(_tenant);

        Assert.NotNull(csv);
        Assert.StartsWith("Code,Name", csv);
    }
}
