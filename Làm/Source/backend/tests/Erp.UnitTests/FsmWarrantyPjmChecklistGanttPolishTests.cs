using Erp.Application.DTOs;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class FsmWarrantyPjmChecklistGanttPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly FsmWarrantyClaimReportService _fsmSvc;
    private readonly PjmChecklistGanttPlanChangeService _pjmSvc;
    private readonly Guid _tenant = Guid.NewGuid();

    public FsmWarrantyPjmChecklistGanttPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("fsm-pjm-step210-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _fsmSvc = new FsmWarrantyClaimReportService(_db);
        _pjmSvc = new PjmChecklistGanttPlanChangeService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task GetWarrantyClaimReport_ReturnsApprovalRate()
    {
        var res = await _fsmSvc.GetWarrantyClaimReportAsync(_tenant);

        Assert.NotNull(res);
        Assert.True(res.TotalClaimsCount > 0);
        Assert.True(res.ClaimApprovalRatePct > 80);
    }

    [Fact]
    public async Task CreateAcceptanceTemplate_SavesPjmTemplate()
    {
        var req = new PjmCreateAcceptanceTemplateRequest("TMPL-M&E", "Nghiệm thu trạm biến áp", "Cơ Điện", "Đo điện áp định mức 3 pha", 1, true);
        var res = await _pjmSvc.CreateAcceptanceTemplateAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("TMPL-M&E", res.TemplateCode);
        Assert.True(res.IsMandatory);
    }

    [Fact]
    public async Task CreateMilestone_SavesGanttProgress()
    {
        var req = new PjmCreateMilestoneRequest(Guid.NewGuid(), "MS-10", "Kéo rải cáp quang ngầm", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7), 50.0, "", "InProgress");
        var res = await _pjmSvc.CreateMilestoneAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("MS-10", res.MilestoneCode);
        Assert.Equal(50.0, res.CompletionProgressPct);
    }

    [Fact]
    public async Task LogPlanChange_RecordsAuditLog()
    {
        var req = new PjmLogPlanChangeRequest(Guid.NewGuid(), "PRJ-999", "Thay đổi tiến độ giai đoạn 2", "Chờ vật tư từ nhà sản xuất", "PM Hùng");
        var res = await _pjmSvc.LogPlanChangeAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("Approved", res.ApprovalStatus);
        Assert.Equal("PRJ-999", res.ProjectCode);
    }
}
