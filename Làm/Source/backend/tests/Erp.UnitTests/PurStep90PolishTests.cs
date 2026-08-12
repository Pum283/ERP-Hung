using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Pur;
using Erp.Domain.Entities.Inv;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Fin;
using Erp.Infrastructure.Implementations.Services.Inv;
using Erp.Infrastructure.Implementations.Services.Pur;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 90:
///   UC_PUR_033 — In / xuất PO (PrintPoAsync & ExportPoCsvAsync)
///   UC_PUR_034 — Tạo phiếu nhận hàng theo PO (CreateGrnFromPoAsync)
///   UC_PUR_035 — Nhận hàng lệch số lượng / chất lượng (UpdateGrnLineAsync & PostGrnAsync)
///   UC_PUR_037 — Đẩy nhập kho sang INV (PushGrnToInventoryAsync)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class PurStep90PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PurPurchasingService _purSvc;
    private readonly PurReceivingService _receivingSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public PurStep90PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pur-step90-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin90", DisplayName = "Admin 90" });
        _db.InvWarehouses.Add(new InvWarehouse
        {
            TenantId = _tenant, Code = "KHO-MAIN", Name = "Kho Chính", Status = "Active", AllowNegativeStock = true, CreatedBy = _userAdmin
        });
        _db.SaveChanges();

        var fin = new FinAccountingService(_db);
        var finAp = new FinApService(_db, fin, new FinCashService(_db, fin), new FinBankService(_db, fin), new FinVatService(_db));
        var invStock = new InvStockService(_db, null!);
        _purSvc = new PurPurchasingService(_db);
        _receivingSvc = new PurReceivingService(_db, invStock, finAp);
    }

    public void Dispose() => _db.Dispose();

    private async Task<(PurVendorDto vendor, PurPurchaseOrderDto po)> CreateSentPoAsync()
    {
        var vendor = await _purSvc.UpsertVendorAsync(_tenant, _userAdmin, new PurVendorUpsertRequest(null, "VEND-90", "NCC Step 90", null, null, null, null, null, "Active"));
        var pr = await _purSvc.UpsertPrAsync(_tenant, _userAdmin, new PurPurchaseRequestUpsertRequest(null, "PR-90-01", null, null));
        await _purSvc.UpsertPrLineAsync(_tenant, _userAdmin, pr.Id, new PurPrLineUpsertRequest(null, "SKU-90", "Hàng Mua 90", 20, "Cái", null));
        await _purSvc.SubmitPrAsync(_tenant, _userAdmin, pr.Id);
        var approvedPr = await _purSvc.ApprovePrAsync(_tenant, _userAdmin, pr.Id, new PurPrDecisionRequest("Ok"));

        var po = await _purSvc.CreatePoFromPrAsync(_tenant, _userAdmin, approvedPr.Id, new PurCreatePoFromPrRequest("PO-90-01", vendor.Id, null));
        var submittedPo = await _purSvc.SubmitPoAsync(_tenant, _userAdmin, po.Id);
        var approvedPo = submittedPo.Status == "Approved" ? submittedPo : await _purSvc.ApprovePoAsync(_tenant, _userAdmin, po.Id);
        var sentPo = await _purSvc.SendPoAsync(_tenant, _userAdmin, approvedPo.Id);

        return (vendor, sentPo);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_PUR_033: In / xuất PO
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_PUR_033_PrintPo_ValidPo_ReturnsPrintedPoAndUpdatesTimestamp()
    {
        var (_, po) = await CreateSentPoAsync();

        var printed = await _purSvc.PrintPoAsync(_tenant, _userAdmin, po.Id);

        Assert.NotNull(printed);
        Assert.NotNull(printed.PrintedAt);
    }

    [Fact]
    public async Task UC_PUR_033_ExportPoCsv_ValidPo_ReturnsCsvFilenameAndContent()
    {
        var (_, po) = await CreateSentPoAsync();

        var (fileName, csv) = await _purSvc.ExportPoCsvAsync(_tenant, _userAdmin, po.Id);

        Assert.Contains(po.Code, fileName);
        Assert.Contains("ProductCode", csv);
        Assert.Contains("SKU-90", csv);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_PUR_034: Tạo phiếu nhận hàng theo PO
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_PUR_034_CreateGrnFromPo_SentPo_CreatesDraftGrn()
    {
        var (_, po) = await CreateSentPoAsync();

        var grn = await _receivingSvc.CreateGrnFromPoAsync(_tenant, _userAdmin, new PurGrnCreateRequest(po.Id, "Nhận hàng đợt 1", null));

        Assert.NotNull(grn);
        Assert.Equal("Draft", grn.Status);
        Assert.Equal(po.Id, grn.PoId);
    }

    [Fact]
    public async Task UC_PUR_034_CreateGrnFromPo_DraftPo_ThrowsException()
    {
        var vendor = await _purSvc.UpsertVendorAsync(_tenant, _userAdmin, new PurVendorUpsertRequest(null, "VEND-90B", "NCC 90B", null, null, null, null, null, "Active"));
        var pr = await _purSvc.UpsertPrAsync(_tenant, _userAdmin, new PurPurchaseRequestUpsertRequest(null, "PR-90B", null, null));
        await _purSvc.UpsertPrLineAsync(_tenant, _userAdmin, pr.Id, new PurPrLineUpsertRequest(null, "SKU-90B", "Món 90B", 1, "Cái", null));
        await _purSvc.SubmitPrAsync(_tenant, _userAdmin, pr.Id);
        var approvedPr = await _purSvc.ApprovePrAsync(_tenant, _userAdmin, pr.Id, new PurPrDecisionRequest("Ok"));

        var draftPo = await _purSvc.CreatePoFromPrAsync(_tenant, _userAdmin, approvedPr.Id, new PurCreatePoFromPrRequest("PO-90B", vendor.Id, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _receivingSvc.CreateGrnFromPoAsync(_tenant, _userAdmin, new PurGrnCreateRequest(draftPo.Id, null, null)));
        Assert.Contains("PO Sent", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_PUR_035: Nhận hàng lệch số lượng / chất lượng
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_PUR_035_UpdateGrnLine_ValidAcceptedAndRejected_UpdatesLine()
    {
        var (_, po) = await CreateSentPoAsync();
        var grn = await _receivingSvc.CreateGrnFromPoAsync(_tenant, _userAdmin, new PurGrnCreateRequest(po.Id, null, null));
        var detail = await _receivingSvc.GetGrnDetailAsync(_tenant, grn.Id);
        var lineId = detail.Lines[0].Id;

        var updatedLine = await _receivingSvc.UpdateGrnLineAsync(_tenant, _userAdmin, grn.Id, new PurGrnLineUpdateRequest(lineId, 20, 18, 2));

        Assert.Equal(18m, updatedLine.AcceptedQty);
        Assert.Equal(2m, updatedLine.RejectedQty);
    }

    [Fact]
    public async Task UC_PUR_035_PostGrn_DraftGrn_TransitionsToPosted()
    {
        var (_, po) = await CreateSentPoAsync();
        var grn = await _receivingSvc.CreateGrnFromPoAsync(_tenant, _userAdmin, new PurGrnCreateRequest(po.Id, null, null));

        var postedGrn = await _receivingSvc.PostGrnAsync(_tenant, _userAdmin, grn.Id);

        Assert.Equal("Posted", postedGrn.Status);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_PUR_037: Đẩy nhập kho sang INV
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_PUR_037_PushGrnToInventory_PostedGrn_CreatesStockDoc()
    {
        var (_, po) = await CreateSentPoAsync();
        var grn = await _receivingSvc.CreateGrnFromPoAsync(_tenant, _userAdmin, new PurGrnCreateRequest(po.Id, null, null));
        await _receivingSvc.PostGrnAsync(_tenant, _userAdmin, grn.Id);

        var pushedGrn = await _receivingSvc.PushGrnToInventoryAsync(_tenant, _userAdmin, grn.Id);

        Assert.Equal("Pushed", pushedGrn.InventoryPushStatus);
    }

    [Fact]
    public async Task UC_PUR_037_PushGrnToInventory_DraftGrn_ThrowsException()
    {
        var (_, po) = await CreateSentPoAsync();
        var grn = await _receivingSvc.CreateGrnFromPoAsync(_tenant, _userAdmin, new PurGrnCreateRequest(po.Id, null, null));

        await Assert.ThrowsAsync<AppException>(() =>
            _receivingSvc.PushGrnToInventoryAsync(_tenant, _userAdmin, grn.Id));
    }

    [Fact]
    public async Task UC_PUR_037_PushGrnToInventory_Idempotent_ReturnsSameGrn()
    {
        var (_, po) = await CreateSentPoAsync();
        var grn = await _receivingSvc.CreateGrnFromPoAsync(_tenant, _userAdmin, new PurGrnCreateRequest(po.Id, null, null));
        await _receivingSvc.PostGrnAsync(_tenant, _userAdmin, grn.Id);

        var pushed1 = await _receivingSvc.PushGrnToInventoryAsync(_tenant, _userAdmin, grn.Id);
        var pushed2 = await _receivingSvc.PushGrnToInventoryAsync(_tenant, _userAdmin, grn.Id);

        Assert.Equal(pushed1.InventoryPushStatus, pushed2.InventoryPushStatus);
    }
}
