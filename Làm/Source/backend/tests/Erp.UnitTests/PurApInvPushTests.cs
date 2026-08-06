using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Fin;
using Erp.Application.Interfaces.Services.Fin;
using Erp.Domain.Entities.Inv;
using Erp.Domain.Entities.Pur;
using Erp.Infrastructure.Implementations.Services.Fin;
using Erp.Infrastructure.Implementations.Services.Inv;
using Erp.Infrastructure.Implementations.Services.Pur;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.UnitTests;

/// <summary>UC_PUR_033/037/043 — xuất PO CSV thật · đẩy GRN→INV · đẩy HĐ→FIN AP thật.</summary>
public sealed class PurApInvPushTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PurReceivingService _svc;
    private readonly PurPurchasingService _po;
    private readonly Guid _tenant = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
    private readonly Guid _user = Guid.Parse("11111111-2222-3333-4444-555555555555");

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

    public PurApInvPushTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pur-ap-inv-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        var fin = new FinAccountingService(_db);
        var finAp = new FinApService(_db, fin, new FinCashService(_db, fin), new FinBankService(_db, fin), new FinVatService(_db));
        var inv = new InvStockService(_db, new NoopFinRevenue());
        _svc = new PurReceivingService(_db, inv, finAp);
        _po = new PurPurchasingService(_db);
    }

    public void Dispose() => _db.Dispose();

    private PurVendor AddVendor(string status = "Active")
    {
        var v = new PurVendor { TenantId = _tenant, Code = "NCC1", Name = "NCC Một", Status = status, CreatedBy = _user };
        _db.PurVendors.Add(v);
        return v;
    }

    private PurPurchaseOrder AddPo(Guid vendorId, string status = "Sent")
    {
        var po = new PurPurchaseOrder
        {
            TenantId = _tenant, Code = "PO-T1", VendorId = vendorId, Status = status,
            TotalAmount = 500_000, CreatedByUserId = _user, CreatedBy = _user,
        };
        _db.PurPurchaseOrders.Add(po);
        _db.PurPoLines.Add(new PurPoLine
        {
            TenantId = _tenant, PoId = po.Id, ProductCode = "SP-A", ProductName = "SP A",
            Qty = 10, UnitPrice = 50_000, Unit = "cai", CreatedBy = _user,
        });
        return po;
    }

    private PurVendorInvoice AddMatchedInvoice(Guid vendorId, Guid? poId, decimal subTotal = 500_000, decimal tax = 50_000)
    {
        var inv = new PurVendorInvoice
        {
            TenantId = _tenant, Code = "VIN-T1", VendorId = vendorId, PoId = poId,
            InvoiceNumber = "HD-9", InvoiceDate = DateTimeOffset.UtcNow,
            Status = "Matched", MatchStatus = "Matched", ApPushStatus = "None",
            SubTotal = subTotal, TaxAmount = tax, TotalAmount = subTotal + tax,
            CreatedBy = _user,
        };
        _db.PurVendorInvoices.Add(inv);
        return inv;
    }

    // ── UC_PUR_043 đẩy FIN AP thật ──

    [Fact]
    public async Task PushAp_CreatesRealOpenFinApInvoice()
    {
        var vendor = AddVendor();
        var po = AddPo(vendor.Id);
        var inv = AddMatchedInvoice(vendor.Id, po.Id);
        await _db.SaveChangesAsync();

        var result = await _svc.PushInvoiceToApAsync(_tenant, _user, inv.Id);

        Assert.Equal("Pushed", result.ApPushStatus);
        Assert.Equal("Posted", result.Status);
        var ap = await _db.FinApInvoices.SingleAsync(x => x.PurVendorInvoiceId == inv.Id);
        Assert.Equal("Open", ap.Status);
        Assert.Equal(550_000, ap.TotalAmount);
        Assert.Equal(vendor.Id, ap.VendorId);
        Assert.Equal("HD-9", ap.VendorInvoiceNo);
    }

    [Fact]
    public async Task PushAp_IsIdempotent()
    {
        var vendor = AddVendor();
        var inv = AddMatchedInvoice(vendor.Id, null);
        await _db.SaveChangesAsync();

        await _svc.PushInvoiceToApAsync(_tenant, _user, inv.Id);
        await _svc.PushInvoiceToApAsync(_tenant, _user, inv.Id);

        Assert.Equal(1, await _db.FinApInvoices.CountAsync(x => x.PurVendorInvoiceId == inv.Id));
    }

    [Fact]
    public async Task PushAp_RejectsUnmatchedInvoice()
    {
        var vendor = AddVendor();
        var inv = AddMatchedInvoice(vendor.Id, null);
        inv.MatchStatus = "Variance";
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<AppException>(() => _svc.PushInvoiceToApAsync(_tenant, _user, inv.Id));
        Assert.Equal(0, await _db.FinApInvoices.CountAsync());
    }

    [Fact]
    public async Task PushAp_RejectsZeroTotal()
    {
        var vendor = AddVendor();
        var inv = AddMatchedInvoice(vendor.Id, null, subTotal: 0, tax: 0);
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<AppException>(() => _svc.PushInvoiceToApAsync(_tenant, _user, inv.Id));
    }

    [Fact]
    public async Task PushAp_MarksFailedWhenVendorInactive()
    {
        var vendor = AddVendor(status: "Inactive");
        var inv = AddMatchedInvoice(vendor.Id, null);
        await _db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<AppException>(() => _svc.PushInvoiceToApAsync(_tenant, _user, inv.Id));
        Assert.Contains("Đẩy AP thất bại", ex.Message);
        var reloaded = await _db.PurVendorInvoices.SingleAsync(x => x.Id == inv.Id);
        Assert.Equal("Failed", reloaded.ApPushStatus);
    }

    // ── UC_PUR_033 xuất PO CSV thật ──

    [Fact]
    public async Task ExportPoCsv_ContainsHeaderLinesAndTotal()
    {
        var vendor = AddVendor();
        var po = AddPo(vendor.Id, status: "Sent");
        await _db.SaveChangesAsync();

        var (fileName, csv) = await _po.ExportPoCsvAsync(_tenant, _user, po.Id);

        Assert.Equal("PO-T1-v1.csv", fileName);
        Assert.Contains("PO,PO-T1", csv);
        Assert.Contains("NCC1", csv);
        Assert.Contains("SP-A", csv);
        Assert.Contains("TOTAL,500000", csv);

        var reloaded = await _db.PurPurchaseOrders.SingleAsync(x => x.Id == po.Id);
        Assert.NotNull(reloaded.PrintedAt);
    }

    [Fact]
    public async Task ExportPoCsv_RejectsDraftAndCancelled()
    {
        var vendor = AddVendor();
        var draft = AddPo(vendor.Id, status: "Draft");
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<AppException>(() => _po.ExportPoCsvAsync(_tenant, _user, draft.Id));
    }

    // ── UC_PUR_037 đẩy GRN → INV ──

    private async Task<Guid> CreateDraftGrnAsync(Guid poId)
    {
        var grn = await _svc.CreateGrnFromPoAsync(_tenant, _user, new Erp.Application.DTOs.Pur.PurGrnCreateRequest(poId, null, null));
        return grn.Id;
    }

    [Fact]
    public async Task PostGrn_PushesRealInvReceiptWhenWarehouseReady()
    {
        _db.InvWarehouses.Add(new InvWarehouse { TenantId = _tenant, Code = "WH1", Name = "Kho 1", Status = "Active", CreatedBy = _user });
        _db.InvUnitsOfMeasure.Add(new InvUnitOfMeasure { TenantId = _tenant, Code = "CAI", Name = "Cái", CreatedBy = _user });
        var vendor = AddVendor();
        var po = AddPo(vendor.Id);
        await _db.SaveChangesAsync();

        var grnId = await CreateDraftGrnAsync(po.Id);
        var posted = await _svc.PostGrnAsync(_tenant, _user, grnId);

        Assert.Equal("Pushed", posted.InventoryPushStatus);
        var doc = await _db.InvStockDocs.SingleAsync(x => x.RefModule == "PUR" && x.RefId == grnId);
        Assert.Equal("Receipt", doc.DocType);
        Assert.Equal("Purchase", doc.SourceType);
        Assert.Equal("Posted", doc.Status);
        var line = await _db.InvStockDocLines.SingleAsync(x => x.DocId == doc.Id);
        Assert.Equal(10, line.Qty);
        Assert.Equal(50_000, line.UnitCost);
    }

    [Fact]
    public async Task PostGrn_RecordsFailureReasonWhenNoWarehouse()
    {
        var vendor = AddVendor();
        var po = AddPo(vendor.Id);
        await _db.SaveChangesAsync();

        var grnId = await CreateDraftGrnAsync(po.Id);
        var posted = await _svc.PostGrnAsync(_tenant, _user, grnId);

        Assert.Equal("Failed", posted.InventoryPushStatus);
        var reloaded = await _db.PurGoodsReceipts.SingleAsync(x => x.Id == grnId);
        Assert.Contains("INV lỗi:", reloaded.Note);
    }

    [Fact]
    public async Task PushGrnInventory_RetrySucceedsAfterFixingWarehouse()
    {
        var vendor = AddVendor();
        var po = AddPo(vendor.Id);
        await _db.SaveChangesAsync();
        var grnId = await CreateDraftGrnAsync(po.Id);
        var posted = await _svc.PostGrnAsync(_tenant, _user, grnId);
        Assert.Equal("Failed", posted.InventoryPushStatus);

        _db.InvWarehouses.Add(new InvWarehouse { TenantId = _tenant, Code = "WH1", Name = "Kho 1", Status = "Active", CreatedBy = _user });
        _db.InvUnitsOfMeasure.Add(new InvUnitOfMeasure { TenantId = _tenant, Code = "CAI", Name = "Cái", CreatedBy = _user });
        await _db.SaveChangesAsync();

        var retried = await _svc.PushGrnToInventoryAsync(_tenant, _user, grnId);
        Assert.Equal("Pushed", retried.InventoryPushStatus);
        Assert.Equal(1, await _db.InvStockDocs.CountAsync(x => x.RefModule == "PUR" && x.RefId == grnId));
    }
}
