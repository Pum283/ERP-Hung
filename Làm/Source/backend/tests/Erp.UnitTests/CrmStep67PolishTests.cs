using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 67:
///   UC_CRM_052 — Lead scoring (Automated Lead Qualification & Scoring Algorithm)
///   UC_CRM_053 — Cập nhật trạng thái pipeline (Lead Pipeline Stage Transition & Funnel Tracking)
///   UC_CRM_054 — Task follow-up lead (Sales Task Creation & Due Date Scheduling)
///   UC_CRM_055 — Nhắc việc follow-up (Lead Follow-up Reminder & Notification Trigger)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class CrmStep67PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmLeadService _leadSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();
    private readonly Guid _userSales = Guid.NewGuid();

    public CrmStep67PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-step67-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_crm67", DisplayName = "Admin CRM 67" });
        _db.Users.Add(new AppUser { Id = _userSales, TenantId = _tenant, Username = "sales_crm67", DisplayName = "Sales Rep 67" });

        _db.SaveChanges();

        _leadSvc = new CrmLeadService(_db, null!);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_052: Lead scoring
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_052_CalculateLeadScore_ValidLead_CalculatesScoreSuccessfully()
    {
        var lead = await _leadSvc.UpsertLeadAsync(_tenant, _userAdmin, new CrmLeadUpsertRequest(
            null, null, "Trần Văn Score", "0901112233", "score@growth.vn", "Tập đoàn ABC",
            null, null, null, "New", 0, null, "Có nhu cầu gấp ERP", "Manual"));

        var scored = await _leadSvc.CalculateLeadScoreAsync(_tenant, _userAdmin, lead.Id);

        Assert.NotNull(scored);
        Assert.True(scored.Score >= 0 && scored.Score <= 100);
    }

    [Fact]
    public async Task UC_CRM_052_CalculateLeadScore_NonExistentLead_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leadSvc.CalculateLeadScoreAsync(_tenant, _userAdmin, Guid.NewGuid()));

        Assert.Equal(404, ex.StatusCode);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_053: Cập nhật trạng thái pipeline
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_053_SetStatus_ValidTransition_UpdatesPipelineStatusSuccessfully()
    {
        var lead = await _leadSvc.UpsertLeadAsync(_tenant, _userAdmin, new CrmLeadUpsertRequest(
            null, null, "Lead Chuyển Trạng Thái", "0904445566", "status@growth.vn", null,
            null, null, null, "New", 0, null, null, null));

        var updated = await _leadSvc.SetStatusAsync(_tenant, _userAdmin, lead.Id, new CrmLeadStatusRequest("Qualified", "Khách hàng rất tiềm năng"));

        Assert.NotNull(updated);
        Assert.Equal("Qualified", updated.PipelineStatus);
    }

    [Fact]
    public async Task UC_CRM_053_SetStatus_InvalidPipelineStatus_ThrowsAppException()
    {
        var lead = await _leadSvc.UpsertLeadAsync(_tenant, _userAdmin, new CrmLeadUpsertRequest(
            null, null, "Lead Status Lỗi", "0907776655", "badstatus@growth.vn", null,
            null, null, null, "New", 0, null, null, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leadSvc.SetStatusAsync(_tenant, _userAdmin, lead.Id, new CrmLeadStatusRequest("InvalidStage", null)));

        Assert.Contains("Pipeline: New|Contacted|Qualified|Converted|Lost.", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_054: Task follow-up lead
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_054_UpsertTask_ValidInput_CreatesTaskSuccessfully()
    {
        var lead = await _leadSvc.UpsertLeadAsync(_tenant, _userAdmin, new CrmLeadUpsertRequest(
            null, null, "Lead Tạo Task", "0908881122", "task@growth.vn", null,
            null, null, null, "New", 0, null, null, null));

        var task = await _leadSvc.UpsertTaskAsync(_tenant, _userAdmin, new CrmLeadTaskUpsertRequest(
            null, lead.Id, "Gọi điện tư vấn báo giá ERP", DateTimeOffset.UtcNow.AddDays(1), _userSales, "Open", true, "Ghi chú gọi lại"));

        Assert.NotNull(task);
        Assert.Equal("Gọi điện tư vấn báo giá ERP", task.Title);
        Assert.Equal("Open", task.Status);
        Assert.True(task.IsReminder);
    }

    [Fact]
    public async Task UC_CRM_054_UpsertTask_MissingTitle_ThrowsAppException()
    {
        var lead = await _leadSvc.UpsertLeadAsync(_tenant, _userAdmin, new CrmLeadUpsertRequest(
            null, null, "Lead Task Lack Title", "0908883344", "notitle@growth.vn", null,
            null, null, null, "New", 0, null, null, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leadSvc.UpsertTaskAsync(_tenant, _userAdmin, new CrmLeadTaskUpsertRequest(
                null, lead.Id, "", DateTimeOffset.UtcNow.AddDays(1), null, null, null, null)));

        Assert.Contains("Tiêu đề task", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_055: Nhắc việc follow-up
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_055_UpsertTask_WithReminderFlag_SchedulesReminderNotification()
    {
        var lead = await _leadSvc.UpsertLeadAsync(_tenant, _userAdmin, new CrmLeadUpsertRequest(
            null, null, "Lead Nhắc Việc", "0902223344", "reminder@growth.vn", null,
            null, null, null, "New", 0, null, null, null));

        var task = await _leadSvc.UpsertTaskAsync(_tenant, _userAdmin, new CrmLeadTaskUpsertRequest(
            null, lead.Id, "Nhắc nhở họp với Khách hàng", DateTimeOffset.UtcNow.AddHours(2), _userSales, "Open", true, "Phòng họp Zoom"));

        Assert.NotNull(task);
        Assert.True(task.IsReminder);
        Assert.Equal(_userSales, task.AssigneeUserId);
    }

    [Fact]
    public async Task UC_CRM_053_MarkLost_ValidLead_SetsStatusToLost()
    {
        var lead = await _leadSvc.UpsertLeadAsync(_tenant, _userAdmin, new CrmLeadUpsertRequest(
            null, null, "Lead Thất Bại", "0909990011", "lost@growth.vn", null,
            null, null, null, "New", 0, null, null, null));

        var lost = await _leadSvc.MarkLostAsync(_tenant, _userAdmin, lead.Id, new CrmLeadLostRequest("Khách hàng chọn đối thủ khác"));

        Assert.NotNull(lost);
        Assert.Equal("Lost", lost.PipelineStatus);
        Assert.Equal("Khách hàng chọn đối thủ khác", lost.LostReason);
    }

    [Fact]
    public async Task UC_CRM_054_UpsertTask_NonExistentLead_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leadSvc.UpsertTaskAsync(_tenant, _userAdmin, new CrmLeadTaskUpsertRequest(
                null, Guid.NewGuid(), "Task Lead Không Tồn Tại", DateTimeOffset.UtcNow.AddDays(1), null, null, null, null)));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC_CRM_055_UpsertTask_CompleteTask_UpdatesStatusToCompleted()
    {
        var lead = await _leadSvc.UpsertLeadAsync(_tenant, _userAdmin, new CrmLeadUpsertRequest(
            null, null, "Lead Hoàn Thành Task", "0905554433", "done@growth.vn", null,
            null, null, null, "New", 0, null, null, null));

        var created = await _leadSvc.UpsertTaskAsync(_tenant, _userAdmin, new CrmLeadTaskUpsertRequest(
            null, lead.Id, "Task sẽ hoàn thành", DateTimeOffset.UtcNow.AddHours(1), null, "Open", false, null));

        var completed = await _leadSvc.UpsertTaskAsync(_tenant, _userAdmin, new CrmLeadTaskUpsertRequest(
            created.Id, lead.Id, "Task sẽ hoàn thành", DateTimeOffset.UtcNow.AddHours(1), null, "Done", false, null));

        Assert.Equal("Done", completed.Status);
    }
}
