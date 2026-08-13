using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Domain.Entities.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class CrmRewardSurveyRetentionCommissionPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmRewardSurveyRetentionCommissionService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();

    public CrmRewardSurveyRetentionCommissionPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-reward-survey-retention-commission-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "TCRM176", Name = "Tenant CRM 176" });
        _db.CrmCustomers.Add(new CrmCustomer
        {
            Id = _customerId,
            TenantId = _tenant,
            Code = "CUST-176",
            DisplayName = "Chuỗi Cửa hàng Tiện Lợi An Khang",
            Phone = "0933444555"
        });

        _db.SaveChanges();

        _svc = new CrmRewardSurveyRetentionCommissionService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_117: Tích điểm / đổi quà
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RedeemReward_RedeemsPointsForReward()
    {
        var req = new CrmRedeemRewardRequest(
            _customerId,
            "Voucher Giảm 500,000 VNĐ Đơn Phân Bón",
            500
        );

        var res = await _svc.RedeemRewardAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(_customerId, res.CustomerId);
        Assert.Equal("Voucher Giảm 500,000 VNĐ Đơn Phân Bón", res.RewardItemName);
        Assert.Equal("Fulfilled", res.Status);

        var list = await _svc.GetRedemptionsAsync(_tenant, _customerId);
        Assert.NotEmpty(list);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_118: Khảo sát hài lòng
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitSurveyResponse_RecordsFeedback()
    {
        var req = new CrmSubmitSurveyResponseRequest(
            _customerId,
            5,
            "Giao hàng đúng cam kết, nhân viên tư vấn nhiệt tình",
            "StoreVisit"
        );

        var res = await _svc.SubmitSurveyResponseAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(_customerId, res.CustomerId);
        Assert.Equal(5, res.RatingScore);

        var responses = await _svc.GetSurveyResponsesAsync(_tenant);
        Assert.NotEmpty(responses);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_119: Báo cáo retention / tái mua
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetRetentionReport_ReturnsRetentionMetrics()
    {
        var report = await _svc.GetRetentionReportAsync(_tenant);

        Assert.NotNull(report);
        Assert.True(report.TotalActiveCustomers > 0);
        Assert.True(report.RepeatPurchaseRatePercent > 50);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_120: Cấu hình rule hoa hồng
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ConfigureCommissionRule_CreatesActiveRule()
    {
        var req = new CrmConfigureCommissionRuleRequest(
            "COMM-FIELD-2026",
            "Hoa Hồng Field Sales Đạt Kế Hoạch 100M",
            "FieldSales",
            100000000m,
            3.0m
        );

        var res = await _svc.ConfigureCommissionRuleAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("COMM-FIELD-2026", res.RuleCode);
        Assert.Equal(3.0m, res.CommissionRatePercent);

        var rules = await _svc.GetCommissionRulesAsync(_tenant);
        Assert.NotEmpty(rules);
    }
}
