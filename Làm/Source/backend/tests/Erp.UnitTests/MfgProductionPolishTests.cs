using System;
using System.Linq;
using System.Threading.Tasks;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Mfg;
using Erp.Domain.Entities.Fin;
using Erp.Domain.Entities.Mfg;
using Erp.Infrastructure.Implementations.Services.Fin;
using Erp.Infrastructure.Implementations.Services.Mfg;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class MfgProductionPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly FinAccountingService _finSvc;
    private readonly MfgProductionService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _user = Guid.NewGuid();

    public MfgProductionPolishTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"MfgTestDb_{Guid.NewGuid()}")
            .Options;
        _db = new AppDbContext(options);
        _finSvc = new FinAccountingService(_db);
        _svc = new MfgProductionService(_db, _finSvc);
    }

    public void Dispose()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    [Fact]
    public async Task ReceiveFg_ValidQuantity_UpdatesWorkOrder_AndStatusCompleted()
    {
        var fgItem = new MfgItem { TenantId = _tenant, Code = "FG-01", Name = "Áo Thun Nam", ItemType = "FG", Unit = "CAI", CreatedBy = _user };
        _db.MfgItems.Add(fgItem);
        var wo = new MfgWorkOrder
        {
            TenantId = _tenant, Code = "WO-2026-001", ItemId = fgItem.Id, Qty = 100,
            Status = "Released", QtyFgReceived = 0, CreatedBy = _user
        };
        _db.MfgWorkOrders.Add(wo);
        await _db.SaveChangesAsync();

        var updated = await _svc.ReceiveFgAsync(_tenant, _user, wo.Id, new MfgFgReceiptRequest(100, null));

        Assert.Equal("Completed", updated.Status);
        Assert.Equal(100, updated.QtyFgReceived);
        Assert.Single(_db.MfgFgReceipts.Where(x => x.WorkOrderId == wo.Id));
    }

    [Fact]
    public async Task ReceiveFg_DraftWorkOrder_ThrowsAppException()
    {
        var fgItem = new MfgItem { TenantId = _tenant, Code = "FG-02", Name = "Quần Jeans", ItemType = "FG", Unit = "CAI", CreatedBy = _user };
        _db.MfgItems.Add(fgItem);
        var wo = new MfgWorkOrder
        {
            TenantId = _tenant, Code = "WO-DRAFT", ItemId = fgItem.Id, Qty = 50,
            Status = "Draft", CreatedBy = _user
        };
        _db.MfgWorkOrders.Add(wo);
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<AppException>(
            () => _svc.ReceiveFgAsync(_tenant, _user, wo.Id, new MfgFgReceiptRequest(10, null)));
    }

    [Fact]
    public async Task ReceiveFg_Exceeding120Percent_ThrowsAppException()
    {
        var fgItem = new MfgItem { TenantId = _tenant, Code = "FG-03", Name = "Mũ Bảo Hiểm", ItemType = "FG", Unit = "CAI", CreatedBy = _user };
        _db.MfgItems.Add(fgItem);
        var wo = new MfgWorkOrder
        {
            TenantId = _tenant, Code = "WO-OVER", ItemId = fgItem.Id, Qty = 100,
            Status = "Released", QtyFgReceived = 50, CreatedBy = _user
        };
        _db.MfgWorkOrders.Add(wo);
        await _db.SaveChangesAsync();

        // 50 + 80 = 130 > 100 * 1.2 = 120 -> Exception
        await Assert.ThrowsAsync<AppException>(
            () => _svc.ReceiveFgAsync(_tenant, _user, wo.Id, new MfgFgReceiptRequest(80, null)));
    }

    [Fact]
    public async Task RecordScrap_ValidScrap_UpdatesScrapQuantity()
    {
        var fgItem = new MfgItem { TenantId = _tenant, Code = "FG-04", Name = "Giày Thể Thao", ItemType = "FG", Unit = "DOI", CreatedBy = _user };
        _db.MfgItems.Add(fgItem);
        var wo = new MfgWorkOrder
        {
            TenantId = _tenant, Code = "WO-SCRAP", ItemId = fgItem.Id, Qty = 200,
            Status = "Released", QtyScrap = 0, CreatedBy = _user
        };
        _db.MfgWorkOrders.Add(wo);
        await _db.SaveChangesAsync();

        var updated = await _svc.RecordScrapAsync(_tenant, _user, wo.Id, new MfgScrapRequest(fgItem.Id, 5, "DOI", "Scrap", "Lỗi may chỉ"));

        Assert.Equal(5, updated.QtyScrap);
        Assert.Single(_db.MfgScraps.Where(x => x.WorkOrderId == wo.Id));
    }

    [Fact]
    public async Task PushCost_CreatesAutoJournal_WipToTp()
    {
        var period = new FinPeriod
        {
            TenantId = _tenant, Name = "T08/2026", StartDate = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2026, 8, 31, 23, 59, 59, TimeSpan.Zero), Status = "Open", CreatedBy = _user
        };
        _db.FinPeriods.Add(period);

        var acc154 = new FinAccount { TenantId = _tenant, Code = "1541", Name = "WIP Chi phí SXKD", Status = "Active", IsPostable = true, CreatedBy = _user };
        var acc155 = new FinAccount { TenantId = _tenant, Code = "1551", Name = "Thành phẩm kho", Status = "Active", IsPostable = true, CreatedBy = _user };
        _db.FinAccounts.AddRange(acc154, acc155);

        var fgItem = new MfgItem { TenantId = _tenant, Code = "FG-05", Name = "Bàn Gỗ", ItemType = "FG", StandardCost = 1000, Unit = "CAI", CreatedBy = _user };
        _db.MfgItems.Add(fgItem);

        var wo = new MfgWorkOrder
        {
            TenantId = _tenant, Code = "WO-COST", ItemId = fgItem.Id, Qty = 10,
            Status = "Completed", QtyFgReceived = 10, CreatedBy = _user
        };
        _db.MfgWorkOrders.Add(wo);

        var sheet = new MfgCostSheet
        {
            TenantId = _tenant, Code = "CS-01", WorkOrderId = wo.Id, Status = "Calculated",
            TotalCost = 10_000_000, GoodQty = 10, UnitCost = 1_000_000, CreatedBy = _user
        };
        _db.MfgCostSheets.Add(sheet);
        await _db.SaveChangesAsync();

        var pushed = await _svc.PushCostAsync(_tenant, _user, wo.Id, new MfgCostPushRequest(period.Id, acc154.Id, acc155.Id, "Đẩy giá thành"));

        Assert.Equal("Pushed", pushed.Status);
        Assert.NotNull(pushed.FinJournalId);
        var je = await _db.FinJournals.FindAsync(pushed.FinJournalId);
        Assert.NotNull(je);
        Assert.Equal("Posted", je!.Status);
    }
}
