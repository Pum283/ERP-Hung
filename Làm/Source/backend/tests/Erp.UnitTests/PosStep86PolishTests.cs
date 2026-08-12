using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Pos;
using Erp.Application.DTOs.Pur;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Pos;
using Erp.Infrastructure.Implementations.Services.Pur;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 86:
///   UC_POS_068 — Xuất báo cáo POS CSV (ExportCsvAsync)
///   UC_POS_069 — Giám sát doanh thu chuỗi realtime (ChainLiveAsync)
///   UC_POS_072 — Cấu hình target doanh thu (UpsertStoreAsync - MonthlyRevenueTarget)
///   UC_PUR_001 — Tạo / cập nhật nhà cung cấp (UpsertVendorAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class PosStep86PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PosConfigService _posConfig;
    private readonly PosReportService _posReport;
    private readonly PurPurchasingService _purPurchasing;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public PosStep86PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pos-step86-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin86", DisplayName = "Admin 86" });
        _db.SaveChanges();

        _posConfig = new PosConfigService(_db);
        _posReport = new PosReportService(_db);
        _purPurchasing = new PurPurchasingService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_068: Xuất báo cáo POS CSV
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_068_ExportCsv_ValidRange_ReturnsCsvText()
    {
        var from = DateTimeOffset.UtcNow.AddDays(-1);
        var to = DateTimeOffset.UtcNow.AddDays(1);

        var csv = await _posReport.ExportCsvAsync(_tenant, "product", from, to);

        Assert.NotNull(csv);
        Assert.Contains("ProductCode", csv);
    }

    [Fact]
    public async Task UC_POS_068_ExportCsv_InvalidDateRange_ThrowsException()
    {
        var from = DateTimeOffset.UtcNow.AddDays(5);
        var to = DateTimeOffset.UtcNow.AddDays(1);

        await Assert.ThrowsAsync<AppException>(() =>
            _posReport.ExportCsvAsync(_tenant, "by-product", from, to));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_069: Giám sát doanh thu chuỗi realtime
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_069_ChainLive_ReturnsChainSummaryAndRows()
    {
        await _posConfig.UpsertStoreAsync(_tenant, _userAdmin, new PosStoreUpsertRequest(null, "ST-CHAIN-1", "CH Chuỗi 1", null, "Active", null, 100000000m));
        await _posConfig.UpsertStoreAsync(_tenant, _userAdmin, new PosStoreUpsertRequest(null, "ST-CHAIN-2", "CH Chuỗi 2", null, "Active", null, 200000000m));

        var live = await _posReport.ChainLiveAsync(_tenant);

        Assert.NotNull(live);
        Assert.Equal(2, live.StoreCount);
        Assert.Equal(300000000m, live.TotalTarget);
    }

    [Fact]
    public async Task UC_POS_069_ChainLive_NoActiveStores_ReturnsEmptyReport()
    {
        var live = await _posReport.ChainLiveAsync(_tenant);

        Assert.Equal(0, live.StoreCount);
        Assert.Equal(0m, live.TotalMonthRevenue);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_072: Cấu hình target doanh thu
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_072_UpsertStore_WithMonthlyTarget_SavesTarget()
    {
        var store = await _posConfig.UpsertStoreAsync(_tenant, _userAdmin, new PosStoreUpsertRequest(null, "ST-TARGET", "CH Target 500M", null, "Active", null, 500000000m));

        Assert.NotNull(store);
        Assert.Equal(500000000m, store.MonthlyRevenueTarget);
    }

    [Fact]
    public async Task UC_POS_072_UpsertStore_UpdateTarget_UpdatesExistingTarget()
    {
        var store = await _posConfig.UpsertStoreAsync(_tenant, _userAdmin, new PosStoreUpsertRequest(null, "ST-TGT2", "CH Target 100M", null, "Active", null, 100000000m));
        var updated = await _posConfig.UpsertStoreAsync(_tenant, _userAdmin, new PosStoreUpsertRequest(store.Id, "ST-TGT2", "CH Target 150M", null, "Active", null, 150000000m));

        Assert.Equal(150000000m, updated.MonthlyRevenueTarget);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_PUR_001: Tạo / cập nhật nhà cung cấp
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_PUR_001_UpsertVendor_ValidNewVendor_CreatesVendor()
    {
        var req = new PurVendorUpsertRequest(null, "NCC-01", "Nhà Cung Cấp A", null, null, null, null, null, "Active");

        var vendor = await _purPurchasing.UpsertVendorAsync(_tenant, _userAdmin, req);

        Assert.NotNull(vendor);
        Assert.Equal("NCC-01", vendor.Code);
        Assert.Equal("Nhà Cung Cấp A", vendor.Name);
    }

    [Fact]
    public async Task UC_PUR_001_UpsertVendor_UpdateVendorInfo_UpdatesSuccessfully()
    {
        var v1 = await _purPurchasing.UpsertVendorAsync(_tenant, _userAdmin, new PurVendorUpsertRequest(null, "NCC-02", "NCC Tên Cũ", null, null, null, null, null, "Active"));
        var v2 = await _purPurchasing.UpsertVendorAsync(_tenant, _userAdmin, new PurVendorUpsertRequest(v1.Id, "NCC-02", "NCC Tên Mới", "0108887776", null, null, null, "Net60", "Active"));

        Assert.Equal("NCC Tên Mới", v2.Name);
        Assert.Equal("Net60", v2.PaymentTerms);
    }

    [Fact]
    public async Task UC_PUR_001_UpsertVendor_EmptyCodeOrName_ThrowsException()
    {
        await Assert.ThrowsAsync<AppException>(() =>
            _purPurchasing.UpsertVendorAsync(_tenant, _userAdmin, new PurVendorUpsertRequest(null, "", "Tên NCC", null, null, null, null, null, "Active")));
    }

    [Fact]
    public async Task UC_PUR_001_ListVendors_FilterBySearchQuery_ReturnsMatchingVendors()
    {
        await _purPurchasing.UpsertVendorAsync(_tenant, _userAdmin, new PurVendorUpsertRequest(null, "NCC-THP", "Tân Hiệp Phát", null, null, null, null, null, "Active"));
        await _purPurchasing.UpsertVendorAsync(_tenant, _userAdmin, new PurVendorUpsertRequest(null, "NCC-VNM", "Vinamilk", null, null, null, null, null, "Active"));

        var list = await _purPurchasing.ListVendorsAsync(_tenant, "VNM");

        Assert.Single(list);
        Assert.Equal("NCC-VNM", list[0].Code);
    }
}
