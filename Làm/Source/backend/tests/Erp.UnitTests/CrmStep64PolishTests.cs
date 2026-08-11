using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 64:
///   UC_CRM_031 — Dashboard marketing (Marketing Performance & Campaign Summary Dashboard)
///   UC_CRM_032 — Tạo chương trình khuyến mại (Promotion Master Campaign Definition)
///   UC_CRM_033 — Cấu hình điều kiện khuyến mại (Promotion Conditions & Min Order Thresholds)
///   UC_CRM_034 — Sinh mã voucher (Batch Voucher Code Generator Engine)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class CrmStep64PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmCampaignService _campSvc;
    private readonly CrmPromotionService _promoSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public CrmStep64PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-step64-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_crm64", DisplayName = "Admin CRM 64" });

        _db.SaveChanges();

        _campSvc = new CrmCampaignService(_db);
        _promoSvc = new CrmPromotionService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_031: Dashboard marketing
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_031_GetDashboard_ReturnsAggregatedMarketingKpis()
    {
        await _campSvc.UpsertAsync(_tenant, _userAdmin, new CrmCampaignUpsertRequest(
            null, "CAMP_DASH64", "Chiến dịch Tết 2026", null, "SEM", null, null, 50000000m, null));

        var dash = await _campSvc.GetDashboardAsync(_tenant);

        Assert.NotNull(dash);
        Assert.True(dash.TotalCampaigns >= 1);
        Assert.True(dash.TotalBudget >= 50000000m);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_032: Tạo chương trình khuyến mại
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_032_UpsertPromo_ValidInput_CreatesPromotionSuccessfully()
    {
        var promo = await _promoSvc.UpsertAsync(_tenant, _userAdmin, new CrmPromotionUpsertRequest(
            null, "KM_TET2026", "Khuyến Mại Giảm 20% Giáp Thìn", "Áp dụng toàn quốc",
            "Percentage", 20m, 500000m, 1000000m,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30),
            100, 1, null, null));

        Assert.NotNull(promo);
        Assert.Equal("KM_TET2026", promo.Code);
        Assert.Equal("Percentage", promo.DiscountType);
        Assert.Equal(20m, promo.DiscountValue);
        Assert.Equal("Active", promo.Status);
    }

    [Fact]
    public async Task UC_CRM_032_UpsertPromo_InvalidDiscountType_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _promoSvc.UpsertAsync(_tenant, _userAdmin, new CrmPromotionUpsertRequest(
                null, "KM_BAD_TYPE", "Khuyến Mại Lỗi", null,
                "InvalidDiscount", 10m, null, null,
                null, null, null, null, null, null)));

        Assert.Contains("DiscountType: Percentage|FixedAmount|BuyXGetY|FreeShipping", ex.Message);
    }

    [Fact]
    public async Task UC_CRM_032_UpsertPromo_EndDateBeforeStartDate_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _promoSvc.UpsertAsync(_tenant, _userAdmin, new CrmPromotionUpsertRequest(
                null, "KM_BAD_DATE", "Khuyến Mại Lỗi Ngày", null,
                "Percentage", 10m, null, null,
                DateTimeOffset.UtcNow.AddDays(10), DateTimeOffset.UtcNow.AddDays(2),
                null, null, null, null)));

        Assert.Contains("Ngày kết thúc phải >=", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_033: Cấu hình điều kiện khuyến mại
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_033_UpsertPromo_WithConditions_SavesConditionsProperly()
    {
        var conds = new List<CrmPromotionConditionRequest>
        {
            new CrmPromotionConditionRequest("MinAmount", "1000000", "GreaterThan"),
            new CrmPromotionConditionRequest("CustomerSegment", "VIP", "Equals")
        };

        var promo = await _promoSvc.UpsertAsync(_tenant, _userAdmin, new CrmPromotionUpsertRequest(
            null, "KM_VIP1M", "Khuyến Mại Đơn 1 Triệu Cho VIP", null,
            "FixedAmount", 100000m, null, 1000000m,
            null, null, null, null, null, conds));

        Assert.NotNull(promo);
        Assert.Equal(2, promo.Conditions.Count);
        Assert.Contains(promo.Conditions, c => c.ConditionType == "MinAmount" && c.ConditionValue == "1000000");
    }

    [Fact]
    public async Task UC_CRM_033_UpsertPromo_InvalidConditionType_ThrowsAppException()
    {
        var conds = new List<CrmPromotionConditionRequest>
        {
            new CrmPromotionConditionRequest("InvalidCondition", "999", "Equals")
        };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _promoSvc.UpsertAsync(_tenant, _userAdmin, new CrmPromotionUpsertRequest(
                null, "KM_BAD_COND", "Khuyến Mại Lỗi Điều Kiện", null,
                "FixedAmount", 50000m, null, null,
                null, null, null, null, null, conds)));

        Assert.Contains("ConditionType: Product|Category|CustomerSegment|MinQty|MinAmount", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_034: Sinh mã voucher
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_034_GenerateVouchers_ValidBatchCount_GeneratesVouchersBatch()
    {
        var promo = await _promoSvc.UpsertAsync(_tenant, _userAdmin, new CrmPromotionUpsertRequest(
            null, "KM_VOUCHER64", "Chương Trình Voucher Tết", null,
            "Percentage", 15m, null, null,
            null, null, null, null, null, null));

        var vouchers = await _promoSvc.GenerateVouchersAsync(_tenant, _userAdmin, new CrmVoucherGenerateRequest(
            promo.Id, 5, "VCH2026-", 1, DateTimeOffset.UtcNow.AddDays(30)));

        Assert.NotNull(vouchers);
        Assert.Equal(5, vouchers.Count);
        Assert.True(vouchers.All(v => v.VoucherCode.StartsWith("VCH2026-")));
    }

    [Fact]
    public async Task UC_CRM_034_GenerateVouchers_CountOutOfRange_ThrowsAppException()
    {
        var promo = await _promoSvc.UpsertAsync(_tenant, _userAdmin, new CrmPromotionUpsertRequest(
            null, "KM_VCH_EX", "Voucher Quá Số Lượng", null,
            "FixedAmount", 20000m, null, null,
            null, null, null, null, null, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _promoSvc.GenerateVouchersAsync(_tenant, _userAdmin, new CrmVoucherGenerateRequest(
                promo.Id, 2000, "ERR-", 1, null)));

        Assert.Contains("Số lượng voucher 1–500", ex.Message);
    }

    [Fact]
    public async Task UC_CRM_032_ListPromos_ReturnsActivePromotions()
    {
        await _promoSvc.UpsertAsync(_tenant, _userAdmin, new CrmPromotionUpsertRequest(
            null, "KM_LIST1", "KM List 1", null,
            "FixedAmount", 50000m, null, null,
            null, null, null, null, null, null));

        var list = await _promoSvc.ListAsync(_tenant);

        Assert.NotNull(list);
        Assert.NotEmpty(list);
        Assert.Contains(list, p => p.Code == "KM_LIST1");
    }

    [Fact]
    public async Task UC_CRM_034_ListVouchers_ReturnsGeneratedVouchers()
    {
        var promo = await _promoSvc.UpsertAsync(_tenant, _userAdmin, new CrmPromotionUpsertRequest(
            null, "KM_LISTVCH", "KM List Voucher", null,
            "Percentage", 10m, null, null,
            null, null, null, null, null, null));

        await _promoSvc.GenerateVouchersAsync(_tenant, _userAdmin, new CrmVoucherGenerateRequest(
            promo.Id, 2, "LSTV-", 1, null));

        var list = await _promoSvc.ListVouchersAsync(_tenant, promo.Id);

        Assert.NotNull(list);
        Assert.Equal(2, list.Count);
    }
}
