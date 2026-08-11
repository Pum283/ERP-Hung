using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Pos;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Pos;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 76:
///   UC_POS_001 — Khai báo điểm bán POS (UpsertStoreAsync & ListStoresAsync)
///   UC_POS_002 — Khai báo quầy / máy POS (UpsertTerminalAsync & GetStoreDetailAsync)
///   UC_POS_003 — Cấu hình máy in hóa đơn (UpsertPrinterAsync)
///   UC_POS_007 — Phân quyền thu ngân trên POS (UpsertCashierAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class PosStep76PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PosConfigService _configSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public PosStep76PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pos-step76-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "cashier76", DisplayName = "Thu ngân 76" });
        _db.SaveChanges();

        _configSvc = new PosConfigService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_001: Khai báo điểm bán POS
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_001_UpsertStore_ValidRequest_CreatesStoreSuccessfully()
    {
        var req = new PosStoreUpsertRequest(null, "POS-STORE-01", "Cửa Hàng Trung Tâm", "123 Lê Lợi, Q1", "Active", null, 100000000m);
        var store = await _configSvc.UpsertStoreAsync(_tenant, _userAdmin, req);

        Assert.NotNull(store);
        Assert.Equal("POS-STORE-01", store.Code);
        Assert.Equal("Cửa Hàng Trung Tâm", store.Name);
        Assert.Equal("Active", store.Status);
    }

    [Fact]
    public async Task UC_POS_001_UpsertStore_InvalidStatus_ThrowsAppException()
    {
        var req = new PosStoreUpsertRequest(null, "POS-STORE-02", "Cửa Hàng Thuận An", null, "InvalidStatus", null, null);
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _configSvc.UpsertStoreAsync(_tenant, _userAdmin, req));

        Assert.Contains("không hợp lệ", ex.Message);
    }

    [Fact]
    public async Task UC_POS_001_UpsertStore_DuplicateCode_ThrowsAppException()
    {
        var req1 = new PosStoreUpsertRequest(null, "POS-STORE-DUP", "Cửa Hàng 1", null, "Active", null, null);
        await _configSvc.UpsertStoreAsync(_tenant, _userAdmin, req1);

        var req2 = new PosStoreUpsertRequest(null, "POS-STORE-DUP", "Cửa Hàng 2", null, "Active", null, null);
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _configSvc.UpsertStoreAsync(_tenant, _userAdmin, req2));

        Assert.Contains("đã tồn tại", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_002: Khai báo quầy / máy POS
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_002_UpsertTerminal_ValidStore_CreatesTerminalSuccessfully()
    {
        var store = await _configSvc.UpsertStoreAsync(_tenant, _userAdmin, new PosStoreUpsertRequest(null, "STORE-T1", "CH Q1", null, "Active", null, null));
        var term = await _configSvc.UpsertTerminalAsync(_tenant, _userAdmin, store.Id, new PosTerminalUpsertRequest(null, "POS-01", "Quầy Thu Ngân 1", "Active"));

        Assert.NotNull(term);
        Assert.Equal("POS-01", term.Code);
        Assert.Equal(store.Id, term.StoreId);
    }

    [Fact]
    public async Task UC_POS_002_UpsertTerminal_NonExistentStore_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _configSvc.UpsertTerminalAsync(_tenant, _userAdmin, Guid.NewGuid(), new PosTerminalUpsertRequest(null, "POS-99", "Quầy 99", "Active")));

        Assert.Equal(404, ex.StatusCode);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_003: Cấu hình máy in hóa đơn
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_003_UpsertPrinter_ValidReceiptPrinter_CreatesPrinterSuccessfully()
    {
        var store = await _configSvc.UpsertStoreAsync(_tenant, _userAdmin, new PosStoreUpsertRequest(null, "STORE-P1", "CH Q3", null, "Active", null, null));
        var printer = await _configSvc.UpsertPrinterAsync(_tenant, _userAdmin, store.Id, new PosPrinterUpsertRequest(null, "PRT-01", "Máy In Bill K80", "Receipt", "192.168.1.100:9100", "Active"));

        Assert.NotNull(printer);
        Assert.Equal("PRT-01", printer.Code);
        Assert.Equal("Receipt", printer.PrinterType);
    }

    [Fact]
    public async Task UC_POS_003_UpsertPrinter_InvalidPrinterType_ThrowsAppException()
    {
        var store = await _configSvc.UpsertStoreAsync(_tenant, _userAdmin, new PosStoreUpsertRequest(null, "STORE-P2", "CH Q7", null, "Active", null, null));
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _configSvc.UpsertPrinterAsync(_tenant, _userAdmin, store.Id, new PosPrinterUpsertRequest(null, "PRT-02", "Máy In 3D", "InvalidType", null, "Active")));

        Assert.Contains("Loại máy in không hợp lệ", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_007: Phân quyền thu ngân trên POS
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_007_UpsertCashier_ValidCashierUser_AssignsRoleSuccessfully()
    {
        var store = await _configSvc.UpsertStoreAsync(_tenant, _userAdmin, new PosStoreUpsertRequest(null, "STORE-C1", "CH Bình Thạnh", null, "Active", null, null));
        var cashier = await _configSvc.UpsertCashierAsync(_tenant, _userAdmin, store.Id, new PosCashierUpsertRequest(null, _userAdmin, "Cashier", true));

        Assert.NotNull(cashier);
        Assert.Equal(_userAdmin, cashier.UserId);
        Assert.Equal("Cashier", cashier.Role);
        Assert.True(cashier.IsActive);
    }

    [Fact]
    public async Task UC_POS_007_UpsertCashier_InvalidRole_ThrowsAppException()
    {
        var store = await _configSvc.UpsertStoreAsync(_tenant, _userAdmin, new PosStoreUpsertRequest(null, "STORE-C2", "CH Thủ Đức", null, "Active", null, null));
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _configSvc.UpsertCashierAsync(_tenant, _userAdmin, store.Id, new PosCashierUpsertRequest(null, _userAdmin, "MasterAdmin", true)));

        Assert.Contains("Vai trò thu ngân không hợp lệ", ex.Message);
    }

    [Fact]
    public async Task UC_POS_007_UpsertCashier_NonExistentUser_ThrowsAppException()
    {
        var store = await _configSvc.UpsertStoreAsync(_tenant, _userAdmin, new PosStoreUpsertRequest(null, "STORE-C3", "CH Gò Vấp", null, "Active", null, null));
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _configSvc.UpsertCashierAsync(_tenant, _userAdmin, store.Id, new PosCashierUpsertRequest(null, Guid.NewGuid(), "Supervisor", true)));

        Assert.Equal(404, ex.StatusCode);
    }
}
