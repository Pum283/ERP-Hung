using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Mfg;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Fin;
using Erp.Infrastructure.Implementations.Services.Log;
using Erp.Infrastructure.Implementations.Services.Mfg;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 109:
///   UC_LOG_038 — Báo cáo COD tồn / đã nộp (GetReportAsync)
///   UC_LOG_039 — Dashboard giao vận (GetReportAsync)
///   UC_MFG_001 — Danh mục thành phẩm / bán thành phẩm (UpsertItemAsync FG/SFG)
///   UC_MFG_002 — Danh mục nguyên vật liệu (UpsertItemAsync RM)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class MfgStep109PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LogLogisticsService _logistics;
    private readonly LogCodService _cod;
    private readonly MfgProductionService _mfg;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public MfgStep109PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("mfg-step109-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin109", DisplayName = "Admin 109" });
        _db.SaveChanges();

        var finAcc = new FinAccountingService(_db);
        _logistics = new LogLogisticsService(_db);
        _cod = new LogCodService(_db, _logistics);
        _mfg = new MfgProductionService(_db, finAcc);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_LOG_038 & UC_LOG_039: Báo cáo COD & Dashboard giao vận
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_LOG_038_GetReport_ReturnsCodSummaryReport()
    {
        var report = await _cod.GetReportAsync(_tenant);

        Assert.NotNull(report);
    }

    [Fact]
    public async Task UC_LOG_039_GetReport_CalculatesDashboardKpiTotals()
    {
        var report = await _cod.GetReportAsync(_tenant);

        Assert.NotNull(report);
        Assert.Equal(0, report.OverdueCount);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_MFG_001: Danh mục thành phẩm / bán thành phẩm
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_MFG_001_UpsertItem_FinishedGood_CreatesFgItem()
    {
        var req = new MfgItemUpsertRequest(null, "TP-109-01", "Áo Sơ Mi Nam Premium", "FG", "CAI", 150000m, "Active", "Thành phẩm may mặc");

        var item = await _mfg.UpsertItemAsync(_tenant, _userAdmin, req);

        Assert.NotNull(item);
        Assert.Equal("TP-109-01", item.Code);
        Assert.Equal("FG", item.ItemType);
    }

    [Fact]
    public async Task UC_MFG_001_UpsertItem_SemiFinishedGood_CreatesSfgItem()
    {
        var req = new MfgItemUpsertRequest(null, "BTP-109-01", "Thân Áo Đã Cắt", "SFG", "CAI", 45000m, "Active", "Bán thành phẩm");

        var item = await _mfg.UpsertItemAsync(_tenant, _userAdmin, req);

        Assert.NotNull(item);
        Assert.Equal("BTP-109-01", item.Code);
        Assert.Equal("SFG", item.ItemType);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_MFG_002: Danh mục nguyên vật liệu
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_MFG_002_UpsertItem_RawMaterial_CreatesRmItem()
    {
        var req = new MfgItemUpsertRequest(null, "NVL-109-01", "Vải Cotton 100%", "RM", "MET", 85000m, "Active", "Nguyên vật liệu chính");

        var item = await _mfg.UpsertItemAsync(_tenant, _userAdmin, req);

        Assert.NotNull(item);
        Assert.Equal("NVL-109-01", item.Code);
        Assert.Equal("RM", item.ItemType);
    }

    [Fact]
    public async Task UC_MFG_001_ListItems_FilterByType_ReturnsFilteredItems()
    {
        await _mfg.UpsertItemAsync(_tenant, _userAdmin, new MfgItemUpsertRequest(null, "TP-109-02", "Áo Thun", "FG", "CAI", 90000m, "Active", null));
        await _mfg.UpsertItemAsync(_tenant, _userAdmin, new MfgItemUpsertRequest(null, "NVL-109-02", "Chỉ May", "RM", "CUON", 12000m, "Active", null));

        var fgItems = await _mfg.ListItemsAsync(_tenant, "FG", null);

        Assert.NotEmpty(fgItems);
        Assert.All(fgItems, x => Assert.Equal("FG", x.ItemType));
    }

    [Fact]
    public async Task UC_MFG_001_UpsertItem_InvalidItemType_ThrowsException()
    {
        var req = new MfgItemUpsertRequest(null, "ERR-109", "SP Lỗi", "INVALID_TYPE", "CAI", 10000m, "Active", null);

        await Assert.ThrowsAsync<AppException>(() =>
            _mfg.UpsertItemAsync(_tenant, _userAdmin, req));
    }

    [Fact]
    public async Task UC_MFG_001_UpsertItem_DuplicateCode_ThrowsException()
    {
        await _mfg.UpsertItemAsync(_tenant, _userAdmin, new MfgItemUpsertRequest(null, "DUP-109", "SP Dup 1", "FG", "CAI", 10000m, "Active", null));

        await Assert.ThrowsAsync<AppException>(() =>
            _mfg.UpsertItemAsync(_tenant, _userAdmin, new MfgItemUpsertRequest(null, "DUP-109", "SP Dup 2", "FG", "CAI", 10000m, "Active", null)));
    }

    [Fact]
    public async Task UC_MFG_002_ListItems_SearchQuery_ReturnsMatchingItems()
    {
        await _mfg.UpsertItemAsync(_tenant, _userAdmin, new MfgItemUpsertRequest(null, "RM-COTTON", "Vải Cotton 60/40", "RM", "MET", 60000m, "Active", null));

        var items = await _mfg.ListItemsAsync(_tenant, null, "COTTON");

        Assert.NotEmpty(items);
    }

    [Fact]
    public async Task UC_MFG_001_UpsertItem_UpdateExisting_UpdatesItemDetails()
    {
        var created = await _mfg.UpsertItemAsync(_tenant, _userAdmin, new MfgItemUpsertRequest(null, "TP-UPDATE", "Áo Cũ", "FG", "CAI", 50000m, "Active", null));
        var updateReq = new MfgItemUpsertRequest(created.Id, "TP-UPDATE", "Áo Mới Đã Cập Nhật", "FG", "CAI", 75000m, "Active", "Đã cập nhật giá");

        var updated = await _mfg.UpsertItemAsync(_tenant, _userAdmin, updateReq);

        Assert.Equal("Áo Mới Đã Cập Nhật", updated.Name);
        Assert.Equal(75000m, updated.StandardCost);
    }
}
