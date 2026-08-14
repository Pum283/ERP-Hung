using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class PjmTimesheetBudgetChecklistPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PjmTimesheetBudgetChecklistService _svc;
    private readonly Guid _tenant = Guid.NewGuid();

    public PjmTimesheetBudgetChecklistPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pjm-timesheet-budget-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new PjmTimesheetBudgetChecklistService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CreateTimesheetEntry_SavesHoursWorked()
    {
        var req = new PjmCreateTimesheetRequest(Guid.NewGuid(), "PRJ-088", Guid.NewGuid(), "Kỹ Sư An", "Đấu nối cáp điện", 8.0m, 1.5m, DateTimeOffset.UtcNow);
        var res = await _svc.CreateTimesheetEntryAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(8.0m, res.HoursSpent);
        Assert.Equal(1.5m, res.OvertimeHours);
    }

    [Fact]
    public async Task GetBudgetOverrunWarnings_ReturnsVarianceMetrics()
    {
        var res = await _svc.GetBudgetOverrunWarningsAsync(_tenant);

        Assert.NotNull(res);
        Assert.NotEmpty(res);
        Assert.True(res[0].OverrunAmountVnd > 0);
    }

    [Fact]
    public async Task CreateSurveyChecklist_SavesInspectionReport()
    {
        var req = new PjmCreateSurveyChecklistRequest(Guid.NewGuid(), "PRJ-088", "Kiểm tra tải trọng sàn", "1500 kg/m2", true, "Đạt yêu cầu");
        var res = await _svc.CreateSurveyChecklistAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.True(res.IsSatisfied);
        Assert.Equal("PRJ-088", res.ProjectCode);
    }

    [Fact]
    public async Task CreateInstallationChecklist_SavesTechnicalSigner()
    {
        var req = new PjmCreateInstallationChecklistRequest(Guid.NewGuid(), "PRJ-088", "Siết bu lông chân máy", "TRANS-2000KVA", true, "KS. Toản");
        var res = await _svc.CreateInstallationChecklistAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.True(res.IsCompleted);
        Assert.Equal("KS. Toản", res.TechnicianSigner);
    }
}
