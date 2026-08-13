using Erp.Application.DTOs;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class PurQuotationCompareAwardPoConfirmPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PurQuotationCompareAwardPoConfirmService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _rfqId = Guid.NewGuid();
    private readonly Guid _supplierId = Guid.NewGuid();
    private readonly Guid _poId = Guid.NewGuid();

    public PurQuotationCompareAwardPoConfirmPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pur-quotation-compare-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "TPUR188", Name = "Tenant PUR 188" });
        _db.SaveChanges();

        _svc = new PurQuotationCompareAwardPoConfirmService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_022: Nhập báo giá từ nhà cung cấp
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitVendorQuotation_SubmitsQuotationSuccessfully()
    {
        var items = new List<PurQuotationLineItemDto>
        {
            new(Guid.NewGuid(), "SKU-MILK", "Sữa Tươi 1L", 1000, 24000m)
        };

        var req = new PurSubmitVendorQuotationRequest(_rfqId, _supplierId, "QUO-VIN-001", 3, "Net 30", items);
        var res = await _svc.SubmitVendorQuotationAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("QUO-VIN-001", res.QuotationNumber);
        Assert.Equal(24000000m, res.TotalAmountVnd);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_023 & UC_PUR_024: So sánh & Chọn NCC thắng
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AwardQuotationWinner_AwardsWinningVendorQuotation()
    {
        var quoId = Guid.NewGuid();
        var req = new PurAwardQuotationWinnerRequest(quoId, "Báo giá tốt nhất và Lead time ngắn nhất 3 ngày");
        var res = await _svc.AwardQuotationWinnerAsync(_tenant, _userId, req);

        Assert.NotNull(res);
        Assert.True(res.IsWinner);
        Assert.Equal("Awarded", res.Status);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_029: Xác nhận PO từ nhà cung cấp
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ConfirmVendorPo_ConfirmsVendorPurchaseOrderAck()
    {
        var req = new PurConfirmVendorPoRequest(_poId, "PO-202608-001", _supplierId, "Confirmed", DateTimeOffset.UtcNow.AddDays(5), "Đã xác nhận giao hàng đúng hẹn");
        var res = await _svc.ConfirmVendorPoAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("PO-202608-001", res.PoNumber);
        Assert.Equal("Confirmed", res.ConfirmationStatus);
    }
}
