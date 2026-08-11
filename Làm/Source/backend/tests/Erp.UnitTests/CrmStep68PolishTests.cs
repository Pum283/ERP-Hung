using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 68:
///   UC_CRM_056 — Nhật ký chăm sóc lead (Interaction Activity Logging & Audit Trail)
///   UC_CRM_057 — Chuyển lead thành cơ hội (Lead-to-Opportunity Conversion & Pipeline Handover)
///   UC_CRM_058 — Gộp lead trùng (Lead Deduplication & Entity Merging)
///   UC_CRM_059 — Báo cáo chuyển đổi lead (Lead Conversion Rate & Funnel Analytics)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class CrmStep68PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmLeadService _leadSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public CrmStep68PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-step68-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_crm68", DisplayName = "Admin CRM 68" });

        _db.SaveChanges();

        _leadSvc = new CrmLeadService(_db, null!);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_056: Nhật ký chăm sóc lead
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_056_AddActivity_ValidInput_LogsActivitySuccessfully()
    {
        var lead = await _leadSvc.UpsertLeadAsync(_tenant, _userAdmin, new CrmLeadUpsertRequest(
            null, null, "Lead Nhật Ký", "0901119988", "log@growth.vn", null,
            null, null, null, "New", 0, null, null, null));

        var act = await _leadSvc.AddActivityAsync(_tenant, _userAdmin, new CrmLeadActivityUpsertRequest(
            lead.Id, "Call", "Đã gọi điện tư vấn nhu cầu triển khai ERP Cloud", null));

        Assert.NotNull(act);
        Assert.Equal("Call", act.ActivityType);
        Assert.Equal("Đã gọi điện tư vấn nhu cầu triển khai ERP Cloud", act.Content);
        Assert.Equal(_userAdmin, act.CreatedByUserId);
    }

    [Fact]
    public async Task UC_CRM_056_AddActivity_MissingContent_ThrowsAppException()
    {
        var lead = await _leadSvc.UpsertLeadAsync(_tenant, _userAdmin, new CrmLeadUpsertRequest(
            null, null, "Lead Lack Content", "0901112244", "nocontent@growth.vn", null,
            null, null, null, "New", 0, null, null, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leadSvc.AddActivityAsync(_tenant, _userAdmin, new CrmLeadActivityUpsertRequest(
                lead.Id, "Email", "", null)));

        Assert.Contains("Nội dung", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_057: Chuyển lead thành cơ hội
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_057_ConvertToOpportunity_ValidLead_ConvertsToOpportunitySuccessfully()
    {
        var lead = await _leadSvc.UpsertLeadAsync(_tenant, _userAdmin, new CrmLeadUpsertRequest(
            null, null, "Lead Chuyển Cơ Hội", "0903334455", "convert@growth.vn", "Công ty CP Tiềm Năng",
            null, null, null, "Qualified", 80, null, null, null));

        var opp = await _leadSvc.ConvertToOpportunityAsync(_tenant, _userAdmin, lead.Id);

        Assert.NotNull(opp);
        Assert.Equal(lead.Id, opp.LeadId);
        Assert.Equal("Qualification", opp.Stage);

        var leadDetail = await _leadSvc.GetLeadDetailAsync(_tenant, lead.Id);
        Assert.Equal("Converted", leadDetail.Lead.PipelineStatus);
    }

    [Fact]
    public async Task UC_CRM_057_ConvertToOpportunity_AlreadyConverted_ThrowsAppException()
    {
        var lead = await _leadSvc.UpsertLeadAsync(_tenant, _userAdmin, new CrmLeadUpsertRequest(
            null, null, "Lead Đã Chuyển", "0905556677", "already@growth.vn", null,
            null, null, null, "Qualified", 80, null, null, null));

        await _leadSvc.ConvertToOpportunityAsync(_tenant, _userAdmin, lead.Id);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leadSvc.ConvertToOpportunityAsync(_tenant, _userAdmin, lead.Id));

        Assert.Contains("Lead đã có cơ hội", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_058: Gộp lead trùng
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_058_MergeLeads_ValidTargetAndSource_MergesLeadsSuccessfully()
    {
        var targetLead = await _leadSvc.UpsertLeadAsync(_tenant, _userAdmin, new CrmLeadUpsertRequest(
            null, null, "Target Lead Gộp", "0908889900", "target@growth.vn", "Công ty Gộp",
            null, null, null, "New", 30, null, null, null));

        var sourceLead = await _leadSvc.UpsertLeadAsync(_tenant, _userAdmin, new CrmLeadUpsertRequest(
            null, null, "Source Lead Trùng", "0908889900", "source@growth.vn", "Công ty Gộp",
            null, null, null, "New", 20, null, null, null));

        var merged = await _leadSvc.MergeLeadsAsync(_tenant, _userAdmin, new CrmLeadMergeRequest(
            targetLead.Id, sourceLead.Id, "Trùng thông tin SĐT"));

        Assert.NotNull(merged);
        Assert.Equal(targetLead.Id, merged.Id);

        // Target lead sẽ cộng dồn score
        Assert.True(merged.Score >= 30);
    }

    [Fact]
    public async Task UC_CRM_058_MergeLeads_SameLeadId_ThrowsAppException()
    {
        var lead = await _leadSvc.UpsertLeadAsync(_tenant, _userAdmin, new CrmLeadUpsertRequest(
            null, null, "Lead Gộp Chính Nó", "0907771122", "same@growth.vn", null,
            null, null, null, "New", 10, null, null, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leadSvc.MergeLeadsAsync(_tenant, _userAdmin, new CrmLeadMergeRequest(lead.Id, lead.Id, "Cùng Lead")));

        Assert.Contains("Chỉ định 2 Lead khác nhau để gộp", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_059: Báo cáo chuyển đổi lead
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_059_GetConversionReport_ReturnsConversionAnalytics()
    {
        await _leadSvc.UpsertLeadAsync(_tenant, _userAdmin, new CrmLeadUpsertRequest(
            null, null, "Lead Báo Cáo 1", "0901230001", "rpt1@growth.vn", null,
            null, null, null, "New", 0, null, null, null));

        var rpt = await _leadSvc.GetConversionReportAsync(_tenant);

        Assert.NotNull(rpt);
        Assert.True(rpt.TotalLeads >= 1);
    }

    [Fact]
    public async Task UC_CRM_056_AddActivity_NonExistentLead_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leadSvc.AddActivityAsync(_tenant, _userAdmin, new CrmLeadActivityUpsertRequest(
                Guid.NewGuid(), "Note", "Ghi chú lead không tồn tại", null)));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC_CRM_057_ConvertToOpportunity_NonExistentLead_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leadSvc.ConvertToOpportunityAsync(_tenant, _userAdmin, Guid.NewGuid()));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC_CRM_058_MergeLeads_NonExistentSource_ThrowsAppException()
    {
        var targetLead = await _leadSvc.UpsertLeadAsync(_tenant, _userAdmin, new CrmLeadUpsertRequest(
            null, null, "Target Exists", "0906667788", "targetexists@growth.vn", null,
            null, null, null, "New", 0, null, null, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leadSvc.MergeLeadsAsync(_tenant, _userAdmin, new CrmLeadMergeRequest(
                targetLead.Id, Guid.NewGuid(), null)));

        Assert.Equal(404, ex.StatusCode);
    }
}
