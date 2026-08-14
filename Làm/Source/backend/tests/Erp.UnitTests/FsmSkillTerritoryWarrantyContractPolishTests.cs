using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class FsmSkillTerritoryWarrantyContractPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly FsmSkillTerritoryWarrantyContractService _svc;
    private readonly Guid _tenant = Guid.NewGuid();

    public FsmSkillTerritoryWarrantyContractPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("fsm-skill-warranty-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new FsmSkillTerritoryWarrantyContractService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CreateTechnicianSkillCert_SavesCertificationDetails()
    {
        var req = new FsmCreateTechnicianSkillRequest(Guid.NewGuid(), "Lê Anh Tuấn", "SKILL-ROBOTIC", "Lắp Ráp Robot Công Nghiệp", "Chuyên Gia Cấp Cao", "CERT-ROBOT-01", DateTimeOffset.UtcNow, null);
        var res = await _svc.CreateTechnicianSkillCertAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("Lê Anh Tuấn", res.TechnicianName);
        Assert.True(res.IsActive);
    }

    [Fact]
    public async Task CreateTerritoryCoverage_SavesRegionAndLead()
    {
        var req = new FsmCreateTerritoryCoverageRequest("REGION-CENTRAL", "Khu Vực Đà Nẵng & Miền Trung", "Đà Nẵng", "HUB-DN-01", Guid.NewGuid(), "Phạm Quốc Bảo");
        var res = await _svc.CreateTerritoryCoverageAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("REGION-CENTRAL", res.TerritoryCode);
        Assert.Equal("Đà Nẵng", res.ProvinceOrCity);
    }

    [Fact]
    public async Task GetWarrantyExpiryAlerts_ReturnsUpcomingExpiries()
    {
        var res = await _svc.GetWarrantyExpiryAlertsAsync(_tenant);

        Assert.NotNull(res);
        Assert.NotEmpty(res);
    }

    [Fact]
    public async Task CreateMaintenanceContract_SavesSlaAndVisits()
    {
        var req = new FsmCreatePeriodicMaintenanceContractRequest("CTR-2026-FPT", Guid.NewGuid(), "FPT Software", "Platinum SLA 1h", 12, 180000000m, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1));
        var res = await _svc.CreateMaintenanceContractAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("CTR-2026-FPT", res.ContractNumber);
        Assert.Equal(12, res.VisitsPerYear);
    }
}
