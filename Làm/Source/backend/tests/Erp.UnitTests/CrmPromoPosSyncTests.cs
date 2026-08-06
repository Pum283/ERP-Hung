using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Domain.Entities.Crm;
using Erp.Infrastructure.Implementations.Services.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.UnitTests;

/// <summary>UC_CRM_036 sync POS + UC_CRM_038 voucher usage report — EF InMemory.</summary>
public sealed class CrmPromoPosSyncTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmPromotionService _promos;
    private readonly Guid _tenant = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private readonly Guid _user = Guid.Parse("44444444-4444-4444-4444-444444444444");

    public CrmPromoPosSyncTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-pos-sync-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _promos = new CrmPromotionService(_db);
    }

    public void Dispose() => _db.Dispose();

    private Task<CrmPromotionDto> SeedPromoAsync(string code = "PROMO-SYNC", string type = "Percentage", decimal value = 10)
        => _promos.UpsertAsync(_tenant, _user, new CrmPromotionUpsertRequest(
            null, code, "Sync me", null, type, value, null, 50_000,
            null, null, 100, 1, null, null));

    [Fact]
    public void MapDiscountToPos_MapsKnownTypes()
    {
        Assert.Equal("Percent", CrmPromotionService.MapDiscountToPos("Percentage"));
        Assert.Equal("Amount", CrmPromotionService.MapDiscountToPos("FixedAmount"));
        Assert.Null(CrmPromotionService.MapDiscountToPos("FreeShipping"));
        Assert.Null(CrmPromotionService.MapDiscountToPos("BuyXGetY"));
    }

    [Fact]
    public async Task SyncToPos_CreatesPosPromotionAndVouchers()
    {
        var p = await SeedPromoAsync();
        await _promos.GenerateVouchersAsync(_tenant, _user, new CrmVoucherGenerateRequest(
            p.Id, 3, "SYNC", 1, null));

        var r = await _promos.SyncToPosAsync(_tenant, _user, p.Id);
        Assert.True(r.Created);
        Assert.Equal(3, r.VouchersSynced);
        Assert.Equal(1, await _db.PosPromotions.CountAsync());
        Assert.Equal(3, await _db.PosVouchers.CountAsync());
        var pos = await _db.PosPromotions.FirstAsync();
        Assert.Equal("Percent", pos.DiscountType);
        Assert.Equal(10, pos.DiscountValue);
    }

    [Fact]
    public async Task SyncToPos_UpdatesExistingByCode()
    {
        var p = await SeedPromoAsync("PROMO-UP", "FixedAmount", 25_000);
        var first = await _promos.SyncToPosAsync(_tenant, _user, p.Id);
        Assert.True(first.Created);

        await _promos.UpsertAsync(_tenant, _user, new CrmPromotionUpsertRequest(
            p.Id, "PROMO-UP", "Updated name", null, "FixedAmount", 30_000, null, 0,
            null, null, 100, 1, null, null));
        var second = await _promos.SyncToPosAsync(_tenant, _user, p.Id);
        Assert.False(second.Created);
        Assert.Equal(first.PosPromotionId, second.PosPromotionId);
        Assert.Equal(30_000, (await _db.PosPromotions.FirstAsync()).DiscountValue);
        Assert.Equal("Amount", (await _db.PosPromotions.FirstAsync()).DiscountType);
    }

    [Fact]
    public async Task SyncToPos_RejectsFreeShipping()
    {
        var p = await SeedPromoAsync("PROMO-FS", "FreeShipping", 0);
        // DiscountValue 0 also fails — set entity to FreeShipping with value 1
        var e = await _db.CrmPromotions.FirstAsync(x => x.Id == p.Id);
        e.DiscountType = "FreeShipping";
        e.DiscountValue = 1;
        await _db.SaveChangesAsync();
        await Assert.ThrowsAsync<AppException>(() => _promos.SyncToPosAsync(_tenant, _user, p.Id));
    }

    [Fact]
    public async Task SyncToPos_SkipsExpiredVouchers()
    {
        var p = await SeedPromoAsync("PROMO-SKIP");
        await _promos.GenerateVouchersAsync(_tenant, _user, new CrmVoucherGenerateRequest(
            p.Id, 2, "OK", 1, null));
        _db.CrmVouchers.Add(new CrmVoucher
        {
            TenantId = _tenant, CreatedBy = _user, PromotionId = p.Id,
            VoucherCode = "EXPIRED1", Status = "Expired", MaxUsage = 1,
        });
        await _db.SaveChangesAsync();

        var r = await _promos.SyncToPosAsync(_tenant, _user, p.Id);
        Assert.Equal(2, r.VouchersSynced);
        Assert.Equal(1, r.VouchersSkipped);
    }

    [Fact]
    public async Task VoucherUsageReport_AggregatesByVoucher()
    {
        var p = await SeedPromoAsync("PROMO-RPT");
        var vouchers = await _promos.GenerateVouchersAsync(_tenant, _user, new CrmVoucherGenerateRequest(
            p.Id, 2, "RPT", 5, null));
        var v1 = vouchers[0];
        var v2 = vouchers[1];

        for (var i = 0; i < 3; i++)
            _db.CrmVoucherUsages.Add(new CrmVoucherUsage
            {
                TenantId = _tenant, CreatedBy = _user, VoucherId = v1.Id,
                DiscountApplied = 10_000, UsedAt = DateTimeOffset.UtcNow.AddMinutes(-i),
            });
        _db.CrmVoucherUsages.Add(new CrmVoucherUsage
        {
            TenantId = _tenant, CreatedBy = _user, VoucherId = v2.Id,
            DiscountApplied = 5_000, UsedAt = DateTimeOffset.UtcNow,
        });
        await _db.SaveChangesAsync();

        var rows = await _promos.GetVoucherUsageReportAsync(_tenant, p.Id);
        Assert.Equal(2, rows.Count);
        Assert.Equal(3, rows[0].RedeemCount);
        Assert.Equal(30_000, rows[0].TotalDiscount);
        Assert.Equal(1, rows[1].RedeemCount);
    }

    [Fact]
    public async Task VoucherUsageReport_FiltersByDate()
    {
        var p = await SeedPromoAsync("PROMO-DATE");
        var vouchers = await _promos.GenerateVouchersAsync(_tenant, _user, new CrmVoucherGenerateRequest(
            p.Id, 1, "DT", 5, null));
        _db.CrmVoucherUsages.Add(new CrmVoucherUsage
        {
            TenantId = _tenant, CreatedBy = _user, VoucherId = vouchers[0].Id,
            DiscountApplied = 1, UsedAt = DateTimeOffset.UtcNow.AddDays(-10),
        });
        _db.CrmVoucherUsages.Add(new CrmVoucherUsage
        {
            TenantId = _tenant, CreatedBy = _user, VoucherId = vouchers[0].Id,
            DiscountApplied = 2, UsedAt = DateTimeOffset.UtcNow,
        });
        await _db.SaveChangesAsync();

        var rows = await _promos.GetVoucherUsageReportAsync(
            _tenant, p.Id, DateTimeOffset.UtcNow.AddDays(-1), null);
        Assert.Single(rows);
        Assert.Equal(1, rows[0].RedeemCount);
        Assert.Equal(2, rows[0].TotalDiscount);
    }

    [Fact]
    public async Task VoucherUsageReport_EmptyWhenNoUsage()
    {
        var p = await SeedPromoAsync("PROMO-EMPTY");
        var rows = await _promos.GetVoucherUsageReportAsync(_tenant, p.Id);
        Assert.Empty(rows);
    }
}
