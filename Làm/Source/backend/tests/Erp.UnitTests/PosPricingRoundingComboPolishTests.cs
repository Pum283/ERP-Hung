using Erp.Application.DTOs;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class PosPricingRoundingComboPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly PosPricingRoundingComboService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();

    public PosPricingRoundingComboPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("pos-pricing-rounding-combo-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "TPOS180", Name = "Tenant POS 180" });
        _db.SaveChanges();

        _svc = new PosPricingRoundingComboService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_017 & UC_POS_018: Giá theo khung giờ & ngày trong tuần
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveTimeSlotPriceRule_SavesHappyHourRuleSuccessfully()
    {
        var req = new PosSaveTimeSlotPriceRuleRequest(
            "Happy Hour Cà Phê (14h-17h)",
            _productId,
            new TimeSpan(14, 0, 0),
            new TimeSpan(17, 0, 0),
            "Monday,Tuesday,Wednesday,Thursday,Friday",
            25000m,
            20
        );

        var res = await _svc.SaveTimeSlotPriceRuleAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("Happy Hour Cà Phê (14h-17h)", res.RuleName);
        Assert.Equal(25000m, res.SpecialPriceVnd);

        var list = await _svc.GetTimeSlotPriceRulesAsync(_tenant);
        Assert.NotEmpty(list);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_020: Làm tròn tiền thanh toán
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CalculateCashRounding_RoundsCashTotalToNearestInterval()
    {
        // 123,400 VND rounded to nearest 500 VND => 123,500 VND
        var res1 = await _svc.CalculateCashRoundingAsync(123400m, 500);
        Assert.Equal(123500m, res1.RoundedTotalVnd);
        Assert.Equal(100m, res1.RoundingDifferenceVnd);

        // 123,100 VND rounded to nearest 500 VND => 123,000 VND
        var res2 = await _svc.CalculateCashRoundingAsync(123100m, 500);
        Assert.Equal(123000m, res2.RoundedTotalVnd);
        Assert.Equal(-100m, res2.RoundingDifferenceVnd);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_POS_023: Khuyến mại theo combo
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveComboPromotionRule_SavesComboRuleSuccessfully()
    {
        var req = new PosSaveComboPromotionRuleRequest(
            "COMBO-LUNCH",
            "Combo Bữa Trưa Tiết Kiệm",
            new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
            55000m,
            new DateTime(2026, 8, 1),
            new DateTime(2026, 12, 31)
        );

        var res = await _svc.SaveComboPromotionRuleAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("COMBO-LUNCH", res.ComboCode);
        Assert.Equal(55000m, res.FixedComboPriceVnd);

        var list = await _svc.GetComboPromotionRulesAsync(_tenant);
        Assert.NotEmpty(list);
    }
}
