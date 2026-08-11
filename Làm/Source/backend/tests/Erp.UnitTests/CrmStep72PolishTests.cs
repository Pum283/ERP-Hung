using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 72:
///   UC_CRM_072 — Áp chính sách giá / bảng giá (Price List Binding & Dynamic Rate Application)
///   UC_CRM_073 — Xin duyệt chiết khấu (Discount Approval Workflow & Threshold Check)
///   UC_CRM_074 — Gửi báo giá PDF/email (PDF Generation & Multi-channel Quote Dispatch)
///   UC_CRM_075 — Phiên bản báo giá (Quote Revision History & Versioning)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class CrmStep72PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmSalesService _salesSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public CrmStep72PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-step72-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_crm72", DisplayName = "Admin CRM 72" });

        _db.SaveChanges();

        _salesSvc = new CrmSalesService(_db, null!, null!, null!);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_072: Áp chính sách giá / bảng giá
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_072_ApplyPriceList_ValidPriceList_UpdatesQuotePriceList()
    {
        var quote = await _salesSvc.UpsertQuoteAsync(_tenant, _userAdmin, new CrmQuoteUpsertRequest(
            null, null, null, null, DateTimeOffset.UtcNow.AddDays(30), 0m, "Báo giá Áp Bảng Giá"));

        var priceListId = Guid.NewGuid();
        _db.CrmPriceLists.Add(new Erp.Domain.Entities.Crm.CrmPriceList
        {
            Id = priceListId, TenantId = _tenant, Name = "Bảng giá Chuẩn", Status = "Active"
        });
        await _db.SaveChangesAsync();

        var updated = await _salesSvc.ApplyPriceListAsync(_tenant, _userAdmin, quote.Id, priceListId);

        Assert.NotNull(updated);
        Assert.Equal(priceListId, updated.PriceListId);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_073: Xin duyệt chiết khấu
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_073_RequestDiscount_HighDiscount_TransitionsStatusToPendingDiscount()
    {
        var quote = await _salesSvc.UpsertQuoteAsync(_tenant, _userAdmin, new CrmQuoteUpsertRequest(
            null, null, null, null, DateTimeOffset.UtcNow.AddDays(30), 0m, "Báo giá Xin Duyệt CK"));

        var requested = await _salesSvc.RequestDiscountAsync(_tenant, _userAdmin, quote.Id, new CrmQuoteDiscountRequest(
            25m, "Chiết khấu cho khách hàng thân thiết dự án lớn"));

        Assert.NotNull(requested);
        Assert.Equal("PendingDiscount", requested.Status);
        Assert.Equal(25m, requested.DiscountPercent);
    }

    [Fact]
    public async Task UC_CRM_073_DecideDiscount_Approved_TransitionsStatusToDraft()
    {
        var quote = await _salesSvc.UpsertQuoteAsync(_tenant, _userAdmin, new CrmQuoteUpsertRequest(
            null, null, null, null, DateTimeOffset.UtcNow.AddDays(30), 0m, "Báo giá Duyệt CK"));

        await _salesSvc.RequestDiscountAsync(_tenant, _userAdmin, quote.Id, new CrmQuoteDiscountRequest(
            20m, "Xin duyệt 20%"));

        var decided = await _salesSvc.DecideDiscountAsync(_tenant, _userAdmin, quote.Id, new CrmQuoteDiscountDecisionRequest(
            true, "Đồng ý duyệt mức chiết khấu 20%"));

        Assert.NotNull(decided);
        Assert.Equal("Draft", decided.Status);
        Assert.Equal("Approved", decided.DiscountApprovalStatus);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_074: Gửi báo giá PDF/email
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_074_SendQuote_EmailChannel_TransitionsStatusToSent()
    {
        var customerId = Guid.NewGuid();
        _db.CrmCustomers.Add(new Erp.Domain.Entities.Crm.CrmCustomer
        {
            Id = customerId, TenantId = _tenant, Code = "KH-72", DisplayName = "Khách Hàng Email Valid", Email = "client72@growth.vn", Status = "Active"
        });
        await _db.SaveChangesAsync();

        var quote = await _salesSvc.UpsertQuoteAsync(_tenant, _userAdmin, new CrmQuoteUpsertRequest(
            null, null, customerId, null, DateTimeOffset.UtcNow.AddDays(30), 0m, "Báo giá Gửi Email"));

        await _salesSvc.UpsertQuoteLineAsync(_tenant, _userAdmin, quote.Id, new CrmQuoteLineUpsertRequest(
            null, "SKU-01", "Gói Dịch Vụ ERP", 1, 50000000m));

        var sent = await _salesSvc.SendQuoteAsync(_tenant, _userAdmin, quote.Id, new CrmQuoteSendRequest("Email"));

        Assert.NotNull(sent);
        Assert.Equal("Sent", sent.Status);
    }

    [Fact]
    public async Task UC_CRM_074_SendQuote_InvalidChannel_ThrowsAppException()
    {
        var quote = await _salesSvc.UpsertQuoteAsync(_tenant, _userAdmin, new CrmQuoteUpsertRequest(
            null, null, null, null, DateTimeOffset.UtcNow.AddDays(30), 0m, "Báo giá Kênh Lỗi"));

        await _salesSvc.UpsertQuoteLineAsync(_tenant, _userAdmin, quote.Id, new CrmQuoteLineUpsertRequest(
            null, "SKU-01", "Gói Dịch Vụ ERP", 1, 50000000m));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.SendQuoteAsync(_tenant, _userAdmin, quote.Id, new CrmQuoteSendRequest("InvalidChannel")));

        Assert.Contains("Kênh gửi: Email", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_075: Phiên bản báo giá
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_075_CreateNewVersion_ValidQuote_IncrementsVersionNumber()
    {
        var quote = await _salesSvc.UpsertQuoteAsync(_tenant, _userAdmin, new CrmQuoteUpsertRequest(
            null, null, null, null, DateTimeOffset.UtcNow.AddDays(30), 0m, "Báo giá Gốc"));

        var rev = await _salesSvc.CreateNewVersionAsync(_tenant, _userAdmin, quote.Id);

        Assert.NotNull(rev);
        Assert.Equal(2, rev.Version);
        Assert.Equal("Draft", rev.Status);
    }

    [Fact]
    public async Task UC_CRM_072_ApplyPriceList_NonExistentQuote_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.ApplyPriceListAsync(_tenant, _userAdmin, Guid.NewGuid(), Guid.NewGuid()));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC_CRM_073_RequestDiscount_InvalidPercent_ThrowsAppException()
    {
        var quote = await _salesSvc.UpsertQuoteAsync(_tenant, _userAdmin, new CrmQuoteUpsertRequest(
            null, null, null, null, DateTimeOffset.UtcNow.AddDays(30), 0m, "Báo giá CK Lỗi"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.RequestDiscountAsync(_tenant, _userAdmin, quote.Id, new CrmQuoteDiscountRequest(
                -10m, "Chiết khấu âm")));

        Assert.Contains("Chiết khấu 0–100%", ex.Message);
    }

    [Fact]
    public async Task UC_CRM_074_SendQuote_NonExistentQuote_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.SendQuoteAsync(_tenant, _userAdmin, Guid.NewGuid(), new CrmQuoteSendRequest("Email")));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC_CRM_075_CreateNewVersion_NonExistentQuote_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.CreateNewVersionAsync(_tenant, _userAdmin, Guid.NewGuid()));

        Assert.Equal(404, ex.StatusCode);
    }
}
