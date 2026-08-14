using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class InvDispatchPurposeReportPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly InvDispatchPurposeReportService _svc;
    private readonly Guid _tenant = Guid.NewGuid();

    public InvDispatchPurposeReportPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("inv-dispatch-purpose-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new InvDispatchPurposeReportService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task GetDispatchPurposeSummaryReport_ReturnsGroupedBreakdown()
    {
        var res = await _svc.GetDispatchPurposeSummaryReportAsync(_tenant);

        Assert.NotNull(res);
        Assert.True(res.TotalDispatchCount > 0);
        Assert.True(res.TotalDispatchedValueVnd > 0);
        Assert.NotEmpty(res.Categories);
    }
}
