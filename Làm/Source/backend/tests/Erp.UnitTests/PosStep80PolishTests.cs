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
/// Unit tests cho Bước 80:
///   UC_POS_032 — Tạm tính / giữ đơn (HoldSaleAsync & ResumeSaleAsync)
///   UC_POS_033 — Thanh toán tiền mặt (PaySaleAsync - Cash)
///   UC_POS_034 — Thanh toán chuyển khoản / QR (PaySaleAsync - Transfer)
///   UC_POS_035 — Thanh toán thẻ / ví điện tử (PaySaleAsync - Card & Wallet)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class PosStep80PolishTests : IDisposable
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

    public PosStep80PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pos-step80-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin80", DisplayName = "Admin 80" });
        _db.InvWarehouses.Add(new InvWarehouse { TenantId = _tenant, Code = "WH-80", Name = "Kho 80", Status = "Active" });
        _db.SaveChanges();

        _configSvc = new PosConfigService(_db);
        _salesSvc = new PosSalesService(_db, new NoopFinRevenue(), null!);
    }

    public void Dispose() => _db.Dispose();

    private async Task<(PosStoreDto store, PosShiftDto shift, PosSaleDto sale)> CreateSaleWithItemsAsync(decimal itemPrice = 100000m)
    {
        var store = await _configSvc.UpsertStoreAsync(_tenant, _userAdmin, new PosStoreUpsertRequest(null, "STORE-80", "CH POS 80", null, "Active", null, null));
        var shift = await _salesSvc.OpenShiftAsync(_tenant, _userAdmin, new PosShiftOpenRequest(store.Id, null, 500000m, "Mở ca 80"));
        var sale = await _salesSvc.OpenSaleAsync(_tenant, _userAdmin, new PosSaleOpenRequest(shift.Id, "Bàn 10", null));
        var prod = await _configSvc.UpsertProductAsync(_tenant, _userAdmin, new PosProductUpsertRequest(null, null, "P-80", "Sản Phẩm Test 80", "Cái", "Active", 1));
        await _salesSvc.UpsertSaleLineAsync(_tenant, _userAdmin, sale.Id, new PosSaleLineUpsertRequest(null, prod.Id, prod.Code, prod.Name, 1, itemPrice, 0m));

        var saleDetail = (await _salesSvc.GetSaleDetailAsync(_tenant, sale.Id)).Sale;
        return (store, shift, saleDetail);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_032: Tạm tính / giữ đơn
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_032_HoldSale_OpenSaleWithItems_TransitionsStatusToHeld()
    {
        var (_, _, sale) = await CreateSaleWithItemsAsync(150000m);

        var held = await _salesSvc.HoldSaleAsync(_tenant, _userAdmin, sale.Id, new PosSaleHoldRequest("Khách ra ngoài nghe điện thoại"));

        Assert.NotNull(held);
        Assert.Equal("Held", held.Status);
    }

    [Fact]
    public async Task UC_POS_032_ResumeSale_HeldSale_TransitionsStatusToOpen()
    {
        var (_, _, sale) = await CreateSaleWithItemsAsync(150000m);
        await _salesSvc.HoldSaleAsync(_tenant, _userAdmin, sale.Id, new PosSaleHoldRequest("Khách chờ đồ"));

        var resumed = await _salesSvc.ResumeSaleAsync(_tenant, _userAdmin, sale.Id);

        Assert.NotNull(resumed);
        Assert.Equal("Open", resumed.Status);
    }

    [Fact]
    public async Task UC_POS_032_HoldSale_NonOpenSale_ThrowsAppException()
    {
        var store = await _configSvc.UpsertStoreAsync(_tenant, _userAdmin, new PosStoreUpsertRequest(null, "STORE-80B", "CH POS 80B", null, "Active", null, null));
        var shift = await _salesSvc.OpenShiftAsync(_tenant, _userAdmin, new PosShiftOpenRequest(store.Id, null, 500000m, null));
        var sale = await _salesSvc.OpenSaleAsync(_tenant, _userAdmin, new PosSaleOpenRequest(shift.Id, null, null));

        // Hủy đơn trước
        await _salesSvc.CancelSaleAsync(_tenant, _userAdmin, sale.Id, "Hủy đơn");

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.HoldSaleAsync(_tenant, _userAdmin, sale.Id, new PosSaleHoldRequest(null)));

        Assert.Contains("Chỉ giữ đơn Open", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_033: Thanh toán tiền mặt
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_033_PaySale_Cash_FullPayment_TransitionsSaleToPaid()
    {
        var (_, _, sale) = await CreateSaleWithItemsAsync(200000m);

        var payResult = await _salesSvc.PaySaleAsync(_tenant, _userAdmin, sale.Id, new PosSalePayRequest("Cash", 200000m, null));
        Assert.NotNull(payResult);

        var paidSale = (await _salesSvc.GetSaleDetailAsync(_tenant, sale.Id)).Sale;
        Assert.Equal("Paid", paidSale.Status);
        Assert.Equal(200000m, paidSale.PaidAmount);
    }

    [Fact]
    public async Task UC_POS_034_PaySale_Transfer_ValidAmount_UpdatesPaidAmount()
    {
        var (_, _, sale) = await CreateSaleWithItemsAsync(500000m);

        var payResult = await _salesSvc.PaySaleAsync(_tenant, _userAdmin, sale.Id, new PosSalePayRequest("Transfer", 500000m, "Chuyển khoản VietQR"));
        Assert.NotNull(payResult);

        var paidSale = (await _salesSvc.GetSaleDetailAsync(_tenant, sale.Id)).Sale;
        Assert.Equal("Paid", paidSale.Status);
    }

    [Fact]
    public async Task UC_POS_035_PaySale_Card_CompletesSale()
    {
        var (_, _, sale) = await CreateSaleWithItemsAsync(300000m);

        var payResult = await _salesSvc.PaySaleAsync(_tenant, _userAdmin, sale.Id, new PosSalePayRequest("Card", 300000m, "Quẹt thẻ POS NganHang"));
        Assert.NotNull(payResult);

        var paidSale = (await _salesSvc.GetSaleDetailAsync(_tenant, sale.Id)).Sale;
        Assert.Equal("Paid", paidSale.Status);
        Assert.Equal(300000m, paidSale.PaidAmount);
    }

    [Fact]
    public async Task UC_POS_035_PaySale_ExceedsRemaining_ThrowsAppException()
    {
        var (_, _, sale) = await CreateSaleWithItemsAsync(500000m);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.PaySaleAsync(_tenant, _userAdmin, sale.Id, new PosSalePayRequest("Card", 600000m, null)));

        Assert.Contains("Vượt còn lại", ex.Message);
    }

    [Fact]
    public async Task UC_POS_035_PaySale_InvalidMethod_ThrowsAppException()
    {
        var (_, _, sale) = await CreateSaleWithItemsAsync(100000m);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.PaySaleAsync(_tenant, _userAdmin, sale.Id, new PosSalePayRequest("Bitcoin", 100000m, null)));

        Assert.Contains("HT: Cash | Transfer | Card | Wallet", ex.Message);
    }

    [Fact]
    public async Task UC_POS_035_PaySale_AlreadyPaidSale_ThrowsAppException()
    {
        var (_, _, sale) = await CreateSaleWithItemsAsync(100000m);
        await _salesSvc.PaySaleAsync(_tenant, _userAdmin, sale.Id, new PosSalePayRequest("Cash", 100000m, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.PaySaleAsync(_tenant, _userAdmin, sale.Id, new PosSalePayRequest("Cash", 100000m, null)));

        Assert.Contains("Vượt còn lại", ex.Message);
    }

    [Fact]
    public async Task UC_POS_035_PaySale_ZeroAmount_ThrowsAppException()
    {
        var (_, _, sale) = await CreateSaleWithItemsAsync(100000m);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.PaySaleAsync(_tenant, _userAdmin, sale.Id, new PosSalePayRequest("Cash", 0m, null)));

        Assert.Contains("Số tiền > 0", ex.Message);
    }
}
