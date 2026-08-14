using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class PurOtdReportRfqSavingsPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PurOtdReportRfqSavingsService _svc;
    private readonly Guid _tenant = Guid.NewGuid();

    public PurOtdReportRfqSavingsPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pur-otd-savings-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new PurOtdReportRfqSavingsService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task GetVendorOtdPerformanceReport_ReturnsPerformanceRatings()
    {
        var res = await _svc.GetVendorOtdPerformanceReportAsync(_tenant);

        Assert.NotNull(res);
        Assert.NotEmpty(res);
        Assert.Contains(res, v => v.PerformanceRating == "Excellent");
    }

    [Fact]
    public async Task GetRfqSavingsSummaryReport_CalculatesOverallSavingsPercentage()
    {
        var res = await _svc.GetRfqSavingsSummaryReportAsync(_tenant);

        Assert.NotNull(res);
        Assert.True(res.TotalSavingsVnd > 0);
        Assert.True(res.OverallSavingsPercentage > 0);
        Assert.NotEmpty(res.SavingsList);
    }
}
