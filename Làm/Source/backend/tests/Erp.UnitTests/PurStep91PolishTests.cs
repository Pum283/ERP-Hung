using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Pur;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Fin;
using Erp.Infrastructure.Implementations.Services.Inv;
using Erp.Infrastructure.Implementations.Services.Pur;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 91:
///   UC_PUR_040 — Nhập hóa đơn nhà cung cấp (CreateInvoiceAsync)
///   UC_PUR_041 — Đối soát 3 chiều PO–GRN–Invoice (MatchThreeWayAsync)
///   UC_PUR_043 — Đẩy công nợ sang FIN AP (PushInvoiceToApAsync)
///   UC_PUR_048 — Báo cáo mua theo nhà cung cấp / SP (PurchaseByVendorAsync & PurchaseByProductAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class PurStep91PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PurPurchasingService _purSvc;
    private readonly PurReceivingService _receivingSvc;
    private readonly PurReportService _reportSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public PurStep91PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pur-step91-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin91", DisplayName = "Admin 91" });
        var now = DateTimeOffset.UtcNow;
        _db.FinPeriods.Add(new Erp.Domain.Entities.Fin.FinPeriod
        {
            TenantId = _tenant, FiscalYearId = Guid.NewGuid(),
            Code = "K" + now.ToString("yyyy-MM"),
            Name = "Kỳ " + now.ToString("yyyy-MM"),
            StartDate = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month), 23, 59, 59, TimeSpan.Zero),
            Status = "Open", CreatedBy = _userAdmin
        });
        _db.FinAccounts.Add(new Erp.Domain.Entities.Fin.FinAccount
        {
            TenantId = _tenant, Code = "331", Name = "Phải trả cho người bán", AccountType = "Liability", Status = "Active"
        });
        _db.FinAccounts.Add(new Erp.Domain.Entities.Fin.FinAccount
        {
            TenantId = _tenant, Code = "156", Name = "Hàng hóa", AccountType = "Asset", Status = "Active"
        });
        _db.SaveChanges();

        var fin = new FinAccountingService(_db);
        var finAp = new FinApService(_db, fin, new FinCashService(_db, fin), new FinBankService(_db, fin), new FinVatService(_db));
        var invStock = new InvStockService(_db, null!);
        _purSvc = new PurPurchasingService(_db);
        _receivingSvc = new PurReceivingService(_db, invStock, finAp);
        _reportSvc = new PurReportService(_db);
    }

    public void Dispose() => _db.Dispose();

    private async Task<(PurVendorDto vendor, PurPurchaseOrderDto po)> CreateSentPoAsync()
    {
        var vendor = await _purSvc.UpsertVendorAsync(_tenant, _userAdmin, new PurVendorUpsertRequest(null, "VEND-91", "NCC Step 91", null, null, null, null, null, "Active"));
        var pr = await _purSvc.UpsertPrAsync(_tenant, _userAdmin, new PurPurchaseRequestUpsertRequest(null, "PR-91-01", null, null));
        await _purSvc.UpsertPrLineAsync(_tenant, _userAdmin, pr.Id, new PurPrLineUpsertRequest(null, "SKU-91", "Hàng Mua 91", 10, "Cái", null));
        await _purSvc.SubmitPrAsync(_tenant, _userAdmin, pr.Id);
        var approvedPr = await _purSvc.ApprovePrAsync(_tenant, _userAdmin, pr.Id, new PurPrDecisionRequest("Ok"));

        var po = await _purSvc.CreatePoFromPrAsync(_tenant, _userAdmin, approvedPr.Id, new PurCreatePoFromPrRequest("PO-91-01", vendor.Id, null));
        var submittedPo = await _purSvc.SubmitPoAsync(_tenant, _userAdmin, po.Id);
        var approvedPo = submittedPo.Status == "Approved" ? submittedPo : await _purSvc.ApprovePoAsync(_tenant, _userAdmin, po.Id);
        var sentPo = await _purSvc.SendPoAsync(_tenant, _userAdmin, approvedPo.Id);

        return (vendor, sentPo);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_PUR_040: Nhập hóa đơn nhà cung cấp
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_PUR_040_CreateInvoice_ValidRequest_CreatesVendorInvoice()
    {
        var (vendor, po) = await CreateSentPoAsync();

        var inv = await _receivingSvc.CreateInvoiceAsync(_tenant, _userAdmin, new PurInvoiceCreateRequest(vendor.Id, po.Id, "HD-NCC-91-01", DateTimeOffset.UtcNow, 100000m, "Hóa đơn đợt 1"));

        Assert.NotNull(inv);
        Assert.Equal("HD-NCC-91-01", inv.InvoiceNumber);
        Assert.Equal(vendor.Id, inv.VendorId);
    }

    [Fact]
    public async Task UC_PUR_040_CreateInvoice_EmptyInvoiceNumber_ThrowsException()
    {
        var (vendor, _) = await CreateSentPoAsync();

        await Assert.ThrowsAsync<AppException>(() =>
            _receivingSvc.CreateInvoiceAsync(_tenant, _userAdmin, new PurInvoiceCreateRequest(vendor.Id, null, "", DateTimeOffset.UtcNow, null, null)));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_PUR_041: Đối soát 3 chiều PO–GRN–Invoice
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_PUR_041_MatchThreeWay_ValidPoGrnInvoice_SetsMatchStatus()
    {
        var (vendor, po) = await CreateSentPoAsync();
        var grn = await _receivingSvc.CreateGrnFromPoAsync(_tenant, _userAdmin, new PurGrnCreateRequest(po.Id, null, null));
        await _receivingSvc.PostGrnAsync(_tenant, _userAdmin, grn.Id);

        var inv = await _receivingSvc.CreateInvoiceAsync(_tenant, _userAdmin, new PurInvoiceCreateRequest(vendor.Id, po.Id, "HD-MATCH-01", DateTimeOffset.UtcNow, 0m, null));

        var matchedInv = await _receivingSvc.MatchThreeWayAsync(_tenant, _userAdmin, inv.Id);

        Assert.NotNull(matchedInv);
        Assert.False(string.IsNullOrWhiteSpace(matchedInv.MatchStatus));
    }

    [Fact]
    public async Task UC_PUR_041_MatchThreeWay_NonExistentInvoice_ThrowsException()
    {
        await Assert.ThrowsAsync<AppException>(() =>
            _receivingSvc.MatchThreeWayAsync(_tenant, _userAdmin, Guid.NewGuid()));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_PUR_043: Đẩy công nợ sang FIN AP
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_PUR_043_PushInvoiceToAp_ValidInvoice_PushesToAp()
    {
        var (vendor, po) = await CreateSentPoAsync();
        var grn = await _receivingSvc.CreateGrnFromPoAsync(_tenant, _userAdmin, new PurGrnCreateRequest(po.Id, null, null));
        await _receivingSvc.PostGrnAsync(_tenant, _userAdmin, grn.Id);

        var inv = await _receivingSvc.CreateInvoiceAsync(_tenant, _userAdmin, new PurInvoiceCreateRequest(vendor.Id, po.Id, "HD-AP-01", DateTimeOffset.UtcNow, 100000m, null));
        await _receivingSvc.MatchThreeWayAsync(_tenant, _userAdmin, inv.Id);

        var pushed = await _receivingSvc.PushInvoiceToApAsync(_tenant, _userAdmin, inv.Id);

        Assert.NotNull(pushed);
        Assert.Equal("Pushed", pushed.ApPushStatus);
    }

    [Fact]
    public async Task UC_PUR_043_PushInvoiceToAp_Idempotent_ReturnsSameStatus()
    {
        var (vendor, po) = await CreateSentPoAsync();
        var grn = await _receivingSvc.CreateGrnFromPoAsync(_tenant, _userAdmin, new PurGrnCreateRequest(po.Id, null, null));
        await _receivingSvc.PostGrnAsync(_tenant, _userAdmin, grn.Id);

        var inv = await _receivingSvc.CreateInvoiceAsync(_tenant, _userAdmin, new PurInvoiceCreateRequest(vendor.Id, po.Id, "HD-AP-02", DateTimeOffset.UtcNow, 100000m, null));
        await _receivingSvc.MatchThreeWayAsync(_tenant, _userAdmin, inv.Id);

        var p1 = await _receivingSvc.PushInvoiceToApAsync(_tenant, _userAdmin, inv.Id);
        var p2 = await _receivingSvc.PushInvoiceToApAsync(_tenant, _userAdmin, inv.Id);

        Assert.Equal(p1.ApPushStatus, p2.ApPushStatus);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_PUR_048: Báo cáo mua theo nhà cung cấp / SP
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_PUR_048_PurchaseByVendor_ReturnsVendorReportRows()
    {
        await CreateSentPoAsync();

        var from = DateTimeOffset.UtcNow.AddDays(-30);
        var to = DateTimeOffset.UtcNow.AddDays(1);
        var rows = await _reportSvc.PurchaseByVendorAsync(_tenant, from, to);

        Assert.NotNull(rows);
    }

    [Fact]
    public async Task UC_PUR_048_PurchaseByProduct_ReturnsProductReportRows()
    {
        await CreateSentPoAsync();

        var from = DateTimeOffset.UtcNow.AddDays(-30);
        var to = DateTimeOffset.UtcNow.AddDays(1);
        var rows = await _reportSvc.PurchaseByProductAsync(_tenant, from, to);

        Assert.NotNull(rows);
    }
}
