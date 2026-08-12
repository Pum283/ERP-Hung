using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Pur;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Pur;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 87:
///   UC_PUR_003 — Người liên hệ & điều khoản (UpsertContactAsync)
///   UC_PUR_009 — Gắn sản phẩm – nhà cung cấp (UpsertVendorProductAsync)
///   UC_PUR_014 — Tạo PR từ đơn vị (UpsertPrAsync & UpsertPrLineAsync)
///   UC_PUR_017 — Luồng duyệt PR (SubmitPrAsync & ApprovePrAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class PurStep87PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PurPurchasingService _purSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public PurStep87PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pur-step87-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin87", DisplayName = "Admin 87" });
        _db.SaveChanges();

        _purSvc = new PurPurchasingService(_db);
    }

    public void Dispose() => _db.Dispose();

    private async Task<PurVendorDto> CreateVendorAsync(string code = "VEND-87")
    {
        return await _purSvc.UpsertVendorAsync(_tenant, _userAdmin, new PurVendorUpsertRequest(null, code, "NCC Step 87", null, null, null, null, null, "Active"));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_PUR_003: Người liên hệ & điều khoản
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_PUR_003_UpsertContact_ValidRequest_CreatesContact()
    {
        var vendor = await CreateVendorAsync();

        var contact = await _purSvc.UpsertContactAsync(_tenant, _userAdmin, vendor.Id, new PurVendorContactUpsertRequest(null, "Nguyễn Văn A", "Giám Đốc", "0901234567", "a@ncc.com", true));

        Assert.NotNull(contact);
        Assert.Equal("Nguyễn Văn A", contact.FullName);
        Assert.Equal("Giám Đốc", contact.Title);
        Assert.True(contact.IsPrimary);
    }

    [Fact]
    public async Task UC_PUR_003_UpsertContact_NonExistentVendor_ThrowsException()
    {
        await Assert.ThrowsAsync<AppException>(() =>
            _purSvc.UpsertContactAsync(_tenant, _userAdmin, Guid.NewGuid(), new PurVendorContactUpsertRequest(null, "Tên A", null, null, null, false)));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_PUR_009: Gắn sản phẩm – nhà cung cấp
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_PUR_009_UpsertVendorProduct_ValidMapping_SavesProductMapping()
    {
        var vendor = await CreateVendorAsync();

        var vp = await _purSvc.UpsertVendorProductAsync(_tenant, _userAdmin, vendor.Id, new PurVendorProductUpsertRequest(null, "SKU-PAPER", "Giấy In A4", true));

        Assert.NotNull(vp);
        Assert.Equal("SKU-PAPER", vp.ProductCode);
        Assert.True(vp.IsPreferred);
    }

    [Fact]
    public async Task UC_PUR_009_UpsertVendorPrice_ValidPrice_SavesPrice()
    {
        var vendor = await CreateVendorAsync();

        var price = await _purSvc.UpsertVendorPriceAsync(_tenant, _userAdmin, vendor.Id, new PurVendorPriceUpsertRequest(null, "SKU-PAPER", "Giấy In A4", 80000m, "VND", DateOnly.FromDateTime(DateTime.UtcNow), null));

        Assert.NotNull(price);
        Assert.Equal(80000m, price.UnitPrice);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_PUR_014: Tạo PR từ đơn vị
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_PUR_014_UpsertPr_ValidRequest_CreatesDraftPr()
    {
        var pr = await _purSvc.UpsertPrAsync(_tenant, _userAdmin, new PurPurchaseRequestUpsertRequest(null, "PR-87-01", "Phòng IT", "Cần mua máy in"));

        Assert.NotNull(pr);
        Assert.Equal("Draft", pr.Status);
        Assert.Equal("PR-87-01", pr.Code);
    }

    [Fact]
    public async Task UC_PUR_014_UpsertPrLine_AddsLineToPr()
    {
        var pr = await _purSvc.UpsertPrAsync(_tenant, _userAdmin, new PurPurchaseRequestUpsertRequest(null, "PR-87-02", "Phòng KT", null));
        var line = await _purSvc.UpsertPrLineAsync(_tenant, _userAdmin, pr.Id, new PurPrLineUpsertRequest(null, "SKU-01", "Bút Viết", 10, "Cây", null));

        Assert.NotNull(line);
        Assert.Equal(10m, line.Qty);

        var updatedPr = await _purSvc.GetPrDetailAsync(_tenant, pr.Id);
        Assert.Single(updatedPr.Lines);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_PUR_017: Luồng duyệt PR
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_PUR_017_SubmitPr_WithLines_TransitionsToSubmitted()
    {
        var pr = await _purSvc.UpsertPrAsync(_tenant, _userAdmin, new PurPurchaseRequestUpsertRequest(null, "PR-87-03", null, null));
        await _purSvc.UpsertPrLineAsync(_tenant, _userAdmin, pr.Id, new PurPrLineUpsertRequest(null, "SKU-01", "SP 01", 5, "Cái", null));

        var submittedPr = await _purSvc.SubmitPrAsync(_tenant, _userAdmin, pr.Id);

        Assert.Equal("Submitted", submittedPr.Status);
    }

    [Fact]
    public async Task UC_PUR_017_SubmitPr_NoLines_ThrowsException()
    {
        var pr = await _purSvc.UpsertPrAsync(_tenant, _userAdmin, new PurPurchaseRequestUpsertRequest(null, "PR-RONG", null, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _purSvc.SubmitPrAsync(_tenant, _userAdmin, pr.Id));
        Assert.Contains("ít nhất 1 dòng", ex.Message);
    }

    [Fact]
    public async Task UC_PUR_017_ApprovePr_SubmittedPr_TransitionsToApproved()
    {
        var pr = await _purSvc.UpsertPrAsync(_tenant, _userAdmin, new PurPurchaseRequestUpsertRequest(null, "PR-87-04", null, null));
        await _purSvc.UpsertPrLineAsync(_tenant, _userAdmin, pr.Id, new PurPrLineUpsertRequest(null, "SKU-01", "SP 01", 5, "Cái", null));
        await _purSvc.SubmitPrAsync(_tenant, _userAdmin, pr.Id);

        var approvedPr = await _purSvc.ApprovePrAsync(_tenant, _userAdmin, pr.Id, new PurPrDecisionRequest("Duyệt mua hàng"));

        Assert.Equal("Approved", approvedPr.Status);
        Assert.NotNull(approvedPr.DecidedAt);
    }

    [Fact]
    public async Task UC_PUR_017_ApprovePr_DraftPr_ThrowsException()
    {
        var pr = await _purSvc.UpsertPrAsync(_tenant, _userAdmin, new PurPurchaseRequestUpsertRequest(null, "PR-DRAFT", null, null));

        await Assert.ThrowsAsync<AppException>(() =>
            _purSvc.ApprovePrAsync(_tenant, _userAdmin, pr.Id, new PurPrDecisionRequest(null)));
    }
}
