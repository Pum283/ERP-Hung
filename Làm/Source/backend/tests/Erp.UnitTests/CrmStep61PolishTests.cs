using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 61:
///   UC_CRM_018 — Gắn sản phẩm / đối tượng mục tiêu (Campaign Target Product & Audience Tagging)
///   UC_CRM_019 — Ghi nhận chi phí quảng cáo (Ad Expense Recording & Tracking)
///   UC_CRM_020 — Gắn ngân sách & theo dõi (Budget Allocation & Expense Aggregation)
///   UC_CRM_021 — Đánh giá hậu chiến dịch (Post-Campaign Marketing ROI Analytics)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class CrmStep61PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmCampaignService _svc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public CrmStep61PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-step61-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_crm61", DisplayName = "Admin CRM 61" });

        _db.SaveChanges();

        _svc = new CrmCampaignService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_018: Gắn sản phẩm / đối tượng mục tiêu
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_018_UpsertCampaign_WithTargetDescription_SavesTargetDetails()
    {
        var camp = await _svc.UpsertAsync(_tenant, _userAdmin, new CrmCampaignUpsertRequest(
            null, "CAMP_PROD61", "Chiến dịch Sản Phẩm A", "Khách hàng Doanh nghiệp vừa và nhỏ, sản phẩm ERP Cloud",
            "Social", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(30), 100000000m, _userAdmin));

        Assert.NotNull(camp);
        Assert.Equal("CAMP_PROD61", camp.Code);
        Assert.Contains("ERP Cloud", camp.Description);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_019: Ghi nhận chi phí quảng cáo (Ad Expense Recording)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_019_UpsertExpense_ValidInput_RecordsExpenseAndUpdateSpentAmount()
    {
        var camp = await _svc.UpsertAsync(_tenant, _userAdmin, new CrmCampaignUpsertRequest(
            null, "CAMP_EXP61", "Chiến dịch Chạy Ads", null, "SEM", null, null, 50000000m, null));

        var exp1 = await _svc.UpsertExpenseAsync(_tenant, _userAdmin, camp.Id, new CrmCampaignExpenseUpsertRequest(
            null, "Ads", "Chi phí Google Ads Tuần 1", 10000000m, DateTimeOffset.UtcNow, "INV_001"));

        var exp2 = await _svc.UpsertExpenseAsync(_tenant, _userAdmin, camp.Id, new CrmCampaignExpenseUpsertRequest(
            null, "Media", "Chi phí Ban rôn & Truyền thông", 5000000m, DateTimeOffset.UtcNow, "INV_002"));

        Assert.NotNull(exp1);
        Assert.Equal(10000000m, exp1.Amount);
        Assert.Equal("Ads", exp1.ExpenseType);

        var updatedCamp = await _svc.GetAsync(_tenant, camp.Id);
        Assert.Equal(15000000m, updatedCamp.SpentAmount); // Tự động tổng hợp 10M + 5M
    }

    [Fact]
    public async Task UC_CRM_019_UpsertExpense_NegativeAmount_ThrowsAppException()
    {
        var camp = await _svc.UpsertAsync(_tenant, _userAdmin, new CrmCampaignUpsertRequest(
            null, "CAMP_EXP_NEG", "Chiến dịch Lỗi Số Tiền", null, "SEM", null, null, 10000000m, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertExpenseAsync(_tenant, _userAdmin, camp.Id, new CrmCampaignExpenseUpsertRequest(
                null, "Ads", "Chi phí âm", -500000m, null, null)));

        Assert.Contains("Số tiền chi phí phải > 0", ex.Message);
    }

    [Fact]
    public async Task UC_CRM_019_UpsertExpense_InvalidType_ThrowsAppException()
    {
        var camp = await _svc.UpsertAsync(_tenant, _userAdmin, new CrmCampaignUpsertRequest(
            null, "CAMP_EXP_TYPE", "Chiến dịch Lỗi Kênh", null, "SEM", null, null, 10000000m, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertExpenseAsync(_tenant, _userAdmin, camp.Id, new CrmCampaignExpenseUpsertRequest(
                null, "InvalidType", "Chi phí loại lỗi", 1000000m, null, null)));

        Assert.Contains("ExpenseType: Ads|Media|Event|Agency|Other", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_020: Gắn ngân sách & theo dõi (Budget Allocation & Tracking)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_020_UpsertExpense_ClosedCampaign_ThrowsAppException()
    {
        var camp = await _svc.UpsertAsync(_tenant, _userAdmin, new CrmCampaignUpsertRequest(
            null, "CAMP_CLOSED61", "Chiến dịch Sắp Đóng", null, "Event", null, null, 20000000m, null));

        await _svc.CloseAsync(_tenant, _userAdmin, camp.Id, new CrmCampaignCloseRequest("Đã xong sự kiện"));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertExpenseAsync(_tenant, _userAdmin, camp.Id, new CrmCampaignExpenseUpsertRequest(
                null, "Event", "Chi phí phát sinh sau khi đóng", 2000000m, null, null)));

        Assert.Contains("Campaign đã đóng — không ghi chi phí", ex.Message);
    }

    [Fact]
    public async Task UC_CRM_020_ListExpenses_ReturnsAllRecordedExpenses()
    {
        var camp = await _svc.UpsertAsync(_tenant, _userAdmin, new CrmCampaignUpsertRequest(
            null, "CAMP_LISTEXP", "Chiến dịch Xem Chi Phí", null, "Social", null, null, 30000000m, null));

        await _svc.UpsertExpenseAsync(_tenant, _userAdmin, camp.Id, new CrmCampaignExpenseUpsertRequest(null, "Ads", "Ads 1", 5000000m, null, null));
        await _svc.UpsertExpenseAsync(_tenant, _userAdmin, camp.Id, new CrmCampaignExpenseUpsertRequest(null, "Agency", "Agency 1", 8000000m, null, null));

        var expenses = await _svc.ListExpensesAsync(_tenant, camp.Id);

        Assert.NotNull(expenses);
        Assert.Equal(2, expenses.Count);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_021: Đánh giá hậu chiến dịch (Post-Campaign Marketing ROI Analytics)
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_021_GetMetrics_CalculatesCplAndRoiMetrics()
    {
        var camp = await _svc.UpsertAsync(_tenant, _userAdmin, new CrmCampaignUpsertRequest(
            null, "CAMP_ROI61", "Chiến dịch Tính ROI", null, "Social", null, null, 40000000m, null));

        await _svc.UpsertExpenseAsync(_tenant, _userAdmin, camp.Id, new CrmCampaignExpenseUpsertRequest(
            null, "Ads", "Chi phí Ads Facebook", 20000000m, null, null));

        await _svc.SyncWebLeadAsync(_tenant, new CrmWebLeadSyncRequest(
            "Lead ROI 1", "0901112233", "lead1@erp.vn", null, null, "facebook", "cpc", "CAMP_ROI61", camp.Id));

        var metrics = await _svc.GetMetricsAsync(_tenant, camp.Id);

        Assert.NotNull(metrics);
        Assert.Equal(20000000m, metrics.TotalSpent);
        Assert.Equal(1, metrics.LeadCount);
        Assert.Equal(20000000m, metrics.CostPerLead); // 20M / 1 lead = 20M
    }

    [Fact]
    public async Task UC_CRM_021_GetDashboard_AggregatesAllCampaignMetrics()
    {
        await _svc.UpsertAsync(_tenant, _userAdmin, new CrmCampaignUpsertRequest(
            null, "CAMP_DASH61", "Chiến dịch Dashboard", null, "Email", null, null, 10000000m, null));

        var dash = await _svc.GetDashboardAsync(_tenant);

        Assert.NotNull(dash);
        Assert.True(dash.TotalCampaigns >= 1);
        Assert.NotNull(dash.CampaignMetrics);
    }

    [Fact]
    public async Task UC_CRM_021_GetMetrics_NonExistentCampaign_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.GetMetricsAsync(_tenant, Guid.NewGuid()));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("Không tìm thấy campaign", ex.Message);
    }

    [Fact]
    public async Task UC_CRM_020_UpsertExpense_UpdateExpense_UpdatesAmountAndRecalculatesTotalSpent()
    {
        var camp = await _svc.UpsertAsync(_tenant, _userAdmin, new CrmCampaignUpsertRequest(
            null, "CAMP_EXPUPD", "Chiến dịch Sửa Chi Phí", null, "Event", null, null, 50000000m, null));

        var exp = await _svc.UpsertExpenseAsync(_tenant, _userAdmin, camp.Id, new CrmCampaignExpenseUpsertRequest(
            null, "Event", "Chi phí ban đầu", 10000000m, null, null));

        var updatedExp = await _svc.UpsertExpenseAsync(_tenant, _userAdmin, camp.Id, new CrmCampaignExpenseUpsertRequest(
            exp.Id, "Event", "Chi phí đã điều chỉnh", 12000000m, null, "INV_UPDATED"));

        Assert.Equal(12000000m, updatedExp.Amount);

        var campDto = await _svc.GetAsync(_tenant, camp.Id);
        Assert.Equal(12000000m, campDto.SpentAmount);
    }
}
