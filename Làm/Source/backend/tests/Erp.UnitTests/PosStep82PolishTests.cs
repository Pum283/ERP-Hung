using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Fin;
using Erp.Application.DTOs.Pos;
using Erp.Application.Interfaces.Services.Fin;
using Erp.Domain.Entities.Inv;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Pos;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 82:
///   UC_POS_042 — Mở ca thu ngân (OpenShiftAsync)
///   UC_POS_043 — Nhập tiền đầu ca (OpenShiftAsync - OpeningCash)
///   UC_POS_045 — Xem doanh thu trong ca (GetShiftDetailAsync)
///   UC_POS_046 — Đóng ca & đếm quỹ (CloseShiftAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class PosStep82PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PosConfigService _configSvc;
    private readonly PosSalesService _salesSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    private sealed class NoopFinRevenue : IFinRevenueService
    {
        public Task<IReadOnlyList<FinRevenueDocumentDto>> ListAsync(
            Guid tenantId, string? kind = null, Guid? periodId = null, string? status = null, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<FinRevenueDocumentDto>>(Array.Empty<FinRevenueDocumentDto>());
        public Task<FinRevenueSummaryDto> GetSummaryAsync(Guid tenantId, Guid? periodId = null, CancellationToken ct = default)
            => throw new AppException("FIN chưa sẵn sàng.");
        public Task<FinRevenueDocumentDto> RecognizeFromPosAsync(Guid tenantId, Guid userId, Guid saleId, FinRevenueRecognizeRequest? req = null, CancellationToken ct = default)
            => throw new AppException("FIN chưa sẵn sàng.");
        public Task<FinRevenueDocumentDto> RecognizeFromSalesOrderAsync(Guid tenantId, Guid userId, Guid orderId, FinRevenueRecognizeRequest? req = null, CancellationToken ct = default)
            => throw new AppException("FIN chưa sẵn sàng.");
        public Task<FinRevenueDocumentDto> RecognizeFromArInvoiceAsync(Guid tenantId, Guid userId, Guid arInvoiceId, FinRevenueRecognizeRequest? req = null, CancellationToken ct = default)
            => throw new AppException("FIN chưa sẵn sàng.");
        public Task<FinRevenueDocumentDto> RecognizeCogsAsync(Guid tenantId, Guid userId, Guid invStockDocId, FinRevenueRecognizeRequest? req = null, CancellationToken ct = default)
            => throw new AppException("FIN chưa sẵn sàng.");
        public Task<FinRevenueDocumentDto> VoidAsync(Guid tenantId, Guid userId, Guid id, string? note = null, CancellationToken ct = default)
            => throw new AppException("FIN chưa sẵn sàng.");
    }

    public PosStep82PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pos-step82-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin82", DisplayName = "Admin 82" });
        _db.InvWarehouses.Add(new InvWarehouse { TenantId = _tenant, Code = "WH-82", Name = "Kho 82", Status = "Active" });
        _db.SaveChanges();

        _configSvc = new PosConfigService(_db);
        _salesSvc = new PosSalesService(_db, new NoopFinRevenue(), null!);
    }

    public void Dispose() => _db.Dispose();

    private async Task<PosStoreDto> CreateStoreAsync(string code = "STORE-82")
    {
        return await _configSvc.UpsertStoreAsync(_tenant, _userAdmin, new PosStoreUpsertRequest(null, code, "CH POS 82", null, "Active", null, null));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_042: Mở ca thu ngân
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_042_OpenShift_ValidRequest_CreatesShiftWithOpenStatus()
    {
        var store = await CreateStoreAsync();

        var shift = await _salesSvc.OpenShiftAsync(_tenant, _userAdmin, new PosShiftOpenRequest(store.Id, null, 300000m, "Mở ca sáng"));

        Assert.NotNull(shift);
        Assert.Equal("Open", shift.Status);
        Assert.Equal(store.Id, shift.StoreId);
        Assert.Equal(_userAdmin, shift.CashierUserId);
    }

    [Fact]
    public async Task UC_POS_042_OpenShift_InvalidStore_ThrowsException()
    {
        await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.OpenShiftAsync(_tenant, _userAdmin, new PosShiftOpenRequest(Guid.NewGuid(), null, 300000m, null)));
    }

    [Fact]
    public async Task UC_POS_042_OpenShift_CashierHasActiveShift_ThrowsException()
    {
        var store = await CreateStoreAsync();
        await _salesSvc.OpenShiftAsync(_tenant, _userAdmin, new PosShiftOpenRequest(store.Id, null, 300000m, "Ca 1"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.OpenShiftAsync(_tenant, _userAdmin, new PosShiftOpenRequest(store.Id, null, 200000m, "Ca 2")));
        Assert.Contains("Đã có ca Open", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_043: Nhập tiền đầu ca
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_043_OpenShift_WithInitialCash_SetsOpeningCash()
    {
        var store = await CreateStoreAsync();
        var initialCash = 750000m;

        var shift = await _salesSvc.OpenShiftAsync(_tenant, _userAdmin, new PosShiftOpenRequest(store.Id, null, initialCash, "Mở ca 750k"));

        Assert.Equal(initialCash, shift.OpeningCash);
    }

    [Fact]
    public async Task UC_POS_043_OpenShift_ZeroInitialCash_Allowed()
    {
        var store = await CreateStoreAsync();

        var shift = await _salesSvc.OpenShiftAsync(_tenant, _userAdmin, new PosShiftOpenRequest(store.Id, null, 0m, "Mở ca 0d"));

        Assert.Equal(0m, shift.OpeningCash);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_045: Xem doanh thu trong ca
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_045_GetShiftDetail_CalculatesTotalsCorrectly()
    {
        var store = await CreateStoreAsync();
        var shift = await _salesSvc.OpenShiftAsync(_tenant, _userAdmin, new PosShiftOpenRequest(store.Id, null, 500000m, null));
        var sale = await _salesSvc.OpenSaleAsync(_tenant, _userAdmin, new PosSaleOpenRequest(shift.Id, "Bàn 1", null));
        var prod = await _configSvc.UpsertProductAsync(_tenant, _userAdmin, new PosProductUpsertRequest(null, null, "P-82", "Món 82", "Cái", "Active", 1));
        await _salesSvc.UpsertSaleLineAsync(_tenant, _userAdmin, sale.Id, new PosSaleLineUpsertRequest(null, prod.Id, prod.Code, prod.Name, 1, 100000m, 0m));
        await _salesSvc.PaySaleAsync(_tenant, _userAdmin, sale.Id, new PosSalePayRequest("Cash", 100000m, null));

        var detail = await _salesSvc.GetShiftDetailAsync(_tenant, shift.Id);

        Assert.NotNull(detail);
        Assert.Equal(1, detail.Shift.SaleCount);
        Assert.Equal(100000m, detail.Shift.CashSalesTotal);
    }

    [Fact]
    public async Task UC_POS_045_GetShiftDetail_EmptyShift_ReturnsZeroSales()
    {
        var store = await CreateStoreAsync();
        var shift = await _salesSvc.OpenShiftAsync(_tenant, _userAdmin, new PosShiftOpenRequest(store.Id, null, 200000m, null));

        var detail = await _salesSvc.GetShiftDetailAsync(_tenant, shift.Id);

        Assert.Equal(0, detail.Shift.SaleCount);
        Assert.Equal(0m, detail.Shift.CashSalesTotal);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_046: Đóng ca & đếm quỹ
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_046_CloseShift_ValidCashCount_SetsClosedStatusAndVariance()
    {
        var store = await CreateStoreAsync();
        var shift = await _salesSvc.OpenShiftAsync(_tenant, _userAdmin, new PosShiftOpenRequest(store.Id, null, 500000m, null));
        var sale = await _salesSvc.OpenSaleAsync(_tenant, _userAdmin, new PosSaleOpenRequest(shift.Id, "Bàn 1", null));
        var prod = await _configSvc.UpsertProductAsync(_tenant, _userAdmin, new PosProductUpsertRequest(null, null, "P-82", "Món 82", "Cái", "Active", 1));
        await _salesSvc.UpsertSaleLineAsync(_tenant, _userAdmin, sale.Id, new PosSaleLineUpsertRequest(null, prod.Id, prod.Code, prod.Name, 1, 100000m, 0m));
        await _salesSvc.PaySaleAsync(_tenant, _userAdmin, sale.Id, new PosSalePayRequest("Cash", 100000m, null));

        // Expected cash = 500k + 100k = 600k. Actual counted = 600k -> Variance = 0
        var closedShift = await _salesSvc.CloseShiftAsync(_tenant, _userAdmin, shift.Id, new PosShiftCloseRequest(600000m, "Đóng ca khớp quỹ"));

        Assert.Equal("Closed", closedShift.Status);
        Assert.NotNull(closedShift.ClosedAt);
        Assert.Equal(600000m, closedShift.ClosingCashCounted);
        Assert.Equal(0m, closedShift.Variance);
    }

    [Fact]
    public async Task UC_POS_046_CloseShift_AlreadyClosed_ThrowsException()
    {
        var store = await CreateStoreAsync();
        var shift = await _salesSvc.OpenShiftAsync(_tenant, _userAdmin, new PosShiftOpenRequest(store.Id, null, 500000m, null));
        await _salesSvc.CloseShiftAsync(_tenant, _userAdmin, shift.Id, new PosShiftCloseRequest(500000m, "Lần 1"));

        await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.CloseShiftAsync(_tenant, _userAdmin, shift.Id, new PosShiftCloseRequest(500000m, "Lần 2")));
    }

    [Fact]
    public async Task UC_POS_046_CloseShift_NonExistentShift_ThrowsException()
    {
        await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.CloseShiftAsync(_tenant, _userAdmin, Guid.NewGuid(), new PosShiftCloseRequest(500000m, null)));
    }
}
