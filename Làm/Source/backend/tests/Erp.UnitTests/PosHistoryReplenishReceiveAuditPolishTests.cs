using Erp.Application.DTOs;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class PosHistoryReplenishReceiveAuditPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PosHistoryReplenishReceiveAuditService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();

    public PosHistoryReplenishReceiveAuditPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pos-history-replenish-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "TPOS184", Name = "Tenant POS 184" });
        _db.SaveChanges();

        _svc = new PosHistoryReplenishReceiveAuditService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_053: Tra cứu lịch sử mua
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCustomerPurchaseHistory_ReturnsCustomerOrders()
    {
        var list = await _svc.GetCustomerPurchaseHistoryAsync(_tenant, _customerId);

        Assert.NotNull(list);
        Assert.NotEmpty(list);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_056: Tạo đề nghị nhập hàng
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateReplenishmentRequest_CreatesStoreReplenishmentOrder()
    {
        var items = new List<PosReplenishmentLineItemDto>
        {
            new(Guid.NewGuid(), "SKU-MILK", "Sữa Tươi NGUYÊN CHẤT 1L", 24),
            new(Guid.NewGuid(), "SKU-BEANS", "Cà Phê Hạt Arabica 1KG", 10)
        };

        var req = new PosCreateReplenishmentRequest("STORE01", "Urgent", items);
        var res = await _svc.CreateReplenishmentRequestAsync(_tenant, _userId, req);

        Assert.NotNull(res);
        Assert.Equal("STORE01", res.StoreCode);
        Assert.Equal("Urgent", res.Priority);
        Assert.Equal(2, res.Items.Count);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_057: Nhận hàng từ kho trung tâm
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ReceiveTransferShipment_ReceivesStockTransferFromCentralWarehouse()
    {
        var items = new List<PosReplenishmentLineItemDto>
        {
            new(Guid.NewGuid(), "SKU-MILK", "Sữa Tươi NGUYÊN CHẤT 1L", 24)
        };

        var req = new PosReceiveTransferShipmentRequest("TRF-2026-001", "STORE01", items, "Hàng nguyên vẹn");
        var res = await _svc.ReceiveTransferShipmentAsync(_tenant, _userId, req);

        Assert.NotNull(res);
        Assert.Equal("TRF-2026-001", res.TransferCode);
        Assert.Equal(24, res.TotalItemsReceived);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_058: Kiểm kê nhanh
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitQuickAudit_SubmitsStoreStockAudit()
    {
        var auditLines = new List<PosQuickAuditLineItemDto>
        {
            new(Guid.NewGuid(), "SKU-MILK", "Sữa Tươi 1L", 24, 22), // Lệch -2
            new(Guid.NewGuid(), "SKU-BEANS", "Cà Phê Hạt 1KG", 10, 10)  // Khớp
        };

        var req = new PosSubmitQuickAuditRequest("STORE01", auditLines);
        var res = await _svc.SubmitQuickAuditAsync(_tenant, _userId, req);

        Assert.NotNull(res);
        Assert.Equal(2, res.TotalItemsAudited);
        Assert.Equal(1, res.DiscrepancyCount);
    }
}
