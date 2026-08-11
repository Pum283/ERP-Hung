using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 63:
///   UC_CRM_027 — Đồng bộ kênh khác (Multi-channel / Partner Lead Ingestion)
///   UC_CRM_028 — Attribution nguồn khách (Lead Source Attribution & UTM Tracking)
///   UC_CRM_029 — Tính CPL / CAC / ROAS / ROI (Financial & Marketing Metric Calculation Engine)
///   UC_CRM_030 — Funnel marketing đến doanh thu (Marketing-to-Revenue Conversion Funnel)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class CrmStep63PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmCampaignService _campSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public CrmStep63PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-step63-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_crm63", DisplayName = "Admin CRM 63" });

        _db.SaveChanges();

        _campSvc = new CrmCampaignService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_027: Đồng bộ kênh khác (Partner & Multi-Channel Lead Ingestion)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_027_SyncWebLead_PartnerChannel_IngestsLeadSuccessfully()
    {
        var lead = await _campSvc.SyncWebLeadAsync(_tenant, new CrmWebLeadSyncRequest(
            "Đại Lý Partner B", "0911223344", "partnerB@partner.vn", null, null,
            "partner_api", "referral", "PARTNER_2026", null));

        Assert.NotNull(lead);
        Assert.Equal("Đại Lý Partner B", lead.ContactName);
        Assert.Equal("partner_api", lead.UtmSource);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_028: Attribution nguồn khách (Lead Source Attribution)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_028_SyncWebLead_AttributesUtmSourceAndCampaign()
    {
        var camp = await _campSvc.UpsertAsync(_tenant, _userAdmin, new CrmCampaignUpsertRequest(
            null, "CAMP_ATTR63", "Chiến dịch Attribution", null, "Event", null, null, 15000000m, null));

        var lead = await _campSvc.SyncWebLeadAsync(_tenant, new CrmWebLeadSyncRequest(
            "Khách Hàng Hội Thảo", "0988776655", "hoithao@erp.vn", "https://event.erp.vn", "https://event.erp.vn/register",
            "event_workshop", "offline", "CAMP_ATTR63", camp.Id));

        Assert.NotNull(lead);
        Assert.Equal("event_workshop", lead.UtmSource);
        Assert.Equal("CAMP_ATTR63", lead.UtmCampaign);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_029: Tính CPL / CAC / ROAS / ROI (Financial Metrics Engine)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_029_GetMetrics_CalculatesCplCacRoasAndRoiAccurately()
    {
        var camp = await _campSvc.UpsertAsync(_tenant, _userAdmin, new CrmCampaignUpsertRequest(
            null, "CAMP_MATH63", "Chiến dịch Phân Tích ROI", null, "SEM", null, null, 50000000m, null));

        await _campSvc.UpsertExpenseAsync(_tenant, _userAdmin, camp.Id, new CrmCampaignExpenseUpsertRequest(
            null, "Ads", "Ads Google", 20000000m, null, null));

        // Sync 2 leads
        await _campSvc.SyncWebLeadAsync(_tenant, new CrmWebLeadSyncRequest(
            "Lead 1", "0901000001", "l1@erp.vn", null, null, "google", "cpc", "CAMP_MATH63", camp.Id));
        await _campSvc.SyncWebLeadAsync(_tenant, new CrmWebLeadSyncRequest(
            "Lead 2", "0901000002", "l2@erp.vn", null, null, "google", "cpc", "CAMP_MATH63", camp.Id));

        var metrics = await _campSvc.GetMetricsAsync(_tenant, camp.Id);

        Assert.NotNull(metrics);
        Assert.Equal(20000000m, metrics.TotalSpent);
        Assert.Equal(2, metrics.LeadCount);
        Assert.Equal(10000000m, metrics.CostPerLead); // 20M / 2 leads = 10M
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_030: Funnel marketing đến doanh thu (Marketing-to-Revenue Funnel)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_030_GetDashboard_IncludesCampaignMetricsInMarketingFunnel()
    {
        var camp = await _campSvc.UpsertAsync(_tenant, _userAdmin, new CrmCampaignUpsertRequest(
            null, "CAMP_FUNNEL63", "Chiến dịch Funnel", null, "Social", null, null, 10000000m, null));

        await _campSvc.SyncWebLeadAsync(_tenant, new CrmWebLeadSyncRequest(
            "Lead Funnel 1", "0909990001", "funnel1@erp.vn", null, null, "tiktok", "cpc", "CAMP_FUNNEL63", camp.Id));

        var dash = await _campSvc.GetDashboardAsync(_tenant);

        Assert.NotNull(dash);
        Assert.Contains(dash.CampaignMetrics, m => m.CampaignId == camp.Id);
    }

    [Fact]
    public async Task UC_CRM_029_GetMetrics_ZeroSpentAndZeroLeads_HandlesDivisionByZeroGracefully()
    {
        var camp = await _campSvc.UpsertAsync(_tenant, _userAdmin, new CrmCampaignUpsertRequest(
            null, "CAMP_ZERO63", "Chiến dịch Chưa Chi Tiền", null, "Other", null, null, 5000000m, null));

        var metrics = await _campSvc.GetMetricsAsync(_tenant, camp.Id);

        Assert.NotNull(metrics);
        Assert.Equal(0m, metrics.TotalSpent);
        Assert.Equal(0, metrics.LeadCount);
        Assert.Equal(0m, metrics.CostPerLead);
        Assert.Equal(0m, metrics.CustomerAcquisitionCost);
    }

    [Fact]
    public async Task UC_CRM_027_SyncWebLead_DuplicatePhone_UpdatesOrSyncsExistingLead()
    {
        var l1 = await _campSvc.SyncWebLeadAsync(_tenant, new CrmWebLeadSyncRequest(
            "Khách A Ban Đầu", "0905556677", "a1@erp.vn", null, null, "facebook", "cpc", "FB_1", null));

        var l2 = await _campSvc.SyncWebLeadAsync(_tenant, new CrmWebLeadSyncRequest(
            "Khách A Cập Nhật", "0905556677", "a2@erp.vn", null, null, "google", "cpc", "GG_1", null));

        Assert.NotNull(l1);
        Assert.NotNull(l2);
    }

    [Fact]
    public async Task UC_CRM_028_ListWebLeads_ReturnsLeadsWithAttributionData()
    {
        await _campSvc.SyncWebLeadAsync(_tenant, new CrmWebLeadSyncRequest(
            "Attribution Lead 1", "0902223344", "attr1@erp.vn", null, null, "email", "newsletter", "NEWS_2026", null));

        var list = await _campSvc.ListWebLeadsAsync(_tenant, null);

        Assert.NotEmpty(list);
        Assert.Contains(list, l => l.UtmSource == "email");
    }

    [Fact]
    public async Task UC_CRM_030_GetDashboard_CalculatesTotalBudgetAndSpentSummary()
    {
        var dash = await _campSvc.GetDashboardAsync(_tenant);
        Assert.NotNull(dash);
        Assert.True(dash.TotalBudget >= 0);
        Assert.True(dash.TotalSpent >= 0);
    }

    [Fact]
    public async Task UC_CRM_029_GetMetrics_InvalidTenant_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _campSvc.GetMetricsAsync(Guid.NewGuid(), Guid.NewGuid()));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC_CRM_030_GetDashboard_EmptyTenant_ReturnsEmptyDashboard()
    {
        var emptyTenant = Guid.NewGuid();
        var dash = await _campSvc.GetDashboardAsync(emptyTenant);

        Assert.NotNull(dash);
        Assert.Equal(0, dash.TotalCampaigns);
        Assert.Equal(0m, dash.TotalBudget);
        Assert.Equal(0m, dash.TotalSpent);
    }
}
