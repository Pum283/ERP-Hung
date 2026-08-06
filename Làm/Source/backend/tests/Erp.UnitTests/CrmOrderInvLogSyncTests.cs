using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Application.DTOs.Fin;
using Erp.Application.Interfaces.Services.Fin;
using Erp.Domain.Entities.Crm;
using Erp.Domain.Entities.Inv;
using Erp.Infrastructure.Implementations.Services.Crm;
using Erp.Infrastructure.Implementations.Services.Inv;
using Erp.Infrastructure.Implementations.Services.Log;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.UnitTests;

/// <summary>UC_CRM_082/088 — giữ tồn INV reservation thật + đẩy đơn sang LOG thật.</summary>
public sealed class CrmOrderInvLogSyncTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmSalesService _svc;
    private readonly Guid _tenant = Guid.Parse("bbbbbbbb-cccc-dddd-eeee-ffffffffffff");
    private readonly Guid _user = Guid.Parse("22222222-3333-4444-5555-666666666666");

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

    public CrmOrderInvLogSyncTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-inv-log-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        var noop = new NoopFinRevenue();
        _svc = new CrmSalesService(_db, noop, new InvStockService(_db, noop), new LogLogisticsService(_db));
    }

    public void Dispose() => _db.Dispose();

    private (Guid WarehouseId, Guid SkuId) SeedInv(decimal onHand = 100)
    {
        var wh = new InvWarehouse { TenantId = _tenant, Code = "WH1", Name = "Kho 1", Status = "Active", CreatedBy = _user };
        var uom = new InvUnitOfMeasure { TenantId = _tenant, Code = "CAI", Name = "Cái", CreatedBy = _user };
        _db.InvWarehouses.Add(wh);
        _db.InvUnitsOfMeasure.Add(uom);
        var sku = new InvSku
        {
            TenantId = _tenant, Code = "SP-A", Name = "SP A",
            BaseUnitId = uom.Id, Status = "Active", CreatedBy = _user,
        };
        _db.InvSkus.Add(sku);
        _db.InvStockBalances.Add(new InvStockBalance
        {
            TenantId = _tenant, WarehouseId = wh.Id, SkuId = sku.Id,
            LotCode = "", QtyOnHand = onHand, CreatedBy = _user,
        });
        return (wh.Id, sku.Id);
    }

    private CrmSalesOrder SeedOrder(string status = "Confirmed", bool withLine = true, string itemCode = "SP-A")
    {
        var cust = new CrmCustomer
        {
            TenantId = _tenant, Code = "KH1", DisplayName = "Khách Một",
            Phone = "0900000001", Address = "1 Lê Lợi", CreatedBy = _user,
        };
        _db.CrmCustomers.Add(cust);
        var order = new CrmSalesOrder
        {
            TenantId = _tenant, Code = "SO-T1", CustomerId = cust.Id, Status = status,
            SubTotal = 250_000, TotalAmount = 250_000,
            StockHoldStatus = "None", WarehousePushStatus = "None", CreatedBy = _user,
        };
        _db.CrmSalesOrders.Add(order);
        if (withLine)
        {
            _db.CrmSalesOrderLines.Add(new CrmSalesOrderLine
            {
                TenantId = _tenant, OrderId = order.Id, ItemCode = itemCode, ItemName = "SP A",
                Quantity = 5, UnitPrice = 50_000, LineAmount = 250_000, LineNo = 1, CreatedBy = _user,
            });
        }
        return order;
    }

    // ── UC_CRM_082 giữ tồn ──

    [Fact]
    public async Task HoldStock_CreatesActiveReservationAndIncrementsReserved()
    {
        var (whId, skuId) = SeedInv();
        var order = SeedOrder();
        await _db.SaveChangesAsync();

        var result = await _svc.HoldStockAsync(_tenant, _user, order.Id);

        Assert.Equal("Held", result.StockHoldStatus);
        Assert.Equal("Holding", result.Status);
        var rv = await _db.InvStockReservations.SingleAsync(
            x => x.RefModule == "CRM" && x.RefId == order.Id);
        Assert.Equal("Active", rv.Status);
        Assert.Equal(whId, rv.WarehouseId);
        var bal = await _db.InvStockBalances.SingleAsync(x => x.SkuId == skuId);
        Assert.Equal(5, bal.QtyReserved);
        Assert.Contains("Giữ tồn RV-", (await _db.CrmSalesOrders.SingleAsync(x => x.Id == order.Id)).Note);
    }

    [Fact]
    public async Task HoldStock_IsIdempotent()
    {
        var (_, skuId) = SeedInv();
        var order = SeedOrder();
        await _db.SaveChangesAsync();

        await _svc.HoldStockAsync(_tenant, _user, order.Id);
        await _svc.HoldStockAsync(_tenant, _user, order.Id);

        Assert.Equal(1, await _db.InvStockReservations.CountAsync(x => x.RefId == order.Id));
        Assert.Equal(5, (await _db.InvStockBalances.SingleAsync(x => x.SkuId == skuId)).QtyReserved);
    }

    [Fact]
    public async Task HoldStock_FailsWhenInsufficientAvailable()
    {
        var (_, skuId) = SeedInv(onHand: 2);
        var order = SeedOrder();
        await _db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.HoldStockAsync(_tenant, _user, order.Id));
        Assert.Contains("Giữ tồn thất bại", ex.Message);
        Assert.Equal(0, (await _db.InvStockBalances.SingleAsync(x => x.SkuId == skuId)).QtyReserved);
        Assert.Equal("None", (await _db.CrmSalesOrders.SingleAsync(x => x.Id == order.Id)).StockHoldStatus);
    }

    [Fact]
    public async Task HoldStock_FailsWhenNoSkuMatches()
    {
        SeedInv();
        var order = SeedOrder(itemCode: "SP-KHONG-CO");
        await _db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.HoldStockAsync(_tenant, _user, order.Id));
        Assert.Contains("khớp SKU", ex.Message);
    }

    [Fact]
    public async Task CancelOrder_ReleasesActiveReservation()
    {
        var (_, skuId) = SeedInv();
        var order = SeedOrder();
        await _db.SaveChangesAsync();
        await _svc.HoldStockAsync(_tenant, _user, order.Id);

        await _svc.CancelOrderAsync(_tenant, _user, order.Id, new CrmOrderCancelRequest("Khách hủy"));

        var rv = await _db.InvStockReservations.SingleAsync(x => x.RefId == order.Id);
        Assert.Equal("Released", rv.Status);
        Assert.Equal(0, (await _db.InvStockBalances.SingleAsync(x => x.SkuId == skuId)).QtyReserved);
    }

    // ── UC_CRM_088 đẩy kho / LOG ──

    [Fact]
    public async Task PushWarehouse_CreatesConfirmedLogDelivery()
    {
        SeedInv();
        var order = SeedOrder();
        await _db.SaveChangesAsync();

        var result = await _svc.PushToWarehouseAsync(_tenant, _user, order.Id);

        Assert.Equal("Pushed", result.WarehousePushStatus);
        Assert.Equal("Released", result.Status);
        var dg = await _db.LogDeliveryOrders.SingleAsync(x => x.SourceOrderCode == "SO-T1");
        Assert.Equal("Confirmed", dg.Status);
        Assert.Equal("Khách Một", dg.CustomerName);
        Assert.Equal("0900000001", dg.Phone);
        var line = await _db.LogDeliveryLines.SingleAsync(x => x.DeliveryOrderId == dg.Id);
        Assert.Equal("SP-A", line.ProductCode);
        Assert.Equal(5, line.Qty);
        Assert.Contains($"LOG {dg.Code}", (await _db.CrmSalesOrders.SingleAsync(x => x.Id == order.Id)).Note);
    }

    [Fact]
    public async Task PushWarehouse_IsIdempotent()
    {
        SeedInv();
        var order = SeedOrder();
        await _db.SaveChangesAsync();

        await _svc.PushToWarehouseAsync(_tenant, _user, order.Id);
        await _svc.PushToWarehouseAsync(_tenant, _user, order.Id);

        Assert.Equal(1, await _db.LogDeliveryOrders.CountAsync(x => x.SourceOrderCode == "SO-T1"));
    }

    [Fact]
    public async Task PushWarehouse_ReleasesHeldReservation()
    {
        var (_, skuId) = SeedInv();
        var order = SeedOrder();
        await _db.SaveChangesAsync();
        await _svc.HoldStockAsync(_tenant, _user, order.Id);

        var result = await _svc.PushToWarehouseAsync(_tenant, _user, order.Id);

        Assert.Equal("Released", result.StockHoldStatus);
        Assert.Equal("Released", (await _db.InvStockReservations.SingleAsync(x => x.RefId == order.Id)).Status);
        Assert.Equal(0, (await _db.InvStockBalances.SingleAsync(x => x.SkuId == skuId)).QtyReserved);
    }

    [Fact]
    public async Task PushWarehouse_RejectsDraftOrder()
    {
        SeedInv();
        var order = SeedOrder(status: "Draft");
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<AppException>(() => _svc.PushToWarehouseAsync(_tenant, _user, order.Id));
    }

    [Fact]
    public async Task PushWarehouse_RejectsOrderWithoutLines()
    {
        SeedInv();
        var order = SeedOrder(withLine: false);
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<AppException>(() => _svc.PushToWarehouseAsync(_tenant, _user, order.Id));
        Assert.Equal(0, await _db.LogDeliveryOrders.CountAsync());
    }
}
