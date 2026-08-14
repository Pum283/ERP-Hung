using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class FsmEquipmentMaintenancePolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly FsmEquipmentMaintenanceService _svc;
    private readonly Guid _tenant = Guid.NewGuid();

    public FsmEquipmentMaintenancePolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("fsm-equip-maint-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new FsmEquipmentMaintenanceService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CreateMaintenanceSchedule_SavesNextDueDate()
    {
        var req = new FsmCreateMaintenanceScheduleRequest(Guid.NewGuid(), "SN-TEST-888", "Máy Nén Khí Trục Vít", "Tập Đoàn Hoa Sen", "Quarterly", DateTimeOffset.UtcNow.AddMonths(3), true);
        var res = await _svc.CreateMaintenanceScheduleAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("SN-TEST-888", res.SerialNumber);
        Assert.True(res.AutoGenerateTicket);
    }

    [Fact]
    public async Task GenerateDueTicket_CreatesDispatchedTicket()
    {
        var res = await _svc.GenerateDueTicketAsync(_tenant, Guid.NewGuid());

        Assert.NotNull(res);
        Assert.StartsWith("TCK-MAINT-", res.GeneratedTicketNumber);
        Assert.Equal("Dispatched", res.Status);
    }

    [Fact]
    public async Task CreateStandardChecklist_SavesSopItem()
    {
        var req = new FsmCreateStandardChecklistItemRequest("CNC", "Kiểm tra mức nhớt trục chính", "Thăm nhớt và châm thêm nếu dưới vạch Min", 1, true);
        var res = await _svc.CreateStandardChecklistAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("CNC", res.EquipmentCategory);
        Assert.True(res.IsMandatory);
    }

    [Fact]
    public async Task GetMaintenanceExecutionReport_ReturnsSummaryMetrics()
    {
        var res = await _svc.GetMaintenanceExecutionReportAsync(_tenant);

        Assert.NotNull(res);
        Assert.True(res.TotalScheduledVisits > 0);
        Assert.True(res.OnTimeCompletionRatePct > 90);
    }
}
