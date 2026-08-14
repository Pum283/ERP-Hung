using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class LogFleetPricingZonePolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LogFleetPricingZoneService _svc;
    private readonly Guid _tenant = Guid.NewGuid();

    public LogFleetPricingZonePolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("log-fleet-pricing-zone-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new LogFleetPricingZoneService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CreateDriverVehicle_SavesAndReturnsFleetMember()
    {
        var req = new LogCreateDriverVehicleRequest("Nguyễn Văn Vận Tải", "0987654321", "FC-123456", "29C-999.88", "Truck-5T", 5000);
        var res = await _svc.CreateDriverVehicleAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("Nguyễn Văn Vận Tải", res.DriverName);
        Assert.Equal("29C-999.88", res.VehiclePlateNumber);
        Assert.True(res.IsActive);
    }

    [Fact]
    public async Task CreateFreightPricingRate_ConfiguresBaseAndKmRates()
    {
        var req = new LogCreateFreightPricingRateRequest("RATE-CONTAINER", "Container", 1200000m, 45000m, 35000m, 300000m);
        var res = await _svc.CreateFreightPricingRateAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("RATE-CONTAINER", res.RateCode);
        Assert.Equal(1200000m, res.BasePriceVnd);
    }

    [Fact]
    public async Task CreateDeliveryZoneConfig_SavesDistrictCoverageList()
    {
        var districts = new List<string> { "Quận 1", "Quận 3", "Quận 7" };
        var req = new LogCreateDeliveryZoneConfigRequest("ZONE-HCM-TRUNGTAM", "Khu Vực Trung Tâm TP.HCM", "TP. Hồ Chí Minh", districts, 2);
        var res = await _svc.CreateDeliveryZoneConfigAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("ZONE-HCM-TRUNGTAM", res.ZoneCode);
        Assert.Equal(3, res.DistrictCoverageList.Count);
        Assert.Equal(2, res.EstimatedTransitHours);
    }
}
