using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class MfgCostVarianceQcInspectionPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly MfgCostVarianceQcInspectionService _svc;
    private readonly Guid _tenant = Guid.NewGuid();

    public MfgCostVarianceQcInspectionPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("mfg-cost-qc-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new MfgCostVarianceQcInspectionService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task AnalyzeCostVariance_CalculatesDifferenceAndPercentage()
    {
        var req = new MfgAnalyzeCostVarianceRequest(Guid.NewGuid(), "WO-TEST-COST", 10000000m, 11500000m, "Hao hụt phôi và chi phí máy ngoài giờ");
        var res = await _svc.AnalyzeCostVarianceAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(1500000m, res.CostVarianceVnd);
        Assert.Equal(15.0, res.VariancePercentage);
        Assert.StartsWith("VAR-COST-", res.AnalysisNumber);
    }

    [Fact]
    public async Task CreateIncomingQcCriterion_SavesSpecification()
    {
        var req = new MfgCreateIncomingQcCriterionRequest("QC-TEST-STEEL", "Độ Phẳng Tấm Thép", "Kim Loại Tấm", "Độ vênh tối đa 0.5mm", "Thước đo độ phẳng", 0m, 0.5m, true);
        var res = await _svc.CreateIncomingQcCriterionAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("QC-TEST-STEEL", res.CriterionCode);
        Assert.True(res.IsMandatory);
    }

    [Fact]
    public async Task PerformFinishedGoodsQc_SetsPassResult()
    {
        var req = new MfgPerformFinishedGoodsQcRequest(Guid.NewGuid(), "WO-FG-001", "FG-CABINET", 20, 0, "Pass", "Kỹ Sư An");
        var res = await _svc.PerformFinishedGoodsQcAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.StartsWith("FQC-", res.InspectionNumber);
        Assert.Equal("Pass", res.InspectionResult);
    }

    [Fact]
    public async Task DecideLotDisposition_SavesAcceptedAndRejectedQty()
    {
        var req = new MfgDecideLotDispositionRequest("LOT-2026-08-01", "FG-CABINET", 100, 98, 2, "ReleaseToStock", "2 sản phẩm lỗi chuyển sang lệnh tái chế");
        var res = await _svc.DecideLotDispositionAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(98, res.AcceptedQuantity);
        Assert.Equal(2, res.RejectedQuantity);
        Assert.Equal("ReleaseToStock", res.DispositionDecision);
    }
}
