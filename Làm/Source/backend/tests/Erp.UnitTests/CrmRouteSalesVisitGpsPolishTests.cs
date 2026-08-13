using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Domain.Entities.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class CrmRouteSalesVisitGpsPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmRouteSalesVisitGpsService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _salespersonId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _territoryId = Guid.NewGuid();
    private readonly Guid _visitPlanId = Guid.NewGuid();

    public CrmRouteSalesVisitGpsPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-route-visit-gps-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "TCRM171", Name = "Tenant CRM 171" });
        _db.CrmCustomers.Add(new CrmCustomer
        {
            Id = _customerId,
            TenantId = _tenant,
            Code = "CUST-171",
            DisplayName = "Đại lý Bách Hóa An Khánh",
            Phone = "0908111222"
        });

        _db.CrmSalesTerritories.Add(new CrmSalesTerritory
        {
            Id = _territoryId,
            TenantId = _tenant,
            TerritoryCode = "T-HCM-Q1",
            TerritoryName = "Tuyến Quận 1 - TP.HCM",
            Region = "Miền Nam",
            VisitFrequency = "Weekly",
            AssignedSalespersonId = _salespersonId,
            IsActive = true
        });

        _db.CrmVisitPlans.Add(new CrmVisitPlan
        {
            Id = _visitPlanId,
            TenantId = _tenant,
            TerritoryId = _territoryId,
            CustomerId = _customerId,
            SalespersonId = _salespersonId,
            PlannedDate = DateTime.UtcNow,
            Status = "Planned"
        });

        _db.SaveChanges();

        _svc = new CrmRouteSalesVisitGpsService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_089: Phân vùng / tuyến bán hàng
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateTerritory_CreatesAndReturnsTerritoryDto()
    {
        var req = new CrmCreateTerritoryRequest(
            "T-HN-CG",
            "Tuyến Cầu Giấy - Hà Nội",
            "Miền Bắc",
            "BiWeekly",
            _salespersonId
        );

        var res = await _svc.CreateTerritoryAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("T-HN-CG", res.TerritoryCode);
        Assert.Equal("Tuyến Cầu Giấy - Hà Nội", res.TerritoryName);

        var list = await _svc.GetTerritoriesAsync(_tenant);
        Assert.NotEmpty(list);
        Assert.Contains(list, t => t.TerritoryCode == "T-HN-CG");
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_090: Phân loại tần suất visit
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ClassifyFrequency_UpdatesTerritoryFrequency()
    {
        var req = new CrmClassifyFrequencyRequest(
            _territoryId,
            "BiWeekly"
        );

        var res = await _svc.ClassifyFrequencyAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(_territoryId, res.TerritoryId);
        Assert.Equal("BiWeekly", res.VisitFrequency);

        var dbTerr = await _db.CrmSalesTerritories.FirstOrDefaultAsync(t => t.TenantId == _tenant && t.Id == _territoryId);
        Assert.NotNull(dbTerr);
        Assert.Equal("BiWeekly", dbTerr.VisitFrequency);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_091: Lập kế hoạch visit
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateVisitPlan_SchedulesVisitPlanSuccessfully()
    {
        var req = new CrmCreateVisitPlanRequest(
            _territoryId,
            _customerId,
            _salespersonId,
            DateTime.UtcNow.AddDays(2),
            "Kiểm tra tồn kho đại lý & chốt đơn đợt 2"
        );

        var res = await _svc.CreateVisitPlanAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(_territoryId, res.TerritoryId);
        Assert.Equal(_customerId, res.CustomerId);
        Assert.Equal("Planned", res.Status);

        var list = await _svc.GetVisitPlansAsync(_tenant);
        Assert.NotEmpty(list);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_092: Check-in / check-out GPS
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CheckInAndCheckOutGps_UpdatesGpsCoordinatesAndTime()
    {
        var checkInReq = new CrmGpsCheckInRequest(_visitPlanId, "10.7769,106.7009");
        var inRes = await _svc.CheckInGpsAsync(_tenant, checkInReq);

        Assert.NotNull(inRes);
        Assert.Equal("InProgress", inRes.Status);
        Assert.Equal("10.7769,106.7009", inRes.CheckInGps);
        Assert.NotNull(inRes.CheckInTime);

        var checkOutReq = new CrmGpsCheckOutRequest(_visitPlanId, "10.7772,106.7012");
        var outRes = await _svc.CheckOutGpsAsync(_tenant, checkOutReq);

        Assert.NotNull(outRes);
        Assert.Equal("Completed", outRes.Status);
        Assert.Equal("10.7772,106.7012", outRes.CheckOutGps);
        Assert.NotNull(outRes.CheckOutTime);
    }
}
