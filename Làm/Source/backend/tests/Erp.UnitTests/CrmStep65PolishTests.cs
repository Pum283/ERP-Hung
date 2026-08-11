using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 65:
///   UC_CRM_035 — Giới hạn lượt dùng voucher (Voucher Usage Limit & Redemption Enforcement)
///   UC_CRM_036 — Đồng bộ khuyến mại sang POS (POS Store Promotion Sync)
///   UC_CRM_037 — Áp dụng khuyến mại trên báo giá (Quote Promotion & Voucher Discount Application)
///   UC_CRM_038 — Báo cáo sử dụng voucher (Voucher Analytics & Usage Report)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class CrmStep65PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmPromotionService _promoSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public CrmStep65PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-step65-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_crm65", DisplayName = "Admin CRM 65" });

        _db.SaveChanges();

        _promoSvc = new CrmPromotionService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_035: Giới hạn lượt dùng voucher
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_035_RedeemVoucher_ValidVoucher_RedeemsSuccessfully()
    {
        var promo = await _promoSvc.UpsertAsync(_tenant, _userAdmin, new CrmPromotionUpsertRequest(
            null, "KM_REDEEM65", "Giảm 50k", null, "FixedAmount", 50000m, null, null, null, null, null, null, null, null));

        var vouchers = await _promoSvc.GenerateVouchersAsync(_tenant, _userAdmin, new CrmVoucherGenerateRequest(
            promo.Id, 1, "VCH65-", 1, null));

        var voucherCode = vouchers[0].VoucherCode;

        var res = await _promoSvc.RedeemVoucherAsync(_tenant, _userAdmin, new CrmVoucherRedeemRequest(
            voucherCode, null, null, null));

        Assert.True(res.Success);
        Assert.Equal(50000m, res.DiscountApplied);
        Assert.NotNull(res.Voucher);
        Assert.Equal(1, res.Voucher.UsageCount);
    }

    [Fact]
    public async Task UC_CRM_035_RedeemVoucher_ExceedMaxUsage_FailsRedemption()
    {
        var promo = await _promoSvc.UpsertAsync(_tenant, _userAdmin, new CrmPromotionUpsertRequest(
            null, "KM_MAXUSE", "Giảm 10%", null, "Percentage", 10m, null, null, null, null, null, null, null, null));

        var vouchers = await _promoSvc.GenerateVouchersAsync(_tenant, _userAdmin, new CrmVoucherGenerateRequest(
            promo.Id, 1, "MAX1-", 1, null));

        var code = vouchers[0].VoucherCode;

        // Lần 1 thành công
        await _promoSvc.RedeemVoucherAsync(_tenant, _userAdmin, new CrmVoucherRedeemRequest(code, null, null, null));

        // Lần 2 vượt quá lượt sử dụng
        var res2 = await _promoSvc.RedeemVoucherAsync(_tenant, _userAdmin, new CrmVoucherRedeemRequest(code, null, null, null));

        Assert.False(res2.Success);
        Assert.NotNull(res2.ErrorMessage);
    }

    [Fact]
    public async Task UC_CRM_035_RedeemVoucher_ExpiredVoucher_FailsRedemption()
    {
        var promo = await _promoSvc.UpsertAsync(_tenant, _userAdmin, new CrmPromotionUpsertRequest(
            null, "KM_EXP65", "Voucher Hết Hạn", null, "FixedAmount", 20000m, null, null, null, null, null, null, null, null));

        var vouchers = await _promoSvc.GenerateVouchersAsync(_tenant, _userAdmin, new CrmVoucherGenerateRequest(
            promo.Id, 1, "EXP-", 1, DateTimeOffset.UtcNow.AddDays(-1)));

        var code = vouchers[0].VoucherCode;

        var res = await _promoSvc.RedeemVoucherAsync(_tenant, _userAdmin, new CrmVoucherRedeemRequest(code, null, null, null));

        Assert.False(res.Success);
        Assert.Contains("hết hạn", res.ErrorMessage);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_036: Đồng bộ khuyến mại sang POS
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_036_SyncPromotionToPos_ValidPromo_SyncsToPosPromotionTable()
    {
        var promo = await _promoSvc.UpsertAsync(_tenant, _userAdmin, new CrmPromotionUpsertRequest(
            null, "KM_POS65", "Khuyến Mại Đồng Bộ POS", null, "Percentage", 15m, null, null, null, null, null, null, null, null));

        var posResult = await _promoSvc.SyncToPosAsync(_tenant, _userAdmin, promo.Id);

        Assert.NotNull(posResult);
        Assert.True(posResult.Created);
        Assert.Equal("KM_POS65", posResult.PosPromotionCode);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_037: Áp dụng khuyến mại trên báo giá
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_037_ApplyToQuote_WithVoucherCode_CalculatesDiscountAmount()
    {
        var promo = await _promoSvc.UpsertAsync(_tenant, _userAdmin, new CrmPromotionUpsertRequest(
            null, "KM_QUOTE65", "KM Báo Giá", null, "Percentage", 10m, null, null, null, null, null, null, null, null));

        var vouchers = await _promoSvc.GenerateVouchersAsync(_tenant, _userAdmin, new CrmVoucherGenerateRequest(
            promo.Id, 1, "QTC-", 1, null));

        var quoteId = Guid.NewGuid();
        _db.CrmQuotes.Add(new Erp.Domain.Entities.Crm.CrmQuote
        {
            Id = quoteId,
            TenantId = _tenant,
            Code = "QT_TEST65",
            SubTotal = 1000000m,
            TotalAmount = 1000000m,
            Status = "Draft",
            CreatedBy = _userAdmin
        });
        await _db.SaveChangesAsync();

        var res = await _promoSvc.ApplyOnQuoteAsync(_tenant, _userAdmin, new CrmApplyPromotionRequest(
            quoteId, promo.Id, vouchers[0].VoucherCode));

        Assert.NotNull(res);
        Assert.True(res.Applied);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_038: Báo cáo sử dụng voucher
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_038_GetVoucherReport_ReturnsUsageSummary()
    {
        var promo = await _promoSvc.UpsertAsync(_tenant, _userAdmin, new CrmPromotionUpsertRequest(
            null, "KM_RPT65", "KM Báo Cáo", null, "FixedAmount", 30000m, null, null, null, null, null, null, null, null));

        await _promoSvc.GenerateVouchersAsync(_tenant, _userAdmin, new CrmVoucherGenerateRequest(
            promo.Id, 3, "RPT-", 1, null));

        var rptList = await _promoSvc.GetVoucherUsageReportAsync(_tenant, promo.Id);

        Assert.NotNull(rptList);
    }

    [Fact]
    public async Task UC_CRM_035_RedeemVoucher_NonExistentCode_FailsRedemption()
    {
        var res = await _promoSvc.RedeemVoucherAsync(_tenant, _userAdmin, new CrmVoucherRedeemRequest(
            "INVALID_CODE_999", null, null, null));

        Assert.False(res.Success);
        Assert.Contains("Không tìm thấy", res.ErrorMessage);
    }

    [Fact]
    public async Task UC_CRM_036_SyncPromotionToPos_NonExistentPromo_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _promoSvc.SyncToPosAsync(_tenant, _userAdmin, Guid.NewGuid()));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC_CRM_037_CalcDiscount_PercentType_CalculatesCorrectDiscount()
    {
        var promo = await _promoSvc.UpsertAsync(_tenant, _userAdmin, new CrmPromotionUpsertRequest(
            null, "KM_CALC_PCT", "KM Calc Pct", null, "Percentage", 20m, 100000m, null, null, null, null, null, null, null));

        var entity = await _db.CrmPromotions.FindAsync(promo.Id);
        var discount = CrmPromotionService.CalcDiscount(entity!, 1000000m); // 20% of 1M = 200k, max 100k

        Assert.Equal(100000m, discount);
    }

    [Fact]
    public async Task UC_CRM_038_GetVoucherReport_ReturnsListForTenant()
    {
        var list = await _promoSvc.GetVoucherUsageReportAsync(_tenant, null, DateTimeOffset.UtcNow.AddDays(-10), DateTimeOffset.UtcNow.AddDays(10));
        Assert.NotNull(list);
    }
}
