using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class MfgScheduleProgressReworkCostPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly MfgScheduleProgressReworkCostService _svc;
    private readonly Guid _tenant = Guid.NewGuid();

    public MfgScheduleProgressReworkCostPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("mfg-sched-prog-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new MfgScheduleProgressReworkCostService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CreateWorkshopShiftSchedule_SavesScheduleTarget()
    {
        var req = new MfgCreateWorkshopShiftScheduleRequest("Xưởng Hàn 2", "Ca 1", DateTimeOffset.UtcNow, Guid.NewGuid(), "WO-TEST-01", 150);
        var res = await _svc.CreateWorkshopShiftScheduleAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.StartsWith("SCHED-", res.ScheduleNumber);
        Assert.Equal(150, res.TargetQuantity);
    }

    [Fact]
    public async Task LogOperationProgress_SavesCompletedAndDefectQty()
    {
        var req = new MfgLogOperationProgressRequest(Guid.NewGuid(), "WO-TEST-01", "OP-WELD", "Hàn Khung", 48, 2, "Thợ Hàn Nam");
        var res = await _svc.LogOperationProgressAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(48, res.CompletedQuantity);
        Assert.Equal(2, res.DefectiveQuantity);
    }

    [Fact]
    public async Task CreateReworkWorkOrder_SetsReworkStatus()
    {
        var req = new MfgCreateReworkWorkOrderRequest(Guid.NewGuid(), "WO-MAIN-001", "Lỗi sơn bọt khí", 5, "WC-PAINT-REWORK");
        var res = await _svc.CreateReworkWorkOrderAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.StartsWith("WO-REWORK-", res.ReworkWoNumber);
        Assert.Equal("Approved", res.Status);
    }

    [Fact]
    public async Task AllocateOverheadCost_CalculatesUnitCost()
    {
        var req = new MfgAllocateOverheadCostRequest(Guid.NewGuid(), "WO-MAIN-001", 5000000m, 2000000m, 3000000m, 100);
        var res = await _svc.AllocateOverheadCostAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(10000000m, res.TotalAllocatedCostVnd);
        Assert.Equal(100000m, res.UnitCostVnd);
    }
}
