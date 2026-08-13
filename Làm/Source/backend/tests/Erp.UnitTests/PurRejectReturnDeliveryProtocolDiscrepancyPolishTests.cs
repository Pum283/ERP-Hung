using Erp.Application.DTOs;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class PurRejectReturnDeliveryProtocolDiscrepancyPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PurRejectReturnDeliveryProtocolDiscrepancyService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _poId = Guid.NewGuid();
    private readonly Guid _supplierId = Guid.NewGuid();
    private readonly Guid _grnId = Guid.NewGuid();

    public PurRejectReturnDeliveryProtocolDiscrepancyPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pur-reject-return-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "TPUR189", Name = "Tenant PUR 189" });
        _db.SaveChanges();

        _svc = new PurRejectReturnDeliveryProtocolDiscrepancyService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_036: Từ chối lô hàng không đạt QC
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RejectShipment_QuarantinesSubstandardGoods()
    {
        var req = new PurRejectShipmentRequest(_poId, _supplierId, "Bao bì rách hỏng và sản phẩm ẩm mốc", 30, "QC từ chối nhập kho");
        var res = await _svc.RejectShipmentAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(30, res.RejectedQuantity);
        Assert.Equal("Quarantined", res.Status);
        Assert.StartsWith("REJ-", res.RejectionNumber);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_038: Trả hàng nhà cung cấp (RTV)
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateVendorReturn_IssuesReturnToVendorOrder()
    {
        var rejId = Guid.NewGuid();
        var req = new PurCreateVendorReturnRequest(rejId, _supplierId, 15000000m, "Xuất trả 30 thùng hàng hỏng cho Vinamilk");
        var res = await _svc.CreateVendorReturnAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(15000000m, res.TotalReturnValueVnd);
        Assert.Equal("PendingCreditMemo", res.CreditMemoStatus);
        Assert.StartsWith("RTV-", res.RtvNumber);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_PUR_039 & UC_PUR_042: Biên bản giao nhận & Xử lý chênh lệch
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateDeliveryProtocolAndSettleDiscrepancy_CalculatesVarianceCorrectly()
    {
        var req = new PurCreateDeliveryProtocolRequest(_grnId, _supplierId, "Trần Văn Bằng", "29C-123.45", 100, 95, 240000m, "AdjustInvoiceAmount");
        var res = await _svc.CreateDeliveryProtocolAndSettleDiscrepancyAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(5, res.DiscrepancyQty); // 100 - 95 = 5
        Assert.Equal(1200000m, res.DiscrepancyAmountVnd); // 5 * 240,000 = 1,200,000
        Assert.Equal("AdjustInvoiceAmount", res.DiscrepancyResolutionAction);
    }
}
