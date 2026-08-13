using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Domain.Entities.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class CrmCommissionSyncLeaderboardPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmCommissionSyncLeaderboardService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _periodId = Guid.NewGuid();

    public CrmCommissionSyncLeaderboardPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-commission-sync-leaderboard-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "TCRM177", Name = "Tenant CRM 177" });
        _db.CrmCommissionPeriods.Add(new CrmCommissionPeriod
        {
            Id = _periodId,
            TenantId = _tenant,
            PeriodCode = "COMM-2026-M08",
            PeriodName = "Bảng Hoa Hồng Tháng 08/2026",
            StartDate = new DateTime(2026, 8, 1),
            EndDate = new DateTime(2026, 8, 31),
            TotalCommissionAmount = 52300000m,
            Status = "Calculated"
        });

        _db.CrmSalesLeaderboardEntries.Add(new CrmSalesLeaderboardEntry
        {
            Id = Guid.NewGuid(),
            TenantId = _tenant,
            SalesUserId = Guid.NewGuid(),
            SalesUserName = "Nguyễn Văn FieldSales 1",
            RankPosition = 1,
            TotalRevenue = 450000000m,
            TotalNewCustomers = 12,
            TotalCommissionEarned = 18000000m,
            RankingPeriod = "Monthly"
        });

        _db.SaveChanges();

        _svc = new CrmCommissionSyncLeaderboardService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_121: Tính hoa hồng theo kỳ
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CalculateCommissionPeriod_CalculatesPeriodSuccessfully()
    {
        var req = new CrmCalculateCommissionRequest(
            "COMM-2026-M09",
            "Bảng Hoa Hồng Tháng 09/2026",
            new DateTime(2026, 9, 1),
            new DateTime(2026, 9, 30)
        );

        var res = await _svc.CalculateCommissionPeriodAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("COMM-2026-M09", res.PeriodCode);
        Assert.Equal("Calculated", res.Status);
        Assert.True(res.TotalCommissionAmount > 0);

        var periods = await _svc.GetCommissionPeriodsAsync(_tenant);
        Assert.NotEmpty(periods);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_122: Duyệt bảng hoa hồng
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ApproveCommissionPeriod_ApprovesPeriodSuccessfully()
    {
        var approverId = Guid.NewGuid();
        var req = new CrmApproveCommissionRequest(
            _periodId,
            approverId,
            "Đã kiểm tra doanh số và phê duyệt chi trả hoa hồng"
        );

        var res = await _svc.ApproveCommissionPeriodAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(_periodId, res.PeriodId);
        Assert.Equal("Approved", res.Status);
        Assert.Equal(approverId, res.ApproverUserId);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_123: Đồng bộ hoa hồng sang HRM/FIN
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SyncCommissionToHrmFin_SyncsDataSuccessfully()
    {
        var req = new CrmSyncCommissionHrmFinRequest(
            _periodId,
            true,
            true
        );

        var res = await _svc.SyncCommissionToHrmFinAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(_periodId, res.PeriodId);
        Assert.Equal("SyncedToHrmFin", res.Status);
        Assert.True(res.SyncedToHrmPayroll);
        Assert.True(res.SyncedToFinAccounting);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_125: Bảng xếp hạng sales
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetSalesLeaderboard_ReturnsRankedEntries()
    {
        var leaderboard = await _svc.GetSalesLeaderboardAsync(_tenant, "Monthly");

        Assert.NotEmpty(leaderboard);
        Assert.Equal(1, leaderboard[0].RankPosition);
        Assert.True(leaderboard[0].TotalRevenue > 0);
    }
}
