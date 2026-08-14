using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class FsmServicePricingPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly FsmServicePricingService _svc;
    private readonly Guid _tenant = Guid.NewGuid();

    public FsmServicePricingPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("fsm-pricing-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new FsmServicePricingService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CreateServicePriceRate_SavesHourlyAndTravelRates()
    {
        var req = new FsmCreateServicePriceRateRequest("FSM-EMERGENCY", "Cứu Hộ Thiết Bị Khẩn Cấp 24/7", "Cứu Hộ", 500000m, 300000m, 50m);
        var res = await _svc.CreateServicePriceRateAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("FSM-EMERGENCY", res.ServiceCode);
        Assert.Equal(500000m, res.BaseHourlyRateVnd);
        Assert.Equal(300000m, res.StandardTravelFeeVnd);
    }

    [Fact]
    public async Task GetServicePriceRates_ReturnsPreconfiguredRates()
    {
        var res = await _svc.GetServicePriceRatesAsync(_tenant);

        Assert.NotNull(res);
        Assert.NotEmpty(res);
    }
}
