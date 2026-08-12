using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Mfg;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Fin;
using Erp.Infrastructure.Implementations.Services.Mfg;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 110:
///   UC_MFG_003 — Danh mục xưởng / dây chuyền (UpsertWorkshopAsync & ListWorkshopsAsync)
///   UC_MFG_006 — Tạo BOM nhiều cấp (UpsertBomAsync & GetBomDetailAsync)
///   UC_MFG_007 — Phiên bản BOM (ActivateBomAsync)
///   UC_MFG_008 — Định mức nguyên vật liệu (UpsertBomLineAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class MfgStep110PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly MfgProductionService _mfg;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public MfgStep110PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("mfg-step110-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin110", DisplayName = "Admin 110" });
        _db.SaveChanges();

        var finAcc = new FinAccountingService(_db);
        _mfg = new MfgProductionService(_db, finAcc);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_MFG_003: Danh mục xưởng / dây chuyền
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_MFG_003_UpsertWorkshop_ValidRequest_CreatesWorkshop()
    {
        var req = new MfgWorkshopUpsertRequest(null, "WS-110-01", "Phân Xưởng May 1", "Workshop", "Active", "Xưởng chính");

        var ws = await _mfg.UpsertWorkshopAsync(_tenant, _userAdmin, req);

        Assert.NotNull(ws);
        Assert.Equal("WS-110-01", ws.Code);
        Assert.Equal("Workshop", ws.WorkshopType);
    }

    [Fact]
    public async Task UC_MFG_003_ListWorkshops_ReturnsWorkshopList()
    {
        await _mfg.UpsertWorkshopAsync(_tenant, _userAdmin, new MfgWorkshopUpsertRequest(null, "LINE-110-01", "Dây Chuyền Cắt", "Line", "Active", null));

        var list = await _mfg.ListWorkshopsAsync(_tenant);

        Assert.NotEmpty(list);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_MFG_006: Tạo BOM nhiều cấp
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_MFG_006_UpsertBom_ValidParentFg_CreatesDraftBom()
    {
        var fg = await _mfg.UpsertItemAsync(_tenant, _userAdmin, new MfgItemUpsertRequest(null, "TP-BOM110", "Áo Vest Nam", "FG", "CAI", 500000m, "Active", null));
        var req = new MfgBomUpsertRequest(null, "BOM-VEST-V1", fg.Id, "1.0", "Draft", "BOM mẫu V1");

        var bom = await _mfg.UpsertBomAsync(_tenant, _userAdmin, req);

        Assert.NotNull(bom);
        Assert.Equal("1.0", bom.Version);
        Assert.Equal("Draft", bom.Status);
    }

    [Fact]
    public async Task UC_MFG_006_UpsertBom_RawMaterialParent_ThrowsException()
    {
        var rm = await _mfg.UpsertItemAsync(_tenant, _userAdmin, new MfgItemUpsertRequest(null, "RM-ERR110", "Vải Lót", "RM", "MET", 30000m, "Active", null));

        await Assert.ThrowsAsync<AppException>(() =>
            _mfg.UpsertBomAsync(_tenant, _userAdmin, new MfgBomUpsertRequest(null, null, rm.Id, "1.0", "Draft", null)));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_MFG_008: Định mức nguyên vật liệu
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_MFG_008_UpsertBomLine_ValidComponent_AddsLineToBom()
    {
        var fg = await _mfg.UpsertItemAsync(_tenant, _userAdmin, new MfgItemUpsertRequest(null, "TP-VEST2", "Áo Vest Nữ", "FG", "CAI", 450000m, "Active", null));
        var rm = await _mfg.UpsertItemAsync(_tenant, _userAdmin, new MfgItemUpsertRequest(null, "NVL-VAI", "Vải Dạ", "RM", "MET", 120000m, "Active", null));
        var bom = await _mfg.UpsertBomAsync(_tenant, _userAdmin, new MfgBomUpsertRequest(null, null, fg.Id, "1.0", "Draft", null));

        var line = await _mfg.UpsertBomLineAsync(_tenant, _userAdmin, bom.Id, new MfgBomLineUpsertRequest(null, rm.Id, 2.5m, "MET", 1, "Cắt 2.5 mét vải"));

        Assert.NotNull(line);
        Assert.Equal(2.5m, line.Qty);
        Assert.Equal("NVL-VAI", line.ComponentCode);
    }

    [Fact]
    public async Task UC_MFG_008_UpsertBomLine_SelfReferenceParent_ThrowsException()
    {
        var fg = await _mfg.UpsertItemAsync(_tenant, _userAdmin, new MfgItemUpsertRequest(null, "TP-SELF", "Áo Thun Self", "FG", "CAI", 100000m, "Active", null));
        var bom = await _mfg.UpsertBomAsync(_tenant, _userAdmin, new MfgBomUpsertRequest(null, null, fg.Id, "1.0", "Draft", null));

        await Assert.ThrowsAsync<AppException>(() =>
            _mfg.UpsertBomLineAsync(_tenant, _userAdmin, bom.Id, new MfgBomLineUpsertRequest(null, fg.Id, 1m, "CAI", 1, null)));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_MFG_007: Phiên bản BOM
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_MFG_007_ActivateBom_WithLines_TransitionsToActive()
    {
        var fg = await _mfg.UpsertItemAsync(_tenant, _userAdmin, new MfgItemUpsertRequest(null, "TP-ACTIVATE", "Quần Tây", "FG", "CAI", 250000m, "Active", null));
        var rm = await _mfg.UpsertItemAsync(_tenant, _userAdmin, new MfgItemUpsertRequest(null, "NVL-CHI", "Chỉ Khâu", "RM", "CUON", 8000m, "Active", null));
        var bom = await _mfg.UpsertBomAsync(_tenant, _userAdmin, new MfgBomUpsertRequest(null, null, fg.Id, "1.0", "Draft", null));
        await _mfg.UpsertBomLineAsync(_tenant, _userAdmin, bom.Id, new MfgBomLineUpsertRequest(null, rm.Id, 0.2m, "CUON", 1, null));

        var activeBom = await _mfg.ActivateBomAsync(_tenant, _userAdmin, bom.Id);

        Assert.NotNull(activeBom);
        Assert.Equal("Active", activeBom.Status);
    }

    [Fact]
    public async Task UC_MFG_007_ActivateBom_NoLines_ThrowsException()
    {
        var fg = await _mfg.UpsertItemAsync(_tenant, _userAdmin, new MfgItemUpsertRequest(null, "TP-NOLINES", "Áo Khoác Empty", "FG", "CAI", 300000m, "Active", null));
        var bom = await _mfg.UpsertBomAsync(_tenant, _userAdmin, new MfgBomUpsertRequest(null, null, fg.Id, "1.0", "Draft", null));

        await Assert.ThrowsAsync<AppException>(() =>
            _mfg.ActivateBomAsync(_tenant, _userAdmin, bom.Id));
    }

    [Fact]
    public async Task UC_MFG_006_GetBomDetail_ReturnsHeaderAndLines()
    {
        var fg = await _mfg.UpsertItemAsync(_tenant, _userAdmin, new MfgItemUpsertRequest(null, "TP-DETAIL", "Đầm Dạ Hội", "FG", "CAI", 800000m, "Active", null));
        var rm = await _mfg.UpsertItemAsync(_tenant, _userAdmin, new MfgItemUpsertRequest(null, "NVL-LUA", "Vải Lụa", "RM", "MET", 200000m, "Active", null));
        var bom = await _mfg.UpsertBomAsync(_tenant, _userAdmin, new MfgBomUpsertRequest(null, null, fg.Id, "1.0", "Draft", null));
        await _mfg.UpsertBomLineAsync(_tenant, _userAdmin, bom.Id, new MfgBomLineUpsertRequest(null, rm.Id, 3m, "MET", 1, null));

        var detail = await _mfg.GetBomDetailAsync(_tenant, bom.Id);

        Assert.NotNull(detail);
        Assert.NotNull(detail.Bom);
        Assert.NotEmpty(detail.Lines);
    }

    [Fact]
    public async Task UC_MFG_003_UpsertWorkshop_InvalidType_ThrowsException()
    {
        var req = new MfgWorkshopUpsertRequest(null, "WS-ERR", "Xưởng Lỗi", "INVALID", "Active", null);

        await Assert.ThrowsAsync<AppException>(() =>
            _mfg.UpsertWorkshopAsync(_tenant, _userAdmin, req));
    }
}
