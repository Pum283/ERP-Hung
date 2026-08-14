using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class LogDriverProductivityCostPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LogDriverProductivityCostService _svc;
    private readonly Guid _tenant = Guid.NewGuid();

    public LogDriverProductivityCostPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("log-prod-cost-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new LogDriverProductivityCostService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task GetDriverProductivityReport_ReturnsKpis()
    {
        var res = await _svc.GetDriverProductivityReportAsync(_tenant);

        Assert.NotNull(res);
        Assert.True(res.TotalActiveDrivers > 0);
        Assert.NotEmpty(res.Drivers);
    }

    [Fact]
    public async Task CalculateTripCost_CalculatesAveragePerOrder()
    {
        var req = new LogCalculateTripCostRequest("TRIP-2026-0814", 450000m, 120000m, 200000m, 5);
        var res = await _svc.CalculateTripCostAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(770000m, res.TotalTripCostVnd);
        Assert.Equal(154000m, res.AverageCostPerOrderVnd);
        Assert.StartsWith("COST-ALLOC-", res.CostAllocationNumber);
    }
}
