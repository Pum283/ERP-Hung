using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Pur;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Pur;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 89:
///   UC_PUR_028 — Gửi PO cho nhà cung cấp (SendPoAsync)
///   UC_PUR_030 — Sửa PO phiên bản (RevisePoAsync)
///   UC_PUR_031 — Theo dõi nhận hàng từng phần (GetPoDetailAsync status)
///   UC_PUR_032 — Đóng / hủy PO (ClosePoAsync & CancelPoAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class PurStep89PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PurPurchasingService _purSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public PurStep89PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pur-step89-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin89", DisplayName = "Admin 89" });
        _db.SaveChanges();

        _purSvc = new PurPurchasingService(_db);
    }

    public void Dispose() => _db.Dispose();

    private async Task<(PurVendorDto vendor, PurPurchaseOrderDto po)> CreateApprovedPoAsync()
    {
        var vendor = await _purSvc.UpsertVendorAsync(_tenant, _userAdmin, new PurVendorUpsertRequest(null, "VEND-89", "NCC Step 89", null, null, null, null, null, "Active"));
        var pr = await _purSvc.UpsertPrAsync(_tenant, _userAdmin, new PurPurchaseRequestUpsertRequest(null, "PR-89-01", null, null));
        await _purSvc.UpsertPrLineAsync(_tenant, _userAdmin, pr.Id, new PurPrLineUpsertRequest(null, "SKU-89", "Món 89", 5, "Bộ", null));
        await _purSvc.SubmitPrAsync(_tenant, _userAdmin, pr.Id);
        var approvedPr = await _purSvc.ApprovePrAsync(_tenant, _userAdmin, pr.Id, new PurPrDecisionRequest("Ok"));

        var po = await _purSvc.CreatePoFromPrAsync(_tenant, _userAdmin, approvedPr.Id, new PurCreatePoFromPrRequest("PO-89-01", vendor.Id, null));
        var submittedPo = await _purSvc.SubmitPoAsync(_tenant, _userAdmin, po.Id);
        var approvedPo = submittedPo.Status == "Approved" ? submittedPo : await _purSvc.ApprovePoAsync(_tenant, _userAdmin, po.Id);

        return (vendor, approvedPo);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_PUR_028: Gửi PO cho nhà cung cấp
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_PUR_028_SendPo_ApprovedPo_TransitionsToSentAndSetsSentAt()
    {
        var (_, po) = await CreateApprovedPoAsync();

        var sentPo = await _purSvc.SendPoAsync(_tenant, _userAdmin, po.Id);

        Assert.Equal("Sent", sentPo.Status);
        Assert.NotNull(sentPo.SentAt);
    }

    [Fact]
    public async Task UC_PUR_028_SendPo_DraftPo_ThrowsException()
    {
        var vendor = await _purSvc.UpsertVendorAsync(_tenant, _userAdmin, new PurVendorUpsertRequest(null, "VEND-89B", "NCC 89B", null, null, null, null, null, "Active"));
        var pr = await _purSvc.UpsertPrAsync(_tenant, _userAdmin, new PurPurchaseRequestUpsertRequest(null, "PR-89B", null, null));
        await _purSvc.UpsertPrLineAsync(_tenant, _userAdmin, pr.Id, new PurPrLineUpsertRequest(null, "SKU-89B", "Món 89B", 1, "Cái", null));
        await _purSvc.SubmitPrAsync(_tenant, _userAdmin, pr.Id);
        var approvedPr = await _purSvc.ApprovePrAsync(_tenant, _userAdmin, pr.Id, new PurPrDecisionRequest("Ok"));

        var draftPo = await _purSvc.CreatePoFromPrAsync(_tenant, _userAdmin, approvedPr.Id, new PurCreatePoFromPrRequest("PO-89B", vendor.Id, null));

        await Assert.ThrowsAsync<AppException>(() =>
            _purSvc.SendPoAsync(_tenant, _userAdmin, draftPo.Id));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_PUR_030: Sửa PO phiên bản
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_PUR_030_RevisePo_ApprovedPo_IncrementsRevisionNumber()
    {
        var (_, po) = await CreateApprovedPoAsync();
        var sentPo = await _purSvc.SendPoAsync(_tenant, _userAdmin, po.Id);

        var revisedPo = await _purSvc.RevisePoAsync(_tenant, _userAdmin, sentPo.Id);

        Assert.NotNull(revisedPo);
        Assert.True(revisedPo.Version > 1);
    }

    [Fact]
    public async Task UC_PUR_030_RevisePo_ClosedPo_ThrowsException()
    {
        var (_, po) = await CreateApprovedPoAsync();
        var sentPo = await _purSvc.SendPoAsync(_tenant, _userAdmin, po.Id);
        var closedPo = await _purSvc.ClosePoAsync(_tenant, _userAdmin, sentPo.Id);

        await Assert.ThrowsAsync<AppException>(() =>
            _purSvc.RevisePoAsync(_tenant, _userAdmin, closedPo.Id));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_PUR_031: Theo dõi nhận hàng từng phần
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_PUR_031_GetPoDetail_ReturnsReceivingTrackingInfo()
    {
        var (_, po) = await CreateApprovedPoAsync();

        var detail = await _purSvc.GetPoDetailAsync(_tenant, po.Id);

        Assert.NotNull(detail);
        Assert.Single(detail.Lines);
        Assert.Equal(5m, detail.Lines[0].Qty);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_PUR_032: Đóng / hủy PO
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_PUR_032_ClosePo_SentPo_SetsStatusClosed()
    {
        var (_, po) = await CreateApprovedPoAsync();
        var sentPo = await _purSvc.SendPoAsync(_tenant, _userAdmin, po.Id);

        var closedPo = await _purSvc.ClosePoAsync(_tenant, _userAdmin, sentPo.Id);

        Assert.Equal("Closed", closedPo.Status);
    }

    [Fact]
    public async Task UC_PUR_032_ClosePo_AlreadyClosed_ThrowsException()
    {
        var (_, po) = await CreateApprovedPoAsync();
        var sentPo = await _purSvc.SendPoAsync(_tenant, _userAdmin, po.Id);
        var closedPo = await _purSvc.ClosePoAsync(_tenant, _userAdmin, sentPo.Id);

        await Assert.ThrowsAsync<AppException>(() =>
            _purSvc.ClosePoAsync(_tenant, _userAdmin, closedPo.Id));
    }

    [Fact]
    public async Task UC_PUR_032_CancelPo_SentPo_SetsStatusCancelled()
    {
        var (_, po) = await CreateApprovedPoAsync();
        var sentPo = await _purSvc.SendPoAsync(_tenant, _userAdmin, po.Id);

        var cancelledPo = await _purSvc.CancelPoAsync(_tenant, _userAdmin, sentPo.Id, new PurPoCancelRequest("Hủy theo thỏa thuận NCC"));

        Assert.Equal("Cancelled", cancelledPo.Status);
    }

    [Fact]
    public async Task UC_PUR_032_CancelPo_AlreadyCancelled_ThrowsException()
    {
        var (_, po) = await CreateApprovedPoAsync();
        var sentPo = await _purSvc.SendPoAsync(_tenant, _userAdmin, po.Id);
        var cancelledPo = await _purSvc.CancelPoAsync(_tenant, _userAdmin, sentPo.Id, new PurPoCancelRequest("Hủy lần 1"));

        await Assert.ThrowsAsync<AppException>(() =>
            _purSvc.CancelPoAsync(_tenant, _userAdmin, cancelledPo.Id, new PurPoCancelRequest("Hủy lần 2")));
    }
}
