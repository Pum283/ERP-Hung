using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 70:
///   UC_CRM_064 — Dự báo doanh thu (Weighted Revenue Forecasting Engine)
///   UC_CRM_065 — Gắn sản phẩm / giá trị ước tính (Opportunity Line Items & Valuation)
///   UC_CRM_066 — Đối thủ / ghi chú đàm phán (Competitor Intelligence & Negotiation Notes)
///   UC_CRM_067 — Chuyển cơ hội sang báo giá (Opportunity-to-Quote Conversion Handover)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class CrmStep70PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmLeadService _leadSvc;
    private readonly CrmSalesService _salesSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public CrmStep70PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-step70-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_crm70", DisplayName = "Admin CRM 70" });

        _db.SaveChanges();

        _salesSvc = new CrmSalesService(_db, null!, null!, null!);
        _leadSvc = new CrmLeadService(_db, _salesSvc);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_064: Dự báo doanh thu
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_064_GetRevenueForecast_ReturnsWeightedForecastMetrics()
    {
        await _leadSvc.UpsertOpportunityAsync(_tenant, _userAdmin, new CrmOpportunityUpsertRequest(
            null, null, "Cơ hội Dự báo 1", null, null, null,
            "Qualification", 300000000m, 50, DateTimeOffset.UtcNow.AddMonths(1), null, null, null));

        var forecast = await _leadSvc.GetRevenueForecastAsync(_tenant);

        Assert.NotNull(forecast);
        Assert.True(forecast.TotalEstimatedValue >= 300000000m);
        Assert.True(forecast.WeightedForecastValue >= 150000000m);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_065: Gắn sản phẩm / giá trị ước tính
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_065_UpsertOpportunityLine_ValidInput_AddsLineItemToOpportunity()
    {
        var opp = await _leadSvc.UpsertOpportunityAsync(_tenant, _userAdmin, new CrmOpportunityUpsertRequest(
            null, null, "Cơ hội Có Sản Phẩm", null, null, null,
            "Qualification", 100000000m, 50, null, null, null, null));

        var line = await _leadSvc.UpsertOpportunityLineAsync(_tenant, _userAdmin, opp.Id, new CrmOpportunityLineUpsertRequest(
            null, "SKU-ERP-01", "Gói ERP Cloud Standard", 2, 50000000m));

        Assert.NotNull(line);
        Assert.Equal("SKU-ERP-01", line.ItemCode);
        Assert.Equal(100000000m, line.LineAmount);
    }

    [Fact]
    public async Task UC_CRM_065_UpsertOpportunityLine_InvalidQuantity_ThrowsAppException()
    {
        var opp = await _leadSvc.UpsertOpportunityAsync(_tenant, _userAdmin, new CrmOpportunityUpsertRequest(
            null, null, "Cơ hội Qty Lỗi", null, null, null,
            "Qualification", 50000000m, 50, null, null, null, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leadSvc.UpsertOpportunityLineAsync(_tenant, _userAdmin, opp.Id, new CrmOpportunityLineUpsertRequest(
                null, "SKU-01", "Tên Sp", 0, 10000m)));

        Assert.Contains("SL > 0", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_066: Đối thủ / ghi chú đàm phán
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_066_UpdateCompetitor_ValidInput_UpdatesCompetitorInfo()
    {
        var opp = await _leadSvc.UpsertOpportunityAsync(_tenant, _userAdmin, new CrmOpportunityUpsertRequest(
            null, null, "Cơ hội Đàm Phán", null, null, null,
            "Negotiation", 400000000m, 80, null, null, null, null));

        var updated = await _leadSvc.UpdateCompetitorInfoAsync(_tenant, _userAdmin, opp.Id, new CrmOpportunityCompetitorRequest(
            "Đối thủ X", "Khách hàng muốn giảm giá thêm 5% và kéo dài thanh toán 60 ngày"));

        Assert.NotNull(updated);
        Assert.Equal("Đối thủ X", updated.CompetitorName);
        Assert.Equal("Khách hàng muốn giảm giá thêm 5% và kéo dài thanh toán 60 ngày", updated.NegotiationNotes);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_067: Chuyển cơ hội sang báo giá
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_067_CreateQuoteFromOpportunity_ValidOpportunity_CreatesQuoteSuccessfully()
    {
        var opp = await _leadSvc.UpsertOpportunityAsync(_tenant, _userAdmin, new CrmOpportunityUpsertRequest(
            null, null, "Cơ hội Chuyển Báo Giá", null, null, null,
            "Proposal", 250000000m, 70, null, null, null, null));

        var quote = await _leadSvc.CreateQuoteFromOpportunityAsync(_tenant, _userAdmin, opp.Id);

        Assert.NotNull(quote);
        Assert.Equal(opp.Id, quote.OpportunityId);
        Assert.Equal("Draft", quote.Status);
    }

    [Fact]
    public async Task UC_CRM_067_CreateQuoteFromOpportunity_NonExistentOpp_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leadSvc.CreateQuoteFromOpportunityAsync(_tenant, _userAdmin, Guid.NewGuid()));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC_CRM_065_UpsertOpportunityLine_NonExistentOpportunity_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leadSvc.UpsertOpportunityLineAsync(_tenant, _userAdmin, Guid.NewGuid(), new CrmOpportunityLineUpsertRequest(
                null, "SKU-ERR", "Sp Lỗi", 1, 10000m)));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC_CRM_066_UpdateCompetitor_NonExistentOpp_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leadSvc.UpdateCompetitorInfoAsync(_tenant, _userAdmin, Guid.NewGuid(), new CrmOpportunityCompetitorRequest(
                "Đối thủ Y", "Notes")));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC_CRM_064_GetRevenueForecast_ReturnsForecastMonthlyItems()
    {
        var forecast = await _leadSvc.GetRevenueForecastAsync(_tenant);
        Assert.NotNull(forecast);
        Assert.NotNull(forecast.MonthlyForecasts);
    }

    [Fact]
    public async Task UC_CRM_065_UpsertOpportunityLine_UpdateExistingLine_UpdatesQuantityAndPrice()
    {
        var opp = await _leadSvc.UpsertOpportunityAsync(_tenant, _userAdmin, new CrmOpportunityUpsertRequest(
            null, null, "Cơ hội Cập Nhật Dòng", null, null, null,
            "Qualification", 10000000m, 50, null, null, null, null));

        var line1 = await _leadSvc.UpsertOpportunityLineAsync(_tenant, _userAdmin, opp.Id, new CrmOpportunityLineUpsertRequest(
            null, "SKU-UPDATE", "Dòng Cần Sửa", 1, 10000000m));

        var line2 = await _leadSvc.UpsertOpportunityLineAsync(_tenant, _userAdmin, opp.Id, new CrmOpportunityLineUpsertRequest(
            line1.Id, "SKU-UPDATE", "Dòng Cần Sửa", 3, 12000000m));

        Assert.Equal(3, line2.Quantity);
        Assert.Equal(36000000m, line2.LineAmount);
    }
}
