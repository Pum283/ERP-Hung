using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class MfgQuarantineYieldBatchParamPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly MfgQuarantineYieldBatchParamService _svc;
    private readonly Guid _tenant = Guid.NewGuid();

    public MfgQuarantineYieldBatchParamPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("mfg-quarantine-batch-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new MfgQuarantineYieldBatchParamService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CreateQuarantineHold_SetsUnderQuarantineStatus()
    {
        var req = new MfgCreateQuarantineHoldRequest("LOT-2026-0814", "ITEM-STEEL", 25, "KHO-CACH-LY-01", "Sai kích thước biên dạng");
        var res = await _svc.CreateQuarantineHoldAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.StartsWith("Q-HOLD-", res.QuarantineHoldNumber);
        Assert.Equal("UnderQuarantine", res.Status);
        Assert.Equal(25, res.QuarantinedQuantity);
    }

    [Fact]
    public async Task GetQualityYieldSummary_ReturnsMetrics()
    {
        var res = await _svc.GetQualityYieldSummaryAsync(_tenant);

        Assert.NotNull(res);
        Assert.True(res.TotalInspectedLots > 0);
        Assert.True(res.OverallPassRatePct > 90);
    }

    [Fact]
    public async Task CreateBatchLot_GeneratesBatchNumber()
    {
        var req = new MfgCreateBatchLotRequest(Guid.NewGuid(), "WO-TEST-BATCH", "FG-CABINET-42U", 300, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(2));
        var res = await _svc.CreateBatchLotAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.StartsWith("BATCH-", res.BatchNumber);
        Assert.Equal(300, res.BatchSizePlannedQty);
    }

    [Fact]
    public async Task LogBatchParameter_SavesTelemetry()
    {
        var req = new MfgLogBatchParameterRequest("BATCH-20260814", "Áp Suất Máy Ép Thủy Lực", 150m, 150.8m, "Bar", true, "Kỹ Sư Tuấn");
        var res = await _svc.LogBatchParameterAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("BATCH-20260814", res.BatchNumber);
        Assert.True(res.IsWithinTolerance);
    }
}
