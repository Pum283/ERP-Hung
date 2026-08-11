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
/// Unit tests cho Bước 74:
///   UC_CRM_081 — Cập nhật trạng thái đơn (SetOrderStatusAsync — Status transition lifecycle)
///   UC_CRM_082 — Giữ tồn khi duyệt đơn (HoldStockAsync — INV reservation integration)
///   UC_CRM_083 — Tách / gộp đơn (SplitOrderAsync & MergeOrdersAsync — Order restructuring)
///   UC_CRM_084 — Hủy đơn có kiểm soát (CancelOrderAsync — Controlled cancellation with reason & stock release)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class CrmStep74PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmSalesService _salesSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public CrmStep74PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-step74-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_crm74", DisplayName = "Admin CRM 74" });

        _db.SaveChanges();

        // Note: HoldStock cần INV service thật (IInvStockService) — ở test đơn lẻ truyền null!
        // Các test HoldStock sẽ test edge cases không cần gọi _inv.
        _salesSvc = new CrmSalesService(_db, null!, null!, null!);
    }

    public void Dispose() => _db.Dispose();

    /// <summary>Tạo đơn hàng Draft với lines sẵn để test.</summary>
    private async Task<CrmSalesOrderDto> CreateDraftOrderWithLinesAsync(string suffix = "")
    {
        var customerId = Guid.NewGuid();
        _db.CrmCustomers.Add(new CrmCustomer
        {
            Id = customerId, TenantId = _tenant, Code = $"KH-74{suffix}", DisplayName = $"KH Order {suffix}", Status = "Active"
        });
        await _db.SaveChangesAsync();

        var quote = await _salesSvc.UpsertQuoteAsync(_tenant, _userAdmin, new CrmQuoteUpsertRequest(
            null, null, customerId, null, DateTimeOffset.UtcNow.AddDays(30), 0m, $"Báo giá Đơn {suffix}"));

        await _salesSvc.UpsertQuoteLineAsync(_tenant, _userAdmin, quote.Id, new CrmQuoteLineUpsertRequest(
            null, $"SKU-74{suffix}", $"SP 74{suffix}", 2, 10000000m));

        return await _salesSvc.ConvertQuoteToOrderAsync(_tenant, _userAdmin, quote.Id);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_081: Cập nhật trạng thái đơn (SetOrderStatusAsync)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_081_SetOrderStatus_ConfirmedToHolding_TransitionsSuccessfully()
    {
        var order = await CreateDraftOrderWithLinesAsync("-081a");

        // Order được tạo ở status "Confirmed" từ ConvertQuoteToOrderAsync
        var updated = await _salesSvc.SetOrderStatusAsync(_tenant, _userAdmin, order.Id,
            new CrmOrderStatusRequest("Holding"));

        Assert.Equal("Holding", updated.Status);
        Assert.Equal("Held", updated.StockHoldStatus);
    }

    [Fact]
    public async Task UC_CRM_081_SetOrderStatus_InvalidStatus_ThrowsAppException()
    {
        var order = await CreateDraftOrderWithLinesAsync("-081b");

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.SetOrderStatusAsync(_tenant, _userAdmin, order.Id,
                new CrmOrderStatusRequest("InvalidFoo")));

        Assert.Contains("không hợp lệ", ex.Message);
    }

    [Fact]
    public async Task UC_CRM_081_SetOrderStatus_CannotUseCancelledViaSetStatus_ThrowsAppException()
    {
        // Phải dùng API CancelOrderAsync, không được đặt "Cancelled" qua SetOrderStatusAsync
        var order = await CreateDraftOrderWithLinesAsync("-081c");

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.SetOrderStatusAsync(_tenant, _userAdmin, order.Id,
                new CrmOrderStatusRequest("Cancelled")));

        Assert.Contains("hủy đơn có lý do", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_082: Giữ tồn khi duyệt đơn (HoldStockAsync)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_082_HoldStock_CancelledOrder_ThrowsAppException()
    {
        var order = await CreateDraftOrderWithLinesAsync("-082a");

        // Hủy đơn trước
        await _salesSvc.CancelOrderAsync(_tenant, _userAdmin, order.Id,
            new CrmOrderCancelRequest("Test hủy để kiểm tra hold stock"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.HoldStockAsync(_tenant, _userAdmin, order.Id));

        Assert.Contains("Không giữ tồn", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_083: Tách / gộp đơn (SplitOrderAsync & MergeOrdersAsync)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_083_SplitOrder_ValidLines_CreatesNewOrderWithMovedLines()
    {
        var order = await CreateDraftOrderWithLinesAsync("-083a");

        // Lấy danh sách lines hiện có
        var detail = await _salesSvc.GetOrderDetailAsync(_tenant, order.Id);
        Assert.NotEmpty(detail.Lines);

        var lineIds = detail.Lines.Select(l => l.Id).ToList();
        var newOrder = await _salesSvc.SplitOrderAsync(_tenant, _userAdmin, order.Id,
            new CrmOrderSplitRequest(lineIds));

        Assert.NotNull(newOrder);
        Assert.NotEqual(order.Id, newOrder.Id);
        Assert.Equal("Draft", newOrder.Status);
        Assert.Contains("-S1", newOrder.Code);
    }

    [Fact]
    public async Task UC_CRM_083_SplitOrder_EmptyLineIds_ThrowsAppException()
    {
        var order = await CreateDraftOrderWithLinesAsync("-083b");

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.SplitOrderAsync(_tenant, _userAdmin, order.Id,
                new CrmOrderSplitRequest(new List<Guid>())));

        Assert.Contains("ít nhất 1 dòng", ex.Message);
    }

    [Fact]
    public async Task UC_CRM_083_MergeOrders_TwoValidOrders_CombinesLinesIntoPrimary()
    {
        var order1 = await CreateDraftOrderWithLinesAsync("-083c1");
        var order2 = await CreateDraftOrderWithLinesAsync("-083c2");

        var merged = await _salesSvc.MergeOrdersAsync(_tenant, _userAdmin,
            new CrmOrderMergeRequest(order1.Id, order2.Id, "Gộp test 083"));

        Assert.Equal(order1.Id, merged.Id);
        Assert.True(merged.TotalAmount >= order1.TotalAmount);

        // Đơn phụ phải bị Cancelled
        var secondary = await _db.CrmSalesOrders.FindAsync(order2.Id);
        Assert.Equal("Cancelled", secondary?.Status);
    }

    [Fact]
    public async Task UC_CRM_083_MergeOrders_SameOrderId_ThrowsAppException()
    {
        var order = await CreateDraftOrderWithLinesAsync("-083d");

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.MergeOrdersAsync(_tenant, _userAdmin,
                new CrmOrderMergeRequest(order.Id, order.Id, null)));

        Assert.Contains("khác nhau", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_084: Hủy đơn có kiểm soát (CancelOrderAsync)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_084_CancelOrder_ValidOrder_TransitionsToCancelledWithReason()
    {
        var order = await CreateDraftOrderWithLinesAsync("-084a");

        var cancelled = await _salesSvc.CancelOrderAsync(_tenant, _userAdmin, order.Id,
            new CrmOrderCancelRequest("Khách hàng yêu cầu hủy đơn"));

        Assert.Equal("Cancelled", cancelled.Status);
        Assert.Equal("Khách hàng yêu cầu hủy đơn", cancelled.CancelReason);
    }

    [Fact]
    public async Task UC_CRM_084_CancelOrder_AlreadyCancelled_ThrowsAppException()
    {
        var order = await CreateDraftOrderWithLinesAsync("-084b");

        await _salesSvc.CancelOrderAsync(_tenant, _userAdmin, order.Id,
            new CrmOrderCancelRequest("Hủy lần 1"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.CancelOrderAsync(_tenant, _userAdmin, order.Id,
                new CrmOrderCancelRequest("Hủy lần 2")));

        Assert.Contains("đã hủy", ex.Message);
    }
}
