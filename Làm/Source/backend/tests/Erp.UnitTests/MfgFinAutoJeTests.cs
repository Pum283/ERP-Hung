using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Fin;
using Erp.Application.DTOs.Mfg;
using Erp.Domain.Entities.Fin;
using Erp.Domain.Entities.Inv;
using Erp.Domain.Entities.Mfg;
using Erp.Infrastructure.Implementations.Services.Fin;
using Erp.Infrastructure.Implementations.Services.Mfg;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.UnitTests;

/// <summary>UC_MFG_031 + UC_FIN_015 — đẩy giá thành tạo JE WIP→TP thật · BT Auto Source filter.</summary>
public sealed class MfgFinAutoJeTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly FinAccountingService _fin;
    private readonly MfgProductionService _mfg;
    private readonly Guid _tenant = Guid.Parse("eeeeeeee-ffff-0000-1111-222222222222");
    private readonly Guid _user = Guid.Parse("55555555-6666-7777-8888-999999999999");

    public MfgFinAutoJeTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("mfg-fin-auto-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _fin = new FinAccountingService(_db);
        _mfg = new MfgProductionService(_db, _fin);
    }

    public void Dispose() => _db.Dispose();

    private (MfgWorkOrder Wo, MfgCostSheet Sheet, FinPeriod Period) SeedCost(
        decimal total = 5_000_000m, decimal qty = 100m)
    {
        var item = new MfgItem
        {
            TenantId = _tenant, Code = "FG-01", Name = "Thành phẩm 1",
            ItemType = "FG", Unit = "CAI", CreatedBy = _user,
        };
        _db.MfgItems.Add(item);
        var wo = new MfgWorkOrder
        {
            TenantId = _tenant, Code = "LSX-01", ItemId = item.Id, Qty = qty,
            Status = "Completed", CreatedByUserId = _user, CreatedBy = _user,
        };
        _db.MfgWorkOrders.Add(wo);
        var sheet = new MfgCostSheet
        {
            TenantId = _tenant, Code = "GT-01", WorkOrderId = wo.Id, Status = "Calculated",
            MaterialCost = total, TotalCost = total, GoodQty = qty,
            UnitCost = decimal.Round(total / qty, 4),
            CalculatedAt = DateTimeOffset.UtcNow, CalculatedByUserId = _user, CreatedBy = _user,
        };
        _db.MfgCostSheets.Add(sheet);

        var now = DateTimeOffset.UtcNow;
        var period = new FinPeriod
        {
            TenantId = _tenant, FiscalYearId = Guid.NewGuid(),
            Code = $"{now:yyyy-MM}", Name = $"Tháng {now:MM/yyyy}",
            StartDate = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month), 0, 0, 0, TimeSpan.Zero),
            Status = "Open", CreatedBy = _user,
        };
        _db.FinPeriods.Add(period);
        _db.FinAccounts.AddRange(
            new FinAccount { TenantId = _tenant, Code = "1541", Name = "Chi phí SX dở dang", AccountType = "Asset", CreatedBy = _user },
            new FinAccount { TenantId = _tenant, Code = "1551", Name = "Thành phẩm", AccountType = "Asset", CreatedBy = _user });

        var uom = new InvUnitOfMeasure { TenantId = _tenant, Code = "CAI", Name = "Cái", CreatedBy = _user };
        _db.InvUnitsOfMeasure.Add(uom);
        _db.InvSkus.Add(new InvSku
        {
            TenantId = _tenant, Code = "FG-01", Name = "Thành phẩm 1",
            BaseUnitId = uom.Id, Status = "Active", CreatedBy = _user,
        });
        _db.SaveChanges();
        return (wo, sheet, period);
    }

    [Fact]
    public async Task PushCost_CreatesPostedWipToFgJournal_AndUpdatesInvStandardCost()
    {
        var (wo, _, _) = SeedCost();

        var dto = await _mfg.PushCostAsync(_tenant, _user, wo.Id, null);

        Assert.Equal("Pushed", dto.Status);
        Assert.NotNull(dto.FinJournalId);
        Assert.NotNull(dto.FinJournalCode);
        Assert.Equal("FG-01", dto.InvSkuCode);

        var je = await _db.FinJournals.SingleAsync(x => x.Id == dto.FinJournalId);
        Assert.Equal("Posted", je.Status);
        Assert.Equal("Auto", je.Source);
        var lines = await _db.FinJournalLines.Where(x => x.JournalId == je.Id).ToListAsync();
        Assert.Equal(2, lines.Count);
        Assert.Equal(lines.Sum(x => x.Debit), lines.Sum(x => x.Credit));
        Assert.Equal(5_000_000m, lines.Sum(x => x.Debit));

        var sku = await _db.InvSkus.SingleAsync(x => x.Code == "FG-01");
        Assert.Equal(50_000m, sku.StandardCost);
        var item = await _db.MfgItems.SingleAsync(x => x.Code == "FG-01");
        Assert.Equal(50_000m, item.StandardCost);
    }

    [Fact]
    public async Task PushCost_RejectsWhenAlreadyPushed()
    {
        var (wo, _, _) = SeedCost();
        await _mfg.PushCostAsync(_tenant, _user, wo.Id, null);

        await Assert.ThrowsAsync<AppException>(() => _mfg.PushCostAsync(_tenant, _user, wo.Id, null));
    }

    [Fact]
    public async Task PushCost_RejectsWhenNoOpenPeriodAndNoPeriodId()
    {
        var (wo, _, period) = SeedCost();
        period.Status = "Locked";
        await _db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<AppException>(() => _mfg.PushCostAsync(_tenant, _user, wo.Id, null));
        Assert.Contains("kỳ FIN", ex.Message);
    }

    [Fact]
    public async Task PushCost_ZeroTotal_PushesInvWithoutJournal()
    {
        var (wo, sheet, _) = SeedCost(total: 0m, qty: 10m);
        sheet.UnitCost = 0;
        await _db.SaveChangesAsync();

        var dto = await _mfg.PushCostAsync(_tenant, _user, wo.Id, null);

        Assert.Equal("Pushed", dto.Status);
        Assert.Null(dto.FinJournalId);
        Assert.Equal(0, await _db.FinJournals.CountAsync());
    }

    [Fact]
    public async Task CreateAutoJournal_SetsSourceAuto()
    {
        var (_, _, period) = SeedCost();
        var wip = await _db.FinAccounts.SingleAsync(x => x.Code == "1541");
        var fg = await _db.FinAccounts.SingleAsync(x => x.Code == "1551");

        var je = await _fin.CreateAutoJournalAsync(_tenant, _user, new FinJournalUpsertRequest(
            null, null, period.Id, DateTimeOffset.UtcNow, "BT auto test",
            null, null, null,
            [
                new(null, fg.Id, 1000, 0, null, null, null),
                new(null, wip.Id, 0, 1000, null, null, null),
            ]));

        Assert.Equal("Auto", je.Source);
        Assert.Equal("Draft", je.Status);
    }

    [Fact]
    public async Task ListJournals_FiltersBySourceAuto()
    {
        var (_, _, period) = SeedCost();
        var wip = await _db.FinAccounts.SingleAsync(x => x.Code == "1541");
        var fg = await _db.FinAccounts.SingleAsync(x => x.Code == "1551");

        await _fin.UpsertJournalAsync(_tenant, _user, new FinJournalUpsertRequest(
            null, null, period.Id, DateTimeOffset.UtcNow, "Manual JE",
            null, null, "Manual",
            [
                new(null, fg.Id, 100, 0, null, null, null),
                new(null, wip.Id, 0, 100, null, null, null),
            ]));
        await _fin.CreateAutoJournalAsync(_tenant, _user, new FinJournalUpsertRequest(
            null, null, period.Id, DateTimeOffset.UtcNow, "Auto JE",
            null, null, null,
            [
                new(null, fg.Id, 200, 0, null, null, null),
                new(null, wip.Id, 0, 200, null, null, null),
            ]));

        var auto = await _fin.ListJournalsAsync(_tenant, null, "Auto");
        Assert.Single(auto);
        Assert.Equal("Auto JE", auto[0].Description);
    }
}
