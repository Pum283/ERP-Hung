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
/// Unit tests cho Bước 73:
///   UC_CRM_076 — Hết hạn báo giá tự động (CheckAndExpireQuotesAsync — Background Job chuyển status)
///   UC_CRM_077 — Chuyển báo giá thành đơn hàng (ConvertQuoteToOrderAsync — Quote-to-Order handover)
///   UC_CRM_078 — In mẫu báo giá (BuildQuotePdfHtmlAsync — HTML/PDF template renderer)
///   UC_CRM_079 — Tạo đơn hàng từ báo giá (ConvertQuoteToOrderAsync — Line item transfer & order creation)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class CrmStep73PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmSalesService _salesSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public CrmStep73PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-step73-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_crm73", DisplayName = "Admin CRM 73" });

        _db.SaveChanges();

        _salesSvc = new CrmSalesService(_db, null!, null!, null!);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_076: Hết hạn báo giá tự động (CheckAndExpireQuotesAsync)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_076_CheckAndExpire_ExpiredDraftQuote_StatusTransitionsToExpired()
    {
        // Tạo báo giá đã quá hạn (ValidUntil = yesterday)
        var quote = await _salesSvc.UpsertQuoteAsync(_tenant, _userAdmin, new CrmQuoteUpsertRequest(
            null, null, null, null, DateTimeOffset.UtcNow.AddDays(-1), 0m, "Báo giá Quá Hạn Draft"));

        var count = await _salesSvc.CheckAndExpireQuotesAsync(_tenant);

        Assert.True(count >= 1);
        var reloaded = await _db.CrmQuotes.FindAsync(quote.Id);
        Assert.Equal("Expired", reloaded?.Status);
        Assert.Contains("[Hệ thống]", reloaded?.Note ?? "");
    }

    [Fact]
    public async Task UC_CRM_076_CheckAndExpire_FutureValidQuote_StatusRemainsUnchanged()
    {
        // Báo giá còn hạn 15 ngày — không bị expire
        var quote = await _salesSvc.UpsertQuoteAsync(_tenant, _userAdmin, new CrmQuoteUpsertRequest(
            null, null, null, null, DateTimeOffset.UtcNow.AddDays(15), 0m, "Báo giá Còn Hạn"));

        var expiredCount = await _salesSvc.CheckAndExpireQuotesAsync(_tenant);

        var reloaded = await _db.CrmQuotes.FindAsync(quote.Id);
        Assert.Equal("Draft", reloaded?.Status); // vẫn Draft, không bị chuyển Expired
    }

    [Fact]
    public async Task UC_CRM_076_CheckAndExpire_AlreadyConvertedQuote_NotAffected()
    {
        // Tạo báo giá đã converted — dù quá hạn cũng không bị expire
        var customerId = Guid.NewGuid();
        _db.CrmCustomers.Add(new CrmCustomer
        {
            Id = customerId, TenantId = _tenant, Code = "KH-076", DisplayName = "KH Converted", Email = "kh076@test.vn", Status = "Active"
        });
        await _db.SaveChangesAsync();

        var quote = await _salesSvc.UpsertQuoteAsync(_tenant, _userAdmin, new CrmQuoteUpsertRequest(
            null, null, customerId, null, DateTimeOffset.UtcNow.AddDays(30), 0m, "Báo giá sẽ Convert"));

        await _salesSvc.UpsertQuoteLineAsync(_tenant, _userAdmin, quote.Id, new CrmQuoteLineUpsertRequest(
            null, "SKU-076", "SP 076", 1, 10000000m));

        // Gửi trước rồi Convert (ConvertQuoteToOrderAsync chấp nhận Draft/Sent)
        var order = await _salesSvc.ConvertQuoteToOrderAsync(_tenant, _userAdmin, quote.Id);
        Assert.NotNull(order);

        // Đặt lại ValidUntil = quá khứ bằng cách update trực tiếp entity
        var entity = await _db.CrmQuotes.FindAsync(quote.Id);
        entity!.ValidUntil = DateTimeOffset.UtcNow.AddDays(-5);
        await _db.SaveChangesAsync();

        var expiredCount = await _salesSvc.CheckAndExpireQuotesAsync(_tenant);
        var reloaded = await _db.CrmQuotes.FindAsync(quote.Id);
        Assert.Equal("Converted", reloaded?.Status); // vẫn Converted, không bị expired
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_077 & UC_CRM_079: Chuyển báo giá thành đơn / Tạo đơn hàng từ báo giá
    // (ConvertQuoteToOrderAsync)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_077_079_ConvertQuoteToOrder_ValidQuoteWithLines_CreatesSalesOrderSuccessfully()
    {
        var customerId = Guid.NewGuid();
        _db.CrmCustomers.Add(new CrmCustomer
        {
            Id = customerId, TenantId = _tenant, Code = "KH-077", DisplayName = "Khách Hàng Đơn Hàng", Status = "Active"
        });
        await _db.SaveChangesAsync();

        var quote = await _salesSvc.UpsertQuoteAsync(_tenant, _userAdmin, new CrmQuoteUpsertRequest(
            null, null, customerId, null, DateTimeOffset.UtcNow.AddDays(30), 0m, "Báo giá Chuyển Đơn"));

        await _salesSvc.UpsertQuoteLineAsync(_tenant, _userAdmin, quote.Id, new CrmQuoteLineUpsertRequest(
            null, "SKU-ORDER-01", "Gói Bản Quyền ERP", 2, 25000000m));

        var order = await _salesSvc.ConvertQuoteToOrderAsync(_tenant, _userAdmin, quote.Id);

        Assert.NotNull(order);
        Assert.Equal(quote.Id, order.QuoteId);
        Assert.Equal("Confirmed", order.Status);
        Assert.True(order.TotalAmount > 0);

        // Kiểm tra quote đã chuyển sang Converted
        var reloadedQuote = await _db.CrmQuotes.FindAsync(quote.Id);
        Assert.Equal("Converted", reloadedQuote?.Status);
        Assert.Equal(order.Id, reloadedQuote?.OrderId);
    }

    [Fact]
    public async Task UC_CRM_077_ConvertQuoteToOrder_AlreadyConverted_ReturnsExistingOrder()
    {
        var customerId = Guid.NewGuid();
        _db.CrmCustomers.Add(new CrmCustomer
        {
            Id = customerId, TenantId = _tenant, Code = "KH-077-2", DisplayName = "KH Đã Converted", Status = "Active"
        });
        await _db.SaveChangesAsync();

        var quote = await _salesSvc.UpsertQuoteAsync(_tenant, _userAdmin, new CrmQuoteUpsertRequest(
            null, null, customerId, null, DateTimeOffset.UtcNow.AddDays(30), 0m, "Báo giá Đã Converted"));

        await _salesSvc.UpsertQuoteLineAsync(_tenant, _userAdmin, quote.Id, new CrmQuoteLineUpsertRequest(
            null, "SKU-02", "Sp Test", 1, 10000000m));

        var order1 = await _salesSvc.ConvertQuoteToOrderAsync(_tenant, _userAdmin, quote.Id);
        var order2 = await _salesSvc.ConvertQuoteToOrderAsync(_tenant, _userAdmin, quote.Id);

        // Gọi lại lần 2 → trả về cùng đơn đã tạo (idempotent)
        Assert.Equal(order1.Id, order2.Id);
    }

    [Fact]
    public async Task UC_CRM_077_ConvertQuoteToOrder_ExpiredQuote_ThrowsAppException()
    {
        var quote = await _salesSvc.UpsertQuoteAsync(_tenant, _userAdmin, new CrmQuoteUpsertRequest(
            null, null, null, null, DateTimeOffset.UtcNow.AddDays(-1), 0m, "Báo giá Expired"));

        // Chạy expire trước
        await _salesSvc.CheckAndExpireQuotesAsync(_tenant);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.ConvertQuoteToOrderAsync(_tenant, _userAdmin, quote.Id));

        Assert.Contains("không hợp lệ", ex.Message);
    }

    [Fact]
    public async Task UC_CRM_077_ConvertQuoteToOrder_NonExistentQuote_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.ConvertQuoteToOrderAsync(_tenant, _userAdmin, Guid.NewGuid()));

        Assert.Equal(404, ex.StatusCode);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_078: In mẫu báo giá (BuildQuotePdfHtmlAsync)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_078_BuildQuotePdfHtml_ValidQuote_ReturnsHtmlContentWithQuoteInfo()
    {
        var quote = await _salesSvc.UpsertQuoteAsync(_tenant, _userAdmin, new CrmQuoteUpsertRequest(
            null, null, null, null, DateTimeOffset.UtcNow.AddDays(30), 0m, "Báo giá In Mẫu HTML"));

        await _salesSvc.UpsertQuoteLineAsync(_tenant, _userAdmin, quote.Id, new CrmQuoteLineUpsertRequest(
            null, "SKU-PRINT", "Sản phẩm In", 3, 5000000m));

        var (fileName, content) = await _salesSvc.BuildQuotePdfHtmlAsync(_tenant, _userAdmin, quote.Id);

        Assert.NotNull(fileName);
        Assert.EndsWith(".html", fileName);
        Assert.Contains("<!DOCTYPE html>", content);
        Assert.Contains("Báo giá Pum's ERP", content);
    }

    [Fact]
    public async Task UC_CRM_078_BuildQuotePdfHtml_NonExistentQuote_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.BuildQuotePdfHtmlAsync(_tenant, _userAdmin, Guid.NewGuid()));

        Assert.Equal(404, ex.StatusCode);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_079: Chi tiết đơn hàng từ báo giá
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_079_GetOrderDetail_AfterConversion_ReturnsOrderWithLineItems()
    {
        var customerId = Guid.NewGuid();
        _db.CrmCustomers.Add(new CrmCustomer
        {
            Id = customerId, TenantId = _tenant, Code = "KH-079", DisplayName = "KH Order Detail", Status = "Active"
        });
        await _db.SaveChangesAsync();

        var quote = await _salesSvc.UpsertQuoteAsync(_tenant, _userAdmin, new CrmQuoteUpsertRequest(
            null, null, customerId, null, DateTimeOffset.UtcNow.AddDays(30), 0m, "Báo giá Order Detail"));

        await _salesSvc.UpsertQuoteLineAsync(_tenant, _userAdmin, quote.Id, new CrmQuoteLineUpsertRequest(
            null, "SKU-DETAIL", "SP Detail", 1, 15000000m));

        var order = await _salesSvc.ConvertQuoteToOrderAsync(_tenant, _userAdmin, quote.Id);
        var detail = await _salesSvc.GetOrderDetailAsync(_tenant, order.Id);

        Assert.NotNull(detail);
        Assert.Equal(order.Code, detail.Order.Code);
        Assert.NotEmpty(detail.Lines);
    }
}
