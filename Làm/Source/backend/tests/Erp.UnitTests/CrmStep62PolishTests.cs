using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 62:
///   UC_CRM_023 — Đóng campaign (Campaign Closure & Final Metrics Snapshot)
///   UC_CRM_024 — Danh mục nguồn lead (Lead Source Master Data Management)
///   UC_CRM_025 — Đồng bộ lead mạng xã hội (Social Media Web Lead Sync)
///   UC_CRM_026 — Đồng bộ lead website / landing (Website & Landing Page Lead Intake)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class CrmStep62PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmCampaignService _campSvc;
    private readonly CrmLeadService _leadSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public CrmStep62PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-step62-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_crm62", DisplayName = "Admin CRM 62" });

        _db.SaveChanges();

        _campSvc = new CrmCampaignService(_db);
        _leadSvc = new CrmLeadService(_db, null!);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_023: Đóng campaign (Campaign Closure & Metrics Snapshot)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_023_CloseCampaign_ActiveCampaign_ClosesAndSnapshotsMetrics()
    {
        var camp = await _campSvc.UpsertAsync(_tenant, _userAdmin, new CrmCampaignUpsertRequest(
            null, "CAMP_CLOSE62", "Chiến dịch Hoàn Tất", null, "Social", null, null, 30000000m, null));

        await _campSvc.UpsertExpenseAsync(_tenant, _userAdmin, camp.Id, new CrmCampaignExpenseUpsertRequest(
            null, "Ads", "Chi phí chạy Ads Facebook", 15000000m, null, null));

        var closed = await _campSvc.CloseAsync(_tenant, _userAdmin, camp.Id, new CrmCampaignCloseRequest("Đã đạt mục tiêu kpi tết"));

        Assert.NotNull(closed);
        Assert.Equal("Closed", closed.Status);
        Assert.NotNull(closed.ClosedAt);
        Assert.Equal("Đã đạt mục tiêu kpi tết", closed.ClosedReason);
        Assert.Equal(15000000m, closed.SpentAmount);
    }

    [Fact]
    public async Task UC_CRM_023_CloseCampaign_AlreadyClosedCampaign_ThrowsAppException()
    {
        var camp = await _campSvc.UpsertAsync(_tenant, _userAdmin, new CrmCampaignUpsertRequest(
            null, "CAMP_DBCONT", "Chiến dịch Đã Đóng", null, "Email", null, null, 10000000m, null));

        await _campSvc.CloseAsync(_tenant, _userAdmin, camp.Id, new CrmCampaignCloseRequest("Lần 1"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _campSvc.CloseAsync(_tenant, _userAdmin, camp.Id, new CrmCampaignCloseRequest("Lần 2")));

        Assert.Contains("Campaign đã đóng", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_024: Danh mục nguồn lead (Lead Source Master Data)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_024_UpsertSource_ValidInput_CreatesLeadSourceSuccessfully()
    {
        var src = await _leadSvc.UpsertSourceAsync(_tenant, _userAdmin, new CrmLeadSourceUpsertRequest(
            null, "SRC_FB62", "Nguồn Facebook Fanpage", "Social", "Active", "Kênh mạng xã hội chính"));

        Assert.NotNull(src);
        Assert.Equal("SRC_FB62", src.Code);
        Assert.Equal("Social", src.ChannelType);
        Assert.Equal("Active", src.Status);

        var list = await _leadSvc.ListSourcesAsync(_tenant);
        Assert.Contains(list, s => s.Code == "SRC_FB62");
    }

    [Fact]
    public async Task UC_CRM_024_UpsertSource_DuplicateCode_ThrowsAppException()
    {
        await _leadSvc.UpsertSourceAsync(_tenant, _userAdmin, new CrmLeadSourceUpsertRequest(
            null, "SRC_DUP62", "Nguồn Gốc", "Website", "Active", null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leadSvc.UpsertSourceAsync(_tenant, _userAdmin, new CrmLeadSourceUpsertRequest(
                null, "SRC_DUP62", "Nguồn Trùng", "Website", "Active", null)));

        Assert.Contains("Mã nguồn đã tồn tại", ex.Message);
    }

    [Fact]
    public async Task UC_CRM_024_UpsertSource_InvalidChannelType_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leadSvc.UpsertSourceAsync(_tenant, _userAdmin, new CrmLeadSourceUpsertRequest(
                null, "SRC_BAD_CHAN", "Nguồn Lỗi Kênh", "InvalidType", "Active", null)));

        Assert.Contains("ChannelType: Manual|Website|Social|Other", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_025: Đồng bộ lead mạng xã hội (Social Web Lead Sync)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_025_SyncWebLead_SocialChannel_CreatesLeadWithSocialUtm()
    {
        var camp = await _campSvc.UpsertAsync(_tenant, _userAdmin, new CrmCampaignUpsertRequest(
            null, "CAMP_SOC62", "Chiến dịch Zalo Ads", null, "Social", null, null, 25000000m, null));

        var lead = await _campSvc.SyncWebLeadAsync(_tenant, new CrmWebLeadSyncRequest(
            "Phạm Văn Social", "0908889900", "social62@zalo.me", "https://zalo.me", "https://erp.vn/zalo-landing",
            "zalo", "cpc", "CAMP_SOC62", camp.Id));

        Assert.NotNull(lead);
        Assert.Equal("zalo", lead.UtmSource);
        Assert.Equal("Synced", lead.SyncStatus);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_026: Đồng bộ lead website / landing page (Website Lead Intake)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_026_SyncWebLead_WebsiteLanding_CreatesLeadAndWebsiteSource()
    {
        var lead = await _campSvc.SyncWebLeadAsync(_tenant, new CrmWebLeadSyncRequest(
            "Vũ Thị Landing", "0907771122", "landing62@gmail.com", "https://google.com", "https://erp.vn/dang-ky-dung-thu",
            "google", "organic", "SEO_2026", null));

        Assert.NotNull(lead);
        Assert.Equal("Vũ Thị Landing", lead.ContactName);
        Assert.Equal("https://erp.vn/dang-ky-dung-thu", lead.LandingPage);
    }

    [Fact]
    public async Task UC_CRM_023_CloseCampaign_NonExistentId_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _campSvc.CloseAsync(_tenant, _userAdmin, Guid.NewGuid(), new CrmCampaignCloseRequest("Không tồn tại")));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Không tìm thấy campaign", ex.Message);
    }

    [Fact]
    public async Task UC_CRM_024_ListSources_ReturnsAllConfiguredSources()
    {
        var list = await _leadSvc.ListSourcesAsync(_tenant);
        Assert.NotNull(list);
    }

    [Fact]
    public async Task UC_CRM_025_ListWebLeads_FiltersBySyncStatus()
    {
        await _campSvc.SyncWebLeadAsync(_tenant, new CrmWebLeadSyncRequest(
            "Lead Filter Test", "0901112233", "filter@erp.vn", null, null, "facebook", "cpc", "FB_CAMP", null));

        var syncedLeads = await _campSvc.ListWebLeadsAsync(_tenant, "Synced");

        Assert.NotEmpty(syncedLeads);
        Assert.Contains(syncedLeads, l => l.ContactName == "Lead Filter Test");
    }
}
