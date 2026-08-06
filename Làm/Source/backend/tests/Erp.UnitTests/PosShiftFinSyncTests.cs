using Erp.Application.DTOs.Fin;
using Erp.Application.DTOs.Pos;
using Erp.Application.Interfaces.Services.Fin;
using Erp.Domain.Entities.Fin;
using Erp.Domain.Entities.Pos;
using Erp.Infrastructure.Implementations.Services.Inv;
using Erp.Infrastructure.Implementations.Services.Pos;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.UnitTests;

sealed class CountingFinRevenueForShift : IFinRevenueService
{
    public int PosCalls { get; private set; }
    public HashSet<Guid> CalledSaleIds { get; } = new();

    private static FinRevenueDocumentDto Doc(Guid? saleId = null) => new(
        Guid.NewGuid(), "REV-X", "PosRevenue", "POS", saleId, "SALE",
        DateTimeOffset.UtcNow, 0, 0, 0, 0,
        null, null, null, null, null, null, null, null, "Draft", null, null);

    public Task<IReadOnlyList<FinRevenueDocumentDto>> ListAsync(
        Guid tenantId, string? kind = null, Guid? periodId = null, string? status = null,
        CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<FinRevenueDocumentDto>>([]);

    public Task<FinRevenueSummaryDto> GetSummaryAsync(
        Guid tenantId, Guid? periodId = null, CancellationToken ct = default)
        => Task.FromResult(new FinRevenueSummaryDto(null, null, 0, 0, 0, 0, 0, 0, 0, 0, 0));

    public Task<FinRevenueDocumentDto> RecognizeFromPosAsync(
        Guid tenantId, Guid userId, Guid saleId, FinRevenueRecognizeRequest? req = null,
        CancellationToken ct = default)
    {
        PosCalls++;
        CalledSaleIds.Add(saleId);
        return Task.FromResult(Doc(saleId));
    }

    public Task<FinRevenueDocumentDto> RecognizeFromSalesOrderAsync(
        Guid tenantId, Guid userId, Guid orderId, FinRevenueRecognizeRequest? req = null,
        CancellationToken ct = default)
        => Task.FromResult(Doc());

    public Task<FinRevenueDocumentDto> RecognizeFromArInvoiceAsync(
        Guid tenantId, Guid userId, Guid arInvoiceId, FinRevenueRecognizeRequest? req = null,
        CancellationToken ct = default)
        => Task.FromResult(Doc());

    public Task<FinRevenueDocumentDto> RecognizeCogsAsync(
        Guid tenantId, Guid userId, Guid invStockDocId, FinRevenueRecognizeRequest? req = null,
        CancellationToken ct = default)
        => Task.FromResult(Doc());

    public Task<FinRevenueDocumentDto> VoidAsync(
        Guid tenantId, Guid userId, Guid id, string? note = null, CancellationToken ct = default)
        => Task.FromResult(Doc());
}

/// <summary>UC_POS_059 — đóng ca sync DT FIN — EF InMemory.</summary>
public sealed class PosShiftFinSyncTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CountingFinRevenueForShift _fin;
    private readonly PosSalesService _sales;
    private readonly Guid _tenant = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private readonly Guid _user = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    public PosShiftFinSyncTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pos-fin-shift-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _fin = new CountingFinRevenueForShift();
        var stock = new InvStockService(_db, _fin);
        _sales = new PosSalesService(_db, _fin, stock);
    }

    public void Dispose() => _db.Dispose();

    private async Task<(PosStore store, PosShift shift)> SeedStoreShiftAsync()
    {
        var store = new PosStore
        {
            TenantId = _tenant, Code = "ST-F", Name = "Store Fin", Status = "Active",
        };
        _db.PosStores.Add(store);
        await _db.SaveChangesAsync();
        var shift = await _sales.OpenShiftAsync(_tenant, _user, new PosShiftOpenRequest(
            store.Id, null, 100_000, null));
        return (store, await _db.PosShifts.FirstAsync(x => x.Id == shift.Id));
    }

    private async Task<PosSale> SeedPaidSaleAsync(Guid shiftId, Guid storeId, string code)
    {
        var sale = new PosSale
        {
            TenantId = _tenant, CreatedBy = _user, Code = code,
            ShiftId = shiftId, StoreId = storeId, Status = "Paid",
            SubTotal = 100_000, TaxAmount = 0, DiscountAmount = 0, TotalAmount = 100_000,
            PaidAmount = 100_000, PaidAt = DateTimeOffset.UtcNow,
        };
        _db.PosSales.Add(sale);
        await _db.SaveChangesAsync();
        return sale;
    }

    [Fact]
    public void FormatFinSyncTag_Shape()
    {
        var tag = PosSalesService.FormatFinSyncTag(new PosShiftFinSyncResult(
            Guid.NewGuid(), 3, 2, 1, 0, "msg"));
        Assert.Equal("FIN:2+1/3 fail=0", tag);
    }

    [Fact]
    public async Task SyncShift_RecognizesMissingPaidSales()
    {
        var (_, shift) = await SeedStoreShiftAsync();
        var s1 = await SeedPaidSaleAsync(shift.Id, shift.StoreId, "S1");
        var s2 = await SeedPaidSaleAsync(shift.Id, shift.StoreId, "S2");

        var r = await _sales.SyncShiftRevenueToFinAsync(_tenant, _user, shift.Id);
        Assert.Equal(2, r.PaidSaleCount);
        Assert.Equal(2, r.SyncedCount);
        Assert.Equal(0, r.AlreadyHadCount);
        Assert.Equal(2, _fin.PosCalls);
        Assert.Contains(s1.Id, _fin.CalledSaleIds);
        Assert.Contains(s2.Id, _fin.CalledSaleIds);
    }

    [Fact]
    public async Task SyncShift_SkipsWhenFinDocExists()
    {
        var (_, shift) = await SeedStoreShiftAsync();
        var s1 = await SeedPaidSaleAsync(shift.Id, shift.StoreId, "S-HAD");
        var s2 = await SeedPaidSaleAsync(shift.Id, shift.StoreId, "S-NEW");
        _db.FinRevenueDocuments.Add(new FinRevenueDocument
        {
            TenantId = _tenant, Code = "DT-POS-1", Kind = "PosRevenue",
            SourceModule = "POS", SourceId = s1.Id, SourceCode = s1.Code,
            DocDate = DateTimeOffset.UtcNow, Status = "Draft", TotalAmount = 100_000,
        });
        await _db.SaveChangesAsync();

        var r = await _sales.SyncShiftRevenueToFinAsync(_tenant, _user, shift.Id);
        Assert.Equal(1, r.AlreadyHadCount);
        Assert.Equal(1, r.SyncedCount);
        Assert.Equal(1, _fin.PosCalls);
        Assert.Contains(s2.Id, _fin.CalledSaleIds);
        Assert.DoesNotContain(s1.Id, _fin.CalledSaleIds);
    }

    [Fact]
    public async Task SyncShift_IgnoresOpenSales()
    {
        var (_, shift) = await SeedStoreShiftAsync();
        await SeedPaidSaleAsync(shift.Id, shift.StoreId, "S-PAID");
        _db.PosSales.Add(new PosSale
        {
            TenantId = _tenant, CreatedBy = _user, Code = "S-OPEN",
            ShiftId = shift.Id, StoreId = shift.StoreId, Status = "Open",
            SubTotal = 1, TotalAmount = 1,
        });
        await _db.SaveChangesAsync();

        var r = await _sales.SyncShiftRevenueToFinAsync(_tenant, _user, shift.Id);
        Assert.Equal(1, r.PaidSaleCount);
        Assert.Equal(1, r.SyncedCount);
    }

    [Fact]
    public async Task CloseShift_AppendsFinTagToNote()
    {
        var (_, shift) = await SeedStoreShiftAsync();
        await SeedPaidSaleAsync(shift.Id, shift.StoreId, "S-CLOSE");

        var closed = await _sales.CloseShiftAsync(
            _tenant, _user, shift.Id, new PosShiftCloseRequest(100_000, "tay"));
        Assert.Equal("Closed", closed.Status);
        Assert.Contains("FIN:", closed.Note);
        Assert.Contains("tay", closed.Note);
        Assert.Equal(1, _fin.PosCalls);
    }

    [Fact]
    public async Task CloseShift_BlocksWhenOpenSalesRemain()
    {
        var (_, shift) = await SeedStoreShiftAsync();
        _db.PosSales.Add(new PosSale
        {
            TenantId = _tenant, CreatedBy = _user, Code = "OPEN1",
            ShiftId = shift.Id, StoreId = shift.StoreId, Status = "Open",
            SubTotal = 1, TotalAmount = 1,
        });
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<Erp.Application.Common.Exceptions.AppException>(() =>
            _sales.CloseShiftAsync(_tenant, _user, shift.Id, new PosShiftCloseRequest(0, null)));
        Assert.Equal(0, _fin.PosCalls);
    }

    [Fact]
    public async Task SyncShift_ZeroPaid_IsOk()
    {
        var (_, shift) = await SeedStoreShiftAsync();
        var r = await _sales.SyncShiftRevenueToFinAsync(_tenant, _user, shift.Id);
        Assert.Equal(0, r.PaidSaleCount);
        Assert.Equal(0, r.SyncedCount);
        Assert.Equal(0, _fin.PosCalls);
    }
}
