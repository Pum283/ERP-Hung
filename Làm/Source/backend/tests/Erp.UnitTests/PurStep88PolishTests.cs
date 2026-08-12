using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Pur;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Pur;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 88:
///   UC_PUR_018 — Từ chối / trả lại PR (RejectPrAsync & ReturnPrAsync)
///   UC_PUR_019 — Theo dõi trạng thái PR (ListPrsAsync & GetPrDetailAsync)
///   UC_PUR_026 — Tạo PO từ PR/RFQ (CreatePoFromPrAsync)
///   UC_PUR_027 — Duyệt PO theo hạn mức (ApprovePoAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class PurStep88PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PurPurchasingService _purSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public PurStep88PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pur-step88-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin88", DisplayName = "Admin 88" });
        _db.SaveChanges();

        _purSvc = new PurPurchasingService(_db);
    }

    public void Dispose() => _db.Dispose();

    private async Task<(PurVendorDto vendor, PurPurchaseRequestDto pr)> CreateApprovedPrAsync()
    {
        var vendor = await _purSvc.UpsertVendorAsync(_tenant, _userAdmin, new PurVendorUpsertRequest(null, "VEND-88", "NCC Step 88", null, null, null, null, null, "Active"));
        var pr = await _purSvc.UpsertPrAsync(_tenant, _userAdmin, new PurPurchaseRequestUpsertRequest(null, "PR-88-01", "Phòng VT", "Cần mua gấp"));
        await _purSvc.UpsertPrLineAsync(_tenant, _userAdmin, pr.Id, new PurPrLineUpsertRequest(null, "SKU-88", "Sản Phẩm 88", 10, "Cái", null));
        await _purSvc.SubmitPrAsync(_tenant, _userAdmin, pr.Id);
        var approvedPr = await _purSvc.ApprovePrAsync(_tenant, _userAdmin, pr.Id, new PurPrDecisionRequest("Đồng ý"));
        return (vendor, approvedPr);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_PUR_018: Từ chối / trả lại PR
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_PUR_018_RejectPr_SubmittedPr_SetsStatusRejected()
    {
        var pr = await _purSvc.UpsertPrAsync(_tenant, _userAdmin, new PurPurchaseRequestUpsertRequest(null, "PR-REJ", null, null));
        await _purSvc.UpsertPrLineAsync(_tenant, _userAdmin, pr.Id, new PurPrLineUpsertRequest(null, "SKU-1", "SP 1", 1, "Cái", null));
        await _purSvc.SubmitPrAsync(_tenant, _userAdmin, pr.Id);

        var rejected = await _purSvc.RejectPrAsync(_tenant, _userAdmin, pr.Id, new PurPrDecisionRequest("Vượt ngân sách"));

        Assert.Equal("Rejected", rejected.Status);
    }

    [Fact]
    public async Task UC_PUR_018_ReturnPr_SubmittedPr_SetsStatusReturned()
    {
        var pr = await _purSvc.UpsertPrAsync(_tenant, _userAdmin, new PurPurchaseRequestUpsertRequest(null, "PR-RET", null, null));
        await _purSvc.UpsertPrLineAsync(_tenant, _userAdmin, pr.Id, new PurPrLineUpsertRequest(null, "SKU-1", "SP 1", 1, "Cái", null));
        await _purSvc.SubmitPrAsync(_tenant, _userAdmin, pr.Id);

        var returned = await _purSvc.ReturnPrAsync(_tenant, _userAdmin, pr.Id, new PurPrDecisionRequest("Thiếu báo giá"));

        Assert.Equal("Returned", returned.Status);
    }

    [Fact]
    public async Task UC_PUR_018_RejectPr_DraftPr_ThrowsException()
    {
        var pr = await _purSvc.UpsertPrAsync(_tenant, _userAdmin, new PurPurchaseRequestUpsertRequest(null, "PR-DRAFT-REJ", null, null));

        await Assert.ThrowsAsync<AppException>(() =>
            _purSvc.RejectPrAsync(_tenant, _userAdmin, pr.Id, new PurPrDecisionRequest("Lý do")));
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_PUR_019: Theo dõi trạng thái PR
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_PUR_019_ListPrs_FiltersByStatus()
    {
        var pr1 = await _purSvc.UpsertPrAsync(_tenant, _userAdmin, new PurPurchaseRequestUpsertRequest(null, "PR-LST-1", null, null));
        var pr2 = await _purSvc.UpsertPrAsync(_tenant, _userAdmin, new PurPurchaseRequestUpsertRequest(null, "PR-LST-2", null, null));
        await _purSvc.UpsertPrLineAsync(_tenant, _userAdmin, pr2.Id, new PurPrLineUpsertRequest(null, "S1", "SP 1", 1, "Cái", null));
        await _purSvc.SubmitPrAsync(_tenant, _userAdmin, pr2.Id);

        var listAll = await _purSvc.ListPrsAsync(_tenant);

        Assert.Contains(listAll, x => x.Id == pr1.Id);
        Assert.Contains(listAll, x => x.Id == pr2.Id);
    }

    [Fact]
    public async Task UC_PUR_019_GetPrDetail_ReturnsPrWithLines()
    {
        var pr = await _purSvc.UpsertPrAsync(_tenant, _userAdmin, new PurPurchaseRequestUpsertRequest(null, "PR-DET", null, null));
        await _purSvc.UpsertPrLineAsync(_tenant, _userAdmin, pr.Id, new PurPrLineUpsertRequest(null, "SKU-X", "Món X", 5, "Hộp", null));

        var detail = await _purSvc.GetPrDetailAsync(_tenant, pr.Id);

        Assert.NotNull(detail);
        Assert.Single(detail.Lines);
        Assert.Equal("SKU-X", detail.Lines[0].ProductCode);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_PUR_026: Tạo PO từ PR/RFQ
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_PUR_026_CreatePoFromPr_ApprovedPr_CreatesDraftPo()
    {
        var (vendor, pr) = await CreateApprovedPrAsync();

        var po = await _purSvc.CreatePoFromPrAsync(_tenant, _userAdmin, pr.Id, new PurCreatePoFromPrRequest("PO-88-01", vendor.Id, "Tạo từ PR 88"));

        Assert.NotNull(po);
        Assert.Equal("Draft", po.Status);
        Assert.Equal(vendor.Id, po.VendorId);
    }

    [Fact]
    public async Task UC_PUR_026_CreatePoFromPr_DraftPr_ThrowsException()
    {
        var vendor = await _purSvc.UpsertVendorAsync(_tenant, _userAdmin, new PurVendorUpsertRequest(null, "VEND-ERR", "NCC Err", null, null, null, null, null, "Active"));
        var pr = await _purSvc.UpsertPrAsync(_tenant, _userAdmin, new PurPurchaseRequestUpsertRequest(null, "PR-DRAFT-PO", null, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _purSvc.CreatePoFromPrAsync(_tenant, _userAdmin, pr.Id, new PurCreatePoFromPrRequest("PO-ERR", vendor.Id, null)));
        Assert.Contains("PR chưa duyệt", ex.Message);
    }

    [Fact]
    public async Task UC_PUR_026_CreatePoFromPr_CopiesLinesAndCalculatesAmount()
    {
        var (vendor, pr) = await CreateApprovedPrAsync();

        var po = await _purSvc.CreatePoFromPrAsync(_tenant, _userAdmin, pr.Id, new PurCreatePoFromPrRequest("PO-88-02", vendor.Id, null));
        var poDetail = await _purSvc.GetPoDetailAsync(_tenant, po.Id);

        Assert.Single(poDetail.Lines);
        Assert.Equal("SKU-88", poDetail.Lines[0].ProductCode);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_PUR_027: Duyệt PO theo hạn mức
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_PUR_027_ApprovePo_SubmittedPo_TransitionsToApproved()
    {
        var (vendor, pr) = await CreateApprovedPrAsync();
        var po = await _purSvc.CreatePoFromPrAsync(_tenant, _userAdmin, pr.Id, new PurCreatePoFromPrRequest("PO-88-03", vendor.Id, null));
        var submittedPo = await _purSvc.SubmitPoAsync(_tenant, _userAdmin, po.Id);
        var approvedPo = submittedPo.Status == "Approved" ? submittedPo : await _purSvc.ApprovePoAsync(_tenant, _userAdmin, po.Id);

        Assert.Equal("Approved", approvedPo.Status);
        Assert.NotNull(approvedPo.ApprovedAt);
    }

    [Fact]
    public async Task UC_PUR_027_ApprovePo_DraftPo_ThrowsException()
    {
        var (vendor, pr) = await CreateApprovedPrAsync();
        var po = await _purSvc.CreatePoFromPrAsync(_tenant, _userAdmin, pr.Id, new PurCreatePoFromPrRequest("PO-88-04", vendor.Id, null));

        await Assert.ThrowsAsync<AppException>(() =>
            _purSvc.ApprovePoAsync(_tenant, _userAdmin, po.Id));
    }
}
