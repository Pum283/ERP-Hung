using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Inv;
using Erp.Application.DTOs.Pur;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Inv;
using Erp.Infrastructure.Implementations.Services.Pur;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 92:
///   UC_PUR_051 — Open PR / Open PO aging (OpenPrAgingAsync & OpenPoAgingAsync)
///   UC_PUR_052 — Xuất báo cáo mua hàng (ExportCsvAsync)
///   UC_INV_001 — Tạo / sửa SKU sản phẩm (UpsertSkuAsync)
///   UC_INV_002 — Phân nhóm hàng / ngành hàng (UpsertGroupAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class PurStep92PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PurReportService _purReport;
    private readonly InvMasterService _invMaster;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public PurStep92PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pur-step92-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin92", DisplayName = "Admin 92" });
        _db.SaveChanges();

        _purReport = new PurReportService(_db);
        _invMaster = new InvMasterService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_PUR_051: Open PR / Open PO aging
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_PUR_051_OpenPrAging_ReturnsAgingList()
    {
        var list = await _purReport.OpenPrAgingAsync(_tenant);

        Assert.NotNull(list);
    }

    [Fact]
    public async Task UC_PUR_051_OpenPoAging_ReturnsAgingList()
    {
        var list = await _purReport.OpenPoAgingAsync(_tenant);

        Assert.NotNull(list);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_PUR_052: Xuất báo cáo mua hàng
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_PUR_052_ExportCsv_ValidReportType_ReturnsCsvText()
    {
        var csv = await _purReport.ExportCsvAsync(_tenant, "by-vendor", null, null);

        Assert.NotNull(csv);
        Assert.Contains("VendorCode", csv);
    }

    [Fact]
    public async Task UC_PUR_052_ExportCsv_InvalidReportType_ThrowsException()
    {
        await Assert.ThrowsAsync<AppException>(() =>
            _purReport.ExportCsvAsync(_tenant, "invalid-type", null, null));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_001: Tạo / sửa SKU sản phẩm
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_001_UpsertSku_ValidRequest_CreatesSku()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "CAI", "Cái", true));
        var req = new InvSkuUpsertRequest(null, "SKU-92-01", "Bút Máy 92", null, uom.Id, false, false, false, "Average", 10000m, "Active", null, null, null, null);

        var sku = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, req);

        Assert.NotNull(sku);
        Assert.Equal("SKU-92-01", sku.Code);
        Assert.Equal("Bút Máy 92", sku.Name);
    }

    [Fact]
    public async Task UC_INV_001_UpsertSku_UpdateExistingSku_UpdatesName()
    {
        var uom = await _invMaster.UpsertUomAsync(_tenant, _userAdmin, new InvUomUpsertRequest(null, "HOP", "Hộp", true));
        var s1 = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "SKU-92-02", "Tên Cũ", null, uom.Id, false, false, false, "Average", 5000m, "Active", null, null, null, null));
        var s2 = await _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(s1.Id, "SKU-92-02", "Tên Mới", null, uom.Id, false, false, false, "Average", 5000m, "Active", null, null, null, null));

        Assert.Equal("Tên Mới", s2.Name);
    }

    [Fact]
    public async Task UC_INV_001_UpsertSku_EmptyCode_ThrowsException()
    {
        await Assert.ThrowsAsync<AppException>(() =>
            _invMaster.UpsertSkuAsync(_tenant, _userAdmin, new InvSkuUpsertRequest(null, "", "Tên SKU", null, Guid.NewGuid(), false, false, false, "Average", 0m, "Active", null, null, null, null)));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_INV_002: Phân nhóm hàng / ngành hàng
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_INV_002_UpsertGroup_ValidRequest_CreatesGroup()
    {
        var group = await _invMaster.UpsertGroupAsync(_tenant, _userAdmin, new InvItemGroupUpsertRequest(null, "NH-VPP", "Văn Phòng Phẩm", 1, true));

        Assert.NotNull(group);
        Assert.Equal("NH-VPP", group.Code);
        Assert.Equal("Văn Phòng Phẩm", group.Name);
    }

    [Fact]
    public async Task UC_INV_002_UpsertGroup_UpdateExisting_UpdatesName()
    {
        var g1 = await _invMaster.UpsertGroupAsync(_tenant, _userAdmin, new InvItemGroupUpsertRequest(null, "NH-MAY", "May Mặc", 1, true));
        var g2 = await _invMaster.UpsertGroupAsync(_tenant, _userAdmin, new InvItemGroupUpsertRequest(g1.Id, "NH-MAY", "May Mặc & Thời Trang", 1, true));

        Assert.Equal("May Mặc & Thời Trang", g2.Name);
    }

    [Fact]
    public async Task UC_INV_002_ListGroups_ReturnsAllGroups()
    {
        await _invMaster.UpsertGroupAsync(_tenant, _userAdmin, new InvItemGroupUpsertRequest(null, "G1", "Nhóm 1", 1, true));
        await _invMaster.UpsertGroupAsync(_tenant, _userAdmin, new InvItemGroupUpsertRequest(null, "G2", "Nhóm 2", 2, true));

        var list = await _invMaster.ListGroupsAsync(_tenant);

        Assert.True(list.Count >= 2);
    }
}
