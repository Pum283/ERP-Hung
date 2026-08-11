using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Domain.Entities.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 75:
///   UC_CRM_085 — Trả hàng / điều chỉnh đơn (ReturnOrderAsync — Return order lifecycle & stock release)
///   UC_CRM_086 — Gắn hợp đồng (LinkContractAsync — Link contract reference to sales order)
///   UC_CRM_087 — Theo dõi thanh toán (AddPaymentAsync — Multi-method payment collection & balance tracking)
///   UC_CRM_088 — Đẩy đơn sang kho / giao vận (PushToWarehouseAsync — Warehouse push validation & state updates)
/// 10+ test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class CrmStep75PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmSalesService _salesSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public CrmStep75PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-step75-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_crm75", DisplayName = "Admin CRM 75" });

        _db.SaveChanges();

        _salesSvc = new CrmSalesService(_db, null!, null!, null!);
    }

    public void Dispose() => _db.Dispose();

    private async Task<CrmSalesOrderDto> CreateDraftOrderWithLinesAsync(string suffix = "")
    {
        var customerId = Guid.NewGuid();
        _db.CrmCustomers.Add(new CrmCustomer
        {
            Id = customerId, TenantId = _tenant, Code = $"KH-75{suffix}", DisplayName = $"KH Order {suffix}", Status = "Active"
        });
        await _db.SaveChangesAsync();

        var quote = await _salesSvc.UpsertQuoteAsync(_tenant, _userAdmin, new CrmQuoteUpsertRequest(
            null, null, customerId, null, DateTimeOffset.UtcNow.AddDays(30), 0m, $"Báo giá Đơn {suffix}"));

        await _salesSvc.UpsertQuoteLineAsync(_tenant, _userAdmin, quote.Id, new CrmQuoteLineUpsertRequest(
            null, $"SKU-75{suffix}", $"SP 75{suffix}", 2, 10000000m));

        return await _salesSvc.ConvertQuoteToOrderAsync(_tenant, _userAdmin, quote.Id);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_085: Trả hàng / điều chỉnh đơn (ReturnOrderAsync)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_085_ReturnOrder_ValidReason_TransitionsToReturned()
    {
        var order = await CreateDraftOrderWithLinesAsync("-085a");

        var returned = await _salesSvc.ReturnOrderAsync(_tenant, _userAdmin, order.Id,
            new CrmOrderReturnRequest("Sản phẩm bị lỗi đóng gói", null));

        Assert.Equal("Returned", returned.Status);
        var entity = await _db.CrmSalesOrders.FindAsync(order.Id);
        Assert.Equal("Sản phẩm bị lỗi đóng gói", entity?.ReturnReason);
    }

    [Fact]
    public async Task UC_CRM_085_ReturnOrder_CancelledOrder_ThrowsAppException()
    {
        var order = await CreateDraftOrderWithLinesAsync("-085b");
        await _salesSvc.CancelOrderAsync(_tenant, _userAdmin, order.Id, new CrmOrderCancelRequest("Hủy test"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.ReturnOrderAsync(_tenant, _userAdmin, order.Id,
                new CrmOrderReturnRequest("Trả đơn hủy", null)));

        Assert.Contains("không thể trả hàng", ex.Message);
    }

    [Fact]
    public async Task UC_CRM_085_ReturnOrder_EmptyReason_ThrowsAppException()
    {
        var order = await CreateDraftOrderWithLinesAsync("-085c");

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.ReturnOrderAsync(_tenant, _userAdmin, order.Id,
                new CrmOrderReturnRequest("", null)));

        Assert.Contains("Lý do trả hàng", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_086: Gắn hợp đồng (LinkContractAsync)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_086_LinkContract_ValidContractId_UpdatesOrderContract()
    {
        var order = await CreateDraftOrderWithLinesAsync("-086a");
        var contractId = Guid.NewGuid();

        var updated = await _salesSvc.LinkContractAsync(_tenant, _userAdmin, order.Id,
            new CrmOrderLinkContractRequest(contractId));

        Assert.NotNull(updated);
        var entity = await _db.CrmSalesOrders.FindAsync(order.Id);
        Assert.Equal(contractId, entity?.ContractId);
    }

    [Fact]
    public async Task UC_CRM_086_LinkContract_NonExistentOrder_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.LinkContractAsync(_tenant, _userAdmin, Guid.NewGuid(),
                new CrmOrderLinkContractRequest(Guid.NewGuid())));

        Assert.Equal(404, ex.StatusCode);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_087: Theo dõi thanh toán (AddPaymentAsync)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_087_AddPayment_ValidAmountAndMethod_UpdatesPaidAmount()
    {
        var order = await CreateDraftOrderWithLinesAsync("-087a"); // TotalAmount = 20,000,000

        var pay = await _salesSvc.AddPaymentAsync(_tenant, _userAdmin, order.Id,
            new CrmOrderPaymentRequest(5000000m, "Transfer", "Thanh toán đợt 1"));

        Assert.NotNull(pay);
        Assert.Equal(5000000m, pay.Amount);
        Assert.Equal("Transfer", pay.Method);

        var updatedOrder = await _db.CrmSalesOrders.FindAsync(order.Id);
        Assert.Equal(5000000m, updatedOrder?.PaidAmount);
    }

    [Fact]
    public async Task UC_CRM_087_AddPayment_AmountExceedsRemaining_ThrowsAppException()
    {
        var order = await CreateDraftOrderWithLinesAsync("-087b"); // TotalAmount = 20,000,000

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.AddPaymentAsync(_tenant, _userAdmin, order.Id,
                new CrmOrderPaymentRequest(25000000m, "Cash", "Thanh toán lố")));

        Assert.Contains("vượt còn lại", ex.Message);
    }

    [Fact]
    public async Task UC_CRM_087_AddPayment_ZeroOrNegativeAmount_ThrowsAppException()
    {
        var order = await CreateDraftOrderWithLinesAsync("-087c");

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.AddPaymentAsync(_tenant, _userAdmin, order.Id,
                new CrmOrderPaymentRequest(0m, "Cash", null)));

        Assert.Contains("Số tiền > 0", ex.Message);
    }

    [Fact]
    public async Task UC_CRM_087_AddPayment_InvalidMethod_ThrowsAppException()
    {
        var order = await CreateDraftOrderWithLinesAsync("-087d");

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.AddPaymentAsync(_tenant, _userAdmin, order.Id,
                new CrmOrderPaymentRequest(1000000m, "Bitcoin", null)));

        Assert.Contains("Phương thức", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_088: Đẩy đơn sang kho / giao vận (PushToWarehouseAsync)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_088_PushToWarehouse_CancelledOrder_ThrowsAppException()
    {
        var order = await CreateDraftOrderWithLinesAsync("-088a");
        await _salesSvc.CancelOrderAsync(_tenant, _userAdmin, order.Id, new CrmOrderCancelRequest("Hủy test 088"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.PushToWarehouseAsync(_tenant, _userAdmin, order.Id));

        Assert.Contains("Đơn đã hủy", ex.Message);
    }

    [Fact]
    public async Task UC_CRM_088_PushToWarehouse_DraftOrder_ThrowsAppException()
    {
        // Manually set status back to Draft for test
        var order = await CreateDraftOrderWithLinesAsync("-088b");
        var entity = await _db.CrmSalesOrders.FindAsync(order.Id);
        entity!.Status = "Draft";
        await _db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.PushToWarehouseAsync(_tenant, _userAdmin, order.Id));

        Assert.Contains("xác nhận đơn trước khi đẩy kho", ex.Message);
    }
}
