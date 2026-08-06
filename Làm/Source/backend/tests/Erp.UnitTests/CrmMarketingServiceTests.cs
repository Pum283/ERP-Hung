using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Domain.Entities.Crm;
using Erp.Infrastructure.Implementations.Services.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.UnitTests;

/// <summary>Test thật (EF InMemory) cho Cap-2 CRM marketing / promo.</summary>
public sealed class CrmMarketingServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmCampaignService _campaigns;
    private readonly CrmPromotionService _promos;
    private readonly Guid _tenant = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly Guid _user = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public CrmMarketingServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-mkt-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _campaigns = new CrmCampaignService(_db);
        _promos = new CrmPromotionService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task UpsertCampaign_CreatesActiveWithCode()
    {
        var dto = await _campaigns.UpsertAsync(_tenant, _user, new CrmCampaignUpsertRequest(
            null, "", "Summer 2026", "Promo", "Social",
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(30), 50_000_000, null));

        Assert.StartsWith("CAMP-", dto.Code);
        Assert.Equal("Active", dto.Status);
        Assert.Equal("Social", dto.Channel);
        Assert.Equal(1, await _db.CrmCampaigns.CountAsync());
    }

    [Fact]
    public async Task UpsertCampaign_RejectsDuplicateCode()
    {
        await _campaigns.UpsertAsync(_tenant, _user, new CrmCampaignUpsertRequest(
            null, "CAMP-A", "A", null, "Email", null, null, 1, null));
        await Assert.ThrowsAsync<AppException>(() => _campaigns.UpsertAsync(_tenant, _user, new CrmCampaignUpsertRequest(
            null, "CAMP-A", "B", null, "Email", null, null, 1, null)));
    }

    [Fact]
    public async Task Expense_UpdatesSpentAmount()
    {
        var c = await _campaigns.UpsertAsync(_tenant, _user, new CrmCampaignUpsertRequest(
            null, "CAMP-E", "E", null, "SEM", null, null, 10_000_000, null));
        await _campaigns.UpsertExpenseAsync(_tenant, _user, c.Id, new CrmCampaignExpenseUpsertRequest(
            null, "Ads", "FB", 2_000_000, null, null));
        await _campaigns.UpsertExpenseAsync(_tenant, _user, c.Id, new CrmCampaignExpenseUpsertRequest(
            null, "Media", "Banner", 500_000, null, null));

        var got = await _campaigns.GetAsync(_tenant, c.Id);
        Assert.Equal(2_500_000, got.SpentAmount);
        Assert.Equal(2, (await _campaigns.ListExpensesAsync(_tenant, c.Id)).Count);
    }

    [Fact]
    public async Task CloseCampaign_SetsClosedAndSnapshot()
    {
        var c = await _campaigns.UpsertAsync(_tenant, _user, new CrmCampaignUpsertRequest(
            null, "CAMP-C", "Close me", null, "Event", null, null, 1, null));
        await _campaigns.UpsertExpenseAsync(_tenant, _user, c.Id, new CrmCampaignExpenseUpsertRequest(
            null, "Event", null, 1000, null, null));
        var closed = await _campaigns.CloseAsync(_tenant, _user, c.Id, new CrmCampaignCloseRequest("Done"));
        Assert.Equal("Closed", closed.Status);
        Assert.NotNull(closed.ClosedAt);
        Assert.Equal(1000, closed.SpentAmount);
        await Assert.ThrowsAsync<AppException>(() => _campaigns.UpsertExpenseAsync(
            _tenant, _user, c.Id, new CrmCampaignExpenseUpsertRequest(null, "Ads", null, 10, null, null)));
    }

    [Fact]
    public async Task SyncWebLead_CreatesLeadAndIncrementsCount()
    {
        var c = await _campaigns.UpsertAsync(_tenant, _user, new CrmCampaignUpsertRequest(
            null, "CAMP-W", "Web", null, "Other", null, null, 1, null));
        var wl = await _campaigns.SyncWebLeadAsync(_tenant, new CrmWebLeadSyncRequest(
            "Nguyen Van A", "0901234567", "a@b.com", null, "/l", "fb", "cpc", "summer", c.Id));

        Assert.Equal("Synced", wl.SyncStatus);
        Assert.NotNull(wl.LeadId);
        Assert.Equal(1, await _db.CrmLeads.CountAsync());
        var camp = await _campaigns.GetAsync(_tenant, c.Id);
        Assert.Equal(1, camp.LeadCount);
    }

    [Fact]
    public async Task Metrics_ComputesCplRoasRoi()
    {
        var c = await _campaigns.UpsertAsync(_tenant, _user, new CrmCampaignUpsertRequest(
            null, "CAMP-M", "Metrics", null, "SEM", null, null, 1, null));
        var entity = await _db.CrmCampaigns.FirstAsync(x => x.Id == c.Id);
        entity.RevenueGenerated = 10_000;
        await _db.SaveChangesAsync();
        await _campaigns.UpsertExpenseAsync(_tenant, _user, c.Id, new CrmCampaignExpenseUpsertRequest(
            null, "Ads", null, 2_000, null, null));
        await _campaigns.SyncWebLeadAsync(_tenant, new CrmWebLeadSyncRequest(
            "B", "090", null, null, null, null, null, null, c.Id));
        await _campaigns.SyncWebLeadAsync(_tenant, new CrmWebLeadSyncRequest(
            "C", "091", null, null, null, null, null, null, c.Id));

        var m = await _campaigns.GetMetricsAsync(_tenant, c.Id);
        Assert.Equal(2, m.LeadCount);
        Assert.Equal(1000, m.CostPerLead);
        Assert.Equal(5m, m.Roas);
        Assert.Equal(400m, m.RoiPercent);

        var dash = await _campaigns.GetDashboardAsync(_tenant);
        Assert.True(dash.TotalCampaigns >= 1);
        Assert.Contains(dash.CampaignMetrics, x => x.CampaignId == c.Id);
    }

    [Fact]
    public async Task UpsertPromotion_WithConditions()
    {
        var p = await _promos.UpsertAsync(_tenant, _user, new CrmPromotionUpsertRequest(
            null, "PROMO-1", "Giảm 10%", null, "Percentage", 10, null, 100_000,
            null, null, 50, 2, null,
            [new CrmPromotionConditionRequest("MinAmount", "100000", "GreaterThan")]));

        Assert.Equal("Active", p.Status);
        Assert.Single(p.Conditions);
        Assert.Equal("MinAmount", p.Conditions[0].ConditionType);
    }

    [Fact]
    public async Task GenerateVouchers_CreatesUniqueCodes()
    {
        var p = await _promos.UpsertAsync(_tenant, _user, new CrmPromotionUpsertRequest(
            null, "PROMO-V", "V", null, "FixedAmount", 50_000, null, null,
            null, null, null, null, null, null));
        var vouchers = await _promos.GenerateVouchersAsync(_tenant, _user, new CrmVoucherGenerateRequest(
            p.Id, 3, "SUMMER", 1, null));
        Assert.Equal(3, vouchers.Count);
        Assert.Equal(3, vouchers.Select(v => v.VoucherCode).Distinct().Count());
        Assert.All(vouchers, v => Assert.StartsWith("SUMMER-", v.VoucherCode));
    }

    [Fact]
    public async Task CalcDiscount_PercentageAndCap()
    {
        var promo = new CrmPromotion
        {
            DiscountType = "Percentage",
            DiscountValue = 20,
            MaxDiscountAmount = 100_000,
            MinOrderValue = 500_000,
        };
        Assert.Equal(0, CrmPromotionService.CalcDiscount(promo, 100_000));
        Assert.Equal(100_000, CrmPromotionService.CalcDiscount(promo, 1_000_000));
        promo.DiscountType = "FixedAmount";
        promo.DiscountValue = 30_000;
        Assert.Equal(30_000, CrmPromotionService.CalcDiscount(promo, 1_000_000));
    }

    [Fact]
    public async Task ApplyOnQuote_UpdatesDiscountAndTotal()
    {
        var quote = new CrmQuote
        {
            TenantId = _tenant,
            Code = "Q-1",
            SubTotal = 1_000_000,
            TotalAmount = 1_000_000,
            Status = "Draft",
        };
        _db.CrmQuotes.Add(quote);
        await _db.SaveChangesAsync();

        var p = await _promos.UpsertAsync(_tenant, _user, new CrmPromotionUpsertRequest(
            null, "PROMO-Q", "10%", null, "Percentage", 10, null, null,
            null, null, 10, null, null, null));

        var result = await _promos.ApplyOnQuoteAsync(_tenant, _user, new CrmApplyPromotionRequest(quote.Id, p.Id, null));
        Assert.True(result.Applied);
        Assert.Equal(100_000, result.DiscountAmount);

        var q = await _db.CrmQuotes.AsNoTracking().FirstAsync(x => x.Id == quote.Id);
        Assert.Equal(100_000, q.DiscountAmount);
        Assert.Equal(900_000, q.TotalAmount);
        Assert.Equal(1, (await _promos.GetAsync(_tenant, p.Id)).CurrentUsageCount);
    }

    [Fact]
    public async Task RedeemVoucher_RespectsMaxUsage()
    {
        var p = await _promos.UpsertAsync(_tenant, _user, new CrmPromotionUpsertRequest(
            null, "PROMO-R", "R", null, "FixedAmount", 10_000, null, null,
            null, null, 100, null, null, null));
        var vouchers = await _promos.GenerateVouchersAsync(_tenant, _user, new CrmVoucherGenerateRequest(
            p.Id, 1, "ONE", 1, null));
        var code = vouchers[0].VoucherCode;

        var ok = await _promos.RedeemVoucherAsync(_tenant, _user, new CrmVoucherRedeemRequest(code, null, null, null));
        Assert.True(ok.Success);
        var again = await _promos.RedeemVoucherAsync(_tenant, _user, new CrmVoucherRedeemRequest(code, null, null, null));
        Assert.False(again.Success);
    }

    [Fact]
    public async Task SyncWebLead_RequiresPhoneOrEmail()
    {
        await Assert.ThrowsAsync<AppException>(() => _campaigns.SyncWebLeadAsync(
            _tenant, new CrmWebLeadSyncRequest("X", null, null, null, null, null, null, null, null)));
    }
}
