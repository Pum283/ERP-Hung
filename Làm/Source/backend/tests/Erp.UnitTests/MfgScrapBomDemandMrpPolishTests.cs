using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class MfgScrapBomDemandMrpPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly MfgScrapBomDemandMrpService _svc;
    private readonly Guid _tenant = Guid.NewGuid();

    public MfgScrapBomDemandMrpPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("mfg-scrap-mrp-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new MfgScrapBomDemandMrpService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task SetBomScrapAllowance_CalculatesGrossPlannedQuantity()
    {
        var req = new MfgSetBomScrapAllowanceRequest(Guid.NewGuid(), "BOM-SERVER-RACK", Guid.NewGuid(), "MAT-STEEL-SHEET", "Tấm Thép Tĩnh Điện", 100m, 5.0m, "Hao hụt cắt gọt CNC");
        var res = await _svc.SetBomScrapAllowanceAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(100m, res.BaseNetQuantity);
        Assert.Equal(5.0m, res.ScrapAllowancePct);
        Assert.Equal(105m, res.GrossPlannedQuantity);
    }

    [Fact]
    public async Task CopyBom_GeneratesNewVersionCode()
    {
        var req = new MfgCopyBomRequest(Guid.NewGuid(), "BOM-DESK-01", "v1.0", "v1.1-EXPORT", "Kỹ Sư Tuấn");
        var res = await _svc.CopyBomAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("BOM-DESK-01-v1.1-EXPORT", res.NewBomCode);
        Assert.Equal(12, res.CopiedLinesCount);
    }

    [Fact]
    public async Task CreateDemandProductionPlan_SumsForecastAndBacklog()
    {
        var req = new MfgCreateDemandProductionPlanRequest("Kế Hoạch Tháng 9/2026", Guid.NewGuid(), "FG-CABINET-01", "Tủ Rack Server 42U", 80m, 45m, "Monthly-2026-09");
        var res = await _svc.CreateDemandProductionPlanAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.StartsWith("MPS-", res.PlanNumber);
        Assert.Equal(125m, res.PlannedProductionQty);
    }

    [Fact]
    public async Task RunMrpCalculation_CalculatesNetRequirementDeficit()
    {
        var req = new MfgRunMrpCalculationRequest(Guid.NewGuid(), "MAT-BOLT-M8", "Bu Lông Inox M8", 500m, 150m, 50m, DateTimeOffset.UtcNow.AddDays(5));
        var res = await _svc.RunMrpCalculationAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.StartsWith("MRP-", res.MrpRunNumber);
        Assert.Equal(300m, res.NetRequirementQty);
        Assert.Equal(300m, res.SuggestedPurchaseOrderQty);
    }
}
