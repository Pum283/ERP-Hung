using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class MfgPackBlendOeePolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly MfgPackBlendOeeService _svc;
    private readonly Guid _tenant = Guid.NewGuid();

    public MfgPackBlendOeePolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("mfg-pack-oee-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new MfgPackBlendOeeService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CreatePackagingLabelTag_SavesPackagingSpecs()
    {
        var req = new MfgCreatePackagingLabelRequest("FG-RACK-42U", "Thùng Gỗ Tiêu Chuẩn", 1, "GS1-128", "/labels/rack.prn");
        var res = await _svc.CreatePackagingLabelTagAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("FG-RACK-42U", res.ProductCode);
        Assert.True(res.IsActive);
    }

    [Fact]
    public async Task CreateBlendingRecipeRatio_SavesRatioAndTolerance()
    {
        var req = new MfgCreateBlendingRecipeRequest("RECIPE-GLUE", "Keo Dán Gỗ Công Nghiệp", "MAT-POLYVINYL", "Nhựa Polyvinyl", 40.0m, 0.5m, "Bước 1");
        var res = await _svc.CreateBlendingRecipeRatioAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("RECIPE-GLUE", res.RecipeCode);
        Assert.Equal(40.0m, res.MixingRatioPercentage);
    }

    [Fact]
    public async Task CalculateOee_CalculatesMultipliedOeePercentage()
    {
        var req = new MfgCalculateOeeRequest("WC-CNC-01", "Máy Phay CNC", 90.0, 90.0, 90.0);
        var res = await _svc.CalculateOeeAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(72.9, res.OverallOeePct);
    }
}
