using Erp.Domain.Entities.Mfg;
using Erp.Infrastructure.Implementations.Services.Mfg;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class MfgReportPolishTests
{
    private static AppDbContext CreateDb(string dbName)
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(opts);
    }

    [Fact]
    public async Task OutputByPeriod_ComputesShiftsCorrectly()
    {
        using var db = CreateDb(nameof(OutputByPeriod_ComputesShiftsCorrectly));
        var svc = new MfgReportService(db);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var ws = new MfgWorkshop { TenantId = tenantId, Code = "WS-01", Name = "Xưởng cơ khí", CreatedBy = userId };
        var item = new MfgItem { TenantId = tenantId, Code = "ITEM-01", Name = "Sản phẩm A", ItemType = "FG", CreatedBy = userId };
        db.MfgWorkshops.Add(ws);
        db.MfgItems.Add(item);
        await db.SaveChangesAsync();

        var wo = new MfgWorkOrder
        {
            TenantId = tenantId, Code = "WO-100", ItemId = item.Id, Qty = 100, WorkshopId = ws.Id,
            Status = "Released", CreatedBy = userId
        };
        db.MfgWorkOrders.Add(wo);
        await db.SaveChangesAsync();

        var dateShift1 = new DateTimeOffset(2026, 8, 7, 8, 30, 0, TimeSpan.FromHours(7)); // Ca 1 (8h30)
        var dateShift2 = new DateTimeOffset(2026, 8, 7, 15, 0, 0, TimeSpan.FromHours(7)); // Ca 2 (15h00)

        db.MfgFgReceipts.AddRange(
            new MfgFgReceipt { TenantId = tenantId, WorkOrderId = wo.Id, ItemId = item.Id, Qty = 30, ReceivedAt = dateShift1, CreatedBy = userId },
            new MfgFgReceipt { TenantId = tenantId, WorkOrderId = wo.Id, ItemId = item.Id, Qty = 40, ReceivedAt = dateShift2, CreatedBy = userId }
        );
        await db.SaveChangesAsync();

        var output = await svc.OutputByPeriodAsync(tenantId, dateShift1.AddDays(-1), dateShift2.AddDays(1));
        Assert.Equal(2, output.Count);
        Assert.Contains(output, x => x.ShiftLabel.StartsWith("Ca 1"));
        Assert.Contains(output, x => x.ShiftLabel.StartsWith("Ca 2"));
        Assert.Equal(70, output.Sum(x => x.QtyFg));
    }

    [Fact]
    public async Task WoProgress_CalculatesPercentAndListsWorkOrders()
    {
        using var db = CreateDb(nameof(WoProgress_CalculatesPercentAndListsWorkOrders));
        var svc = new MfgReportService(db);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var item = new MfgItem { TenantId = tenantId, Code = "ITEM-02", Name = "Sản phẩm B", ItemType = "FG", CreatedBy = userId };
        db.MfgItems.Add(item);
        await db.SaveChangesAsync();

        db.MfgWorkOrders.Add(new MfgWorkOrder
        {
            TenantId = tenantId, Code = "WO-200", ItemId = item.Id, Qty = 200, QtyFgReceived = 150, QtyScrap = 10,
            Status = "MaterialsIssued", CreatedBy = userId
        });
        await db.SaveChangesAsync();

        var progress = await svc.WoProgressAsync(tenantId);
        Assert.Single(progress);
        Assert.Equal(75.0m, progress[0].ProgressPercent);
        Assert.Equal("WO-200", progress[0].Code);
    }
}
