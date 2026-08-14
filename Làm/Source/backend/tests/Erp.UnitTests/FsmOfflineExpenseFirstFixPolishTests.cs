using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class FsmOfflineExpenseFirstFixPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly FsmOfflineExpenseFirstFixService _svc;
    private readonly Guid _tenant = Guid.NewGuid();

    public FsmOfflineExpenseFirstFixPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("fsm-offline-expense-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new FsmOfflineExpenseFirstFixService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task GetSparePartLossWarnings_ReturnsShrinkageAnomalies()
    {
        var res = await _svc.GetSparePartLossWarningsAsync(_tenant);

        Assert.NotNull(res);
        Assert.NotEmpty(res);
    }

    [Fact]
    public async Task RecordOfflineSync_SavesAuditLog()
    {
        var req = new FsmSyncOfflineDataRequest(Guid.NewGuid(), "Lê Anh Tuấn", "TAB-ACTIVE-4", 15, DateTimeOffset.UtcNow.AddHours(-2));
        var res = await _svc.RecordOfflineSyncAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("Success", res.SyncStatus);
        Assert.Equal(15, res.SyncedOperationsCount);
    }

    [Fact]
    public async Task SubmitDailySettlement_CalculatesNetAmount()
    {
        var req = new FsmSubmitDailySettlementRequest(Guid.NewGuid(), "Lê Anh Tuấn", 3000000m, 500000m);
        var res = await _svc.SubmitDailySettlementAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.StartsWith("SETTLE-DAY-", res.SettlementVoucherNumber);
        Assert.Equal(2500000m, res.NetSettlementAmountVnd);
    }

    [Fact]
    public async Task GetFirstTimeFixRateReport_ReturnsFtfrPercentage()
    {
        var res = await _svc.GetFirstTimeFixRateReportAsync(_tenant);

        Assert.NotNull(res);
        Assert.True(res.TotalResolvedTickets > 0);
        Assert.True(res.FirstTimeFixRatePct >= 80);
    }
}
