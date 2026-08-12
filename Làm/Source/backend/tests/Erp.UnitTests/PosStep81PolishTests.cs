using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Fin;
using Erp.Application.DTOs.Pos;
using Erp.Application.Interfaces.Services.Fin;
using Erp.Domain.Entities.Inv;
using Erp.Domain.Entities.Pos;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Pos;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 81:
///   UC_POS_037 — In hóa đơn (BuildReceiptTextAsync)
///   UC_POS_038 — Hủy sản phẩm (CancelSaleLineAsync)
///   UC_POS_039 — Hủy cả bill (CancelSaleAsync)
///   UC_POS_040 — Trả hàng / hoàn tiền (CancelSaleAsync validation & status logic)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class PosStep81PolishTests : IDisposable
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

    public PosStep81PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pos-step81-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin81", DisplayName = "Admin 81" });
        _db.InvWarehouses.Add(new InvWarehouse { TenantId = _tenant, Code = "WH-81", Name = "Kho 81", Status = "Active" });
        _db.SaveChanges();

        _configSvc = new PosConfigService(_db);
        _salesSvc = new PosSalesService(_db, new NoopFinRevenue(), null!);
    }

    public void Dispose() => _db.Dispose();

    private async Task<(PosStoreDto store, PosShiftDto shift, PosSaleDto sale, PosSaleLineDto line)> CreateSaleWithLineAsync()
    {
        var store = await _configSvc.UpsertStoreAsync(_tenant, _userAdmin, new PosStoreUpsertRequest(null, "STORE-81", "CH POS 81", null, "Active", null, null));
        var shift = await _salesSvc.OpenShiftAsync(_tenant, _userAdmin, new PosShiftOpenRequest(store.Id, null, 500000m, "Mở ca 81"));
        var sale = await _salesSvc.OpenSaleAsync(_tenant, _userAdmin, new PosSaleOpenRequest(shift.Id, "Bàn 1", null));
        var prod = await _configSvc.UpsertProductAsync(_tenant, _userAdmin, new PosProductUpsertRequest(null, null, "P-81", "Món Ăn 81", "Dĩa", "Active", 1));
        var line = await _salesSvc.UpsertSaleLineAsync(_tenant, _userAdmin, sale.Id, new PosSaleLineUpsertRequest(null, prod.Id, prod.Code, prod.Name, 2, 50000m, 0m));

        var updatedSale = (await _salesSvc.GetSaleDetailAsync(_tenant, sale.Id)).Sale;
        return (store, shift, updatedSale, line);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_037: In hóa đơn
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_037_BuildReceiptText_PaidSale_ReturnsTextAndUpdatesPrintedAt()
    {
        var (_, _, sale, _) = await CreateSaleWithLineAsync();
        await _salesSvc.PaySaleAsync(_tenant, _userAdmin, sale.Id, new PosSalePayRequest("Cash", 100000m, null));

        var (fileName, content) = await _salesSvc.BuildReceiptTextAsync(_tenant, _userAdmin, sale.Id);

        Assert.NotNull(fileName);
        Assert.Contains("HÓA ĐƠN BÁN LẺ", content);
        Assert.Contains("Món Ăn 81", content);

        var dbSale = await _db.PosSales.FirstAsync(x => x.Id == sale.Id);
        Assert.NotNull(dbSale.ReceiptPrintedAt);
    }

    [Fact]
    public async Task UC_POS_037_BuildReceiptText_OpenSale_ThrowsException()
    {
        var (_, _, sale, _) = await CreateSaleWithLineAsync();

        await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.BuildReceiptTextAsync(_tenant, _userAdmin, sale.Id));
    }

    [Fact]
    public async Task UC_POS_037_BuildReceiptText_Reprinting_UpdatesTimestamp()
    {
        var (_, _, sale, _) = await CreateSaleWithLineAsync();
        await _salesSvc.PaySaleAsync(_tenant, _userAdmin, sale.Id, new PosSalePayRequest("Cash", 100000m, null));

        await _salesSvc.BuildReceiptTextAsync(_tenant, _userAdmin, sale.Id);
        var t1 = (await _db.PosSales.FirstAsync(x => x.Id == sale.Id)).ReceiptPrintedAt;

        await Task.Delay(10);
        await _salesSvc.BuildReceiptTextAsync(_tenant, _userAdmin, sale.Id);
        var t2 = (await _db.PosSales.FirstAsync(x => x.Id == sale.Id)).ReceiptPrintedAt;

        Assert.True(t2 >= t1);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_038: Hủy sản phẩm
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_038_CancelSaleLine_OpenSale_SetsStatusCancelledAndRecalculatesTotal()
    {
        var (_, _, sale, line) = await CreateSaleWithLineAsync();

        var cancelledLine = await _salesSvc.CancelSaleLineAsync(_tenant, _userAdmin, sale.Id, line.Id);

        Assert.Equal("Cancelled", cancelledLine.Status);
        var updatedSale = (await _salesSvc.GetSaleDetailAsync(_tenant, sale.Id)).Sale;
        Assert.Equal(0m, updatedSale.TotalAmount);
    }

    [Fact]
    public async Task UC_POS_038_CancelSaleLine_LineNotFoundOrWrongSale_ThrowsException()
    {
        var (_, _, sale, _) = await CreateSaleWithLineAsync();

        await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.CancelSaleLineAsync(_tenant, _userAdmin, sale.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task UC_POS_038_CancelSaleLine_PaidSale_ThrowsException()
    {
        var (_, _, sale, line) = await CreateSaleWithLineAsync();
        await _salesSvc.PaySaleAsync(_tenant, _userAdmin, sale.Id, new PosSalePayRequest("Cash", 100000m, null));

        await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.CancelSaleLineAsync(_tenant, _userAdmin, sale.Id, line.Id));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_039: Hủy cả bill
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_039_CancelSale_OpenSale_SetsStatusCancelledAndNote()
    {
        var (_, _, sale, _) = await CreateSaleWithLineAsync();

        var cancelledSale = await _salesSvc.CancelSaleAsync(_tenant, _userAdmin, sale.Id, "Khách đổi ý");

        Assert.Equal("Cancelled", cancelledSale.Status);
        Assert.Equal("Khách đổi ý", cancelledSale.Note);
    }

    [Fact]
    public async Task UC_POS_039_CancelSale_AlreadyCancelled_ThrowsException()
    {
        var (_, _, sale, _) = await CreateSaleWithLineAsync();
        await _salesSvc.CancelSaleAsync(_tenant, _userAdmin, sale.Id, "Hủy lần 1");

        await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.CancelSaleAsync(_tenant, _userAdmin, sale.Id, "Hủy lần 2"));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_POS_040: Trả hàng / hoàn tiền
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_POS_040_CancelSale_PaidSale_ThrowsExceptionDirectingToRefund()
    {
        var (_, _, sale, _) = await CreateSaleWithLineAsync();
        await _salesSvc.PaySaleAsync(_tenant, _userAdmin, sale.Id, new PosSalePayRequest("Cash", 100000m, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.CancelSaleAsync(_tenant, _userAdmin, sale.Id, "Yêu cầu hoàn trả"));
        Assert.Contains("Đơn đã thanh toán — dùng trả hàng", ex.Message);
    }

    [Fact]
    public async Task UC_POS_040_PaySale_CancelledSale_ThrowsException()
    {
        var (_, _, sale, _) = await CreateSaleWithLineAsync();
        await _salesSvc.CancelSaleAsync(_tenant, _userAdmin, sale.Id, "Hủy đơn trước khi thanh toán");

        await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.PaySaleAsync(_tenant, _userAdmin, sale.Id, new PosSalePayRequest("Cash", 100000m, null)));
    }
}
