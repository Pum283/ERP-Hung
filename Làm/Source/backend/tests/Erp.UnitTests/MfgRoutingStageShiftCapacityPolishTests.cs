using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class MfgRoutingStageShiftCapacityPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly MfgRoutingStageShiftCapacityService _svc;
    private readonly Guid _tenant = Guid.NewGuid();

    public MfgRoutingStageShiftCapacityPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("mfg-routing-capacity-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new MfgRoutingStageShiftCapacityService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CreateRoutingStage_SavesCycleAndSetupTimes()
    {
        var req = new MfgCreateRoutingStageRequest("OP-TEST-STAMP", "Dập Khuôn Định Hình", "WC-STAMP-01", 10m, 25m, false);
        var res = await _svc.CreateRoutingStageAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("OP-TEST-STAMP", res.StageCode);
        Assert.Equal(10m, res.StandardCycleTimeMinutes);
    }

    [Fact]
    public async Task CreateShiftCapacity_SavesWorkCenterCapacity()
    {
        var req = new MfgCreateShiftCapacityRequest("SHIFT-NIGHT", "Ca 3 Đêm", "WC-CNC-01", 8m, 80m, 380m);
        var res = await _svc.CreateShiftCapacityAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("SHIFT-NIGHT", res.ShiftCode);
        Assert.Equal(380m, res.MaxCapacityOutputUnits);
    }
}
