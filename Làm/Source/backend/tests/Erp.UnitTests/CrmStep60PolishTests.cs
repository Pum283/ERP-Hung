using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 60:
///   UC_CRM_014 — Import / export khách hàng (CSV Export & Batch Import)
///   UC_CRM_015 — Tìm kiếm khách đa tiêu chí (Multi-criteria Search & Filtering)
///   UC_CRM_016 — Tạo campaign marketing (Marketing Campaign Management)
///   UC_CRM_017 — Quản lý nhóm quảng cáo (Ad Groups & Web Leads Management)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class CrmStep60PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmCustomerService _custSvc;
    private readonly CrmCampaignService _campSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public CrmStep60PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-step60-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_crm60", DisplayName = "Admin CRM 60" });

        _db.SaveChanges();

        _custSvc = new CrmCustomerService(_db);
        _campSvc = new CrmCampaignService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_014: Import / export khách hàng (CSV Export & Batch Import)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_014_ExportCsv_GeneratesValidCsvHeaderAndRows()
    {
        await _custSvc.UpsertAsync(_tenant, _userAdmin,
            new CrmCustomerUpsertRequest(null, "CUST_EXP60", "Person", "Khách Xuất CSV", null, "0901234567", "exp60@erp.vn",
                null, "Prospect", null, "Địa chỉ 60", null, null, "Active"));

        var csv = await _custSvc.ExportCsvAsync(_tenant);

        Assert.NotNull(csv);
        Assert.Contains("Code,CustomerType,DisplayName", csv);
        Assert.Contains("CUST_EXP60", csv);
    }

    [Fact]
    public async Task UC_CRM_014_ImportCsv_ValidContent_ImportsCustomersSuccessfully()
    {
        var csvContent = "Code,CustomerType,DisplayName,CompanyName,Phone,Email,TaxCode,Segment,Status,Address\n" +
                         "CUST_IMP60_1,Person,Khách Import 1,,0909999001,imp1@erp.vn,,Lead,Active,Địa chỉ 1\n" +
                         "CUST_IMP60_2,Organization,Công ty Import 2,Công ty Import 2,02839999002,imp2@erp.vn,039999002,Customer,Active,Địa chỉ 2";

        var res = await _custSvc.ImportCsvAsync(_tenant, _userAdmin, new CrmImportRequest(csvContent));

        Assert.NotNull(res);
        Assert.Equal(2, res.Total);
        Assert.Equal(2, res.Success);
        Assert.Equal(0, res.Failed);
    }

    [Fact]
    public async Task UC_CRM_014_ImportCsv_EmptyCsvText_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _custSvc.ImportCsvAsync(_tenant, _userAdmin, new CrmImportRequest("")));

        Assert.Contains("CSV trống", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_015: Tìm kiếm khách đa tiêu chí (Multi-criteria Search & Filtering)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_015_Search_MultiCriteria_ReturnsFilteredResults()
    {
        await _custSvc.UpsertAsync(_tenant, _userAdmin,
            new CrmCustomerUpsertRequest(null, "CUST_FILTER60", "Organization", "DN Tìm Kiếm 60", "Công ty Tìm Kiếm 60", "0908886644", "filter60@erp.vn",
                "038886644", "Customer", null, null, null, null, "Active"));

        var results = await _custSvc.SearchAsync(_tenant, new CrmCustomerSearchRequest(
            "DN Tìm Kiếm", "Organization", "Customer", "Active", null, "0908886644", "038886644", false));

        Assert.NotEmpty(results);
        Assert.Contains(results, c => c.Code == "CUST_FILTER60");
    }

    [Fact]
    public async Task UC_CRM_015_Search_NoMatchingResults_ReturnsEmptyList()
    {
        var results = await _custSvc.SearchAsync(_tenant, new CrmCustomerSearchRequest(
            "NonExistentCustomerNameKeyWord999", null, null, null, null, null, null, false));

        Assert.Empty(results);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_016: Tạo campaign marketing (Marketing Campaign Management)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_016_UpsertCampaign_ValidInput_CreatesCampaignSuccessfully()
    {
        var camp = await _campSvc.UpsertAsync(_tenant, _userAdmin, new CrmCampaignUpsertRequest(
            null, "CAMP_60_FB", "Chiến dịch Tết 2026", "Chương trình quảng cáo Facebook", "Social",
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(30), 50000000m, _userAdmin));

        Assert.NotNull(camp);
        Assert.Equal("CAMP_60_FB", camp.Code);
        Assert.Equal("Social", camp.Channel);
        Assert.Equal(50000000m, camp.BudgetAmount);
        Assert.Equal("Active", camp.Status);
    }

    [Fact]
    public async Task UC_CRM_016_UpsertCampaign_EndDateBeforeStartDate_ThrowsAppException()
    {
        var s = DateTimeOffset.UtcNow;
        var e = s.AddDays(-5);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _campSvc.UpsertAsync(_tenant, _userAdmin, new CrmCampaignUpsertRequest(
                null, "CAMP_BAD_DATE", "Chiến dịch Ngày Lỗi", null, "Email", s, e, 1000000m, null)));

        Assert.Contains("Ngày kết thúc phải ≥ ngày bắt đầu", ex.Message);
    }

    [Fact]
    public async Task UC_CRM_016_UpsertCampaign_InvalidChannel_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _campSvc.UpsertAsync(_tenant, _userAdmin, new CrmCampaignUpsertRequest(
                null, "CAMP_BAD_CHAN", "Chiến dịch Kênh Lỗi", null, "InvalidChannel", null, null, 1000000m, null)));

        Assert.Contains("Channel: Email|Social|SEM|Event|Other", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_017: Quản lý nhóm quảng cáo (Ad Groups & Web Leads Management)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_017_SyncWebLead_ValidWebLead_SyncsAndCreatesLead()
    {
        var camp = await _campSvc.UpsertAsync(_tenant, _userAdmin, new CrmCampaignUpsertRequest(
            null, "CAMP_60_SEM", "Chiến dịch Google Ads 60", null, "SEM", null, null, 20000000m, null));

        var webLead = await _campSvc.SyncWebLeadAsync(_tenant, new CrmWebLeadSyncRequest(
            "Trần Văn Web Lead", "0903334455", "weblead@erp.vn",
            "https://google.com/search", "https://erp.vn/landing-page", "google", "cpc", "CAMP_60_SEM",
            camp.Id));

        Assert.NotNull(webLead);
        Assert.Equal("Synced", webLead.SyncStatus);
        Assert.Equal("google", webLead.UtmSource);
        Assert.NotNull(webLead.LeadId);

        var leadsList = await _campSvc.ListWebLeadsAsync(_tenant, "Synced");
        Assert.NotEmpty(leadsList);
    }

    [Fact]
    public async Task UC_CRM_017_SyncWebLead_MissingPhoneAndEmail_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _campSvc.SyncWebLeadAsync(_tenant, new CrmWebLeadSyncRequest(
                "Trần Văn Thiếu Thông Tin", "", "", null, null, null, null, null, null)));

        Assert.Contains("Cần ít nhất SĐT hoặc Email", ex.Message);
    }
}
