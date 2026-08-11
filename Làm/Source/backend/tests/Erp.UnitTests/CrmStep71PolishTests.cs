using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 71:
///   UC_CRM_068 — Đóng thắng / thua (Opportunity Closing: Closed-Won / Closed-Lost Stage Transition)
///   UC_CRM_069 — Báo cáo win-rate (Sales Win-Rate Analytics & Loss Reason Breakdown)
///   UC_CRM_070 — Tạo báo giá từ cơ hội (Quote Creation & Header Initialization)
///   UC_CRM_071 — Thêm dòng sản phẩm / dịch vụ (Quote Line Items & Amount Calculation)
/// 10 test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class CrmStep71PolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmLeadService _leadSvc;
    private readonly CrmSalesService _salesSvc;

    private readonly Guid _tenant    = Guid.NewGuid();
    private readonly Guid _userAdmin = Guid.NewGuid();

    public CrmStep71PolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-step71-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "ENTERPRISE", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            MaxUsers = 100, MaxOrgUnits = 500
        });

        _db.Users.Add(new AppUser { Id = _userAdmin, TenantId = _tenant, Username = "admin_crm71", DisplayName = "Admin CRM 71" });

        _db.SaveChanges();

        _salesSvc = new CrmSalesService(_db, null!, null!, null!);
        _leadSvc = new CrmLeadService(_db, _salesSvc);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_068: Đóng thắng / thua
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_068_SetOpportunityStage_WonStage_ClosesOpportunityAsWon()
    {
        var opp = await _leadSvc.UpsertOpportunityAsync(_tenant, _userAdmin, new CrmOpportunityUpsertRequest(
            null, null, "Cơ hội Đóng Thắng", null, null, null,
            "Negotiation", 500000000m, 90, null, null, null, null));

        var closed = await _leadSvc.SetOpportunityStageAsync(_tenant, _userAdmin, opp.Id, new CrmOpportunityStageRequest("Won", null));

        Assert.NotNull(closed);
        Assert.Equal("Won", closed.Stage);
        Assert.Equal(100m, closed.ProbabilityPercent);
    }

    [Fact]
    public async Task UC_CRM_068_SetOpportunityStage_LostStage_ClosesOpportunityAsLostWithReason()
    {
        var opp = await _leadSvc.UpsertOpportunityAsync(_tenant, _userAdmin, new CrmOpportunityUpsertRequest(
            null, null, "Cơ hội Đóng Thua", null, null, null,
            "Proposal", 200000000m, 70, null, null, null, null));

        var closed = await _leadSvc.SetOpportunityStageAsync(_tenant, _userAdmin, opp.Id, new CrmOpportunityStageRequest("Lost", "Giá ngân sách vượt quá khả năng"));

        Assert.NotNull(closed);
        Assert.Equal("Lost", closed.Stage);
        Assert.Equal("Giá ngân sách vượt quá khả năng", closed.LostReason);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_069: Báo cáo win-rate
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_069_GetWinRateReport_ReturnsWinRateMetrics()
    {
        var report = await _leadSvc.GetWinRateReportAsync(_tenant);

        Assert.NotNull(report);
        Assert.NotNull(report.LossReasons);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_070: Tạo báo giá từ cơ hội
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_070_CreateQuote_ValidInput_CreatesQuoteHeader()
    {
        var quote = await _salesSvc.UpsertQuoteAsync(_tenant, _userAdmin, new CrmQuoteUpsertRequest(
            null, null, null, null, DateTimeOffset.UtcNow.AddDays(30), 0m, "Tạo báo giá"));

        Assert.NotNull(quote);
        Assert.Equal("Draft", quote.Status);
    }

    [Fact]
    public async Task UC_CRM_070_CreateQuote_InvalidDiscount_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.UpsertQuoteAsync(_tenant, _userAdmin, new CrmQuoteUpsertRequest(
                null, null, null, null, DateTimeOffset.UtcNow.AddDays(30), 150m, null)));

        Assert.Contains("Chiết khấu 0–100%", ex.Message);
    }

    // ────────────────────────────────────────────────────────────────────────
    // UC_CRM_071: Thêm dòng sản phẩm / dịch vụ
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UC_CRM_071_UpsertQuoteLine_ValidInput_AddsLineItemToQuote()
    {
        var quote = await _salesSvc.UpsertQuoteAsync(_tenant, _userAdmin, new CrmQuoteUpsertRequest(
            null, null, null, null, DateTimeOffset.UtcNow.AddDays(30), 0m, null));

        var line = await _salesSvc.UpsertQuoteLineAsync(_tenant, _userAdmin, quote.Id, new CrmQuoteLineUpsertRequest(
            null, "SKU-BG-01", "Gói Bản Quyền Cloud", 5, 20000000m));

        Assert.NotNull(line);
        Assert.Equal("SKU-BG-01", line.ItemCode);
        Assert.Equal(100000000m, line.LineAmount);
    }

    [Fact]
    public async Task UC_CRM_071_UpsertQuoteLine_InvalidQuantity_ThrowsAppException()
    {
        var quote = await _salesSvc.UpsertQuoteAsync(_tenant, _userAdmin, new CrmQuoteUpsertRequest(
            null, null, null, null, DateTimeOffset.UtcNow.AddDays(30), 0m, null));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.UpsertQuoteLineAsync(_tenant, _userAdmin, quote.Id, new CrmQuoteLineUpsertRequest(
                null, "SKU-ERR", "Dòng Lỗi Qty", 0, 10000m)));

        Assert.Contains("Số lượng > 0", ex.Message);
    }

    [Fact]
    public async Task UC_CRM_068_SetOpportunityStage_NonExistentOpp_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _leadSvc.SetOpportunityStageAsync(_tenant, _userAdmin, Guid.NewGuid(), new CrmOpportunityStageRequest("Won", null)));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC_CRM_071_UpsertQuoteLine_NonExistentQuote_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _salesSvc.UpsertQuoteLineAsync(_tenant, _userAdmin, Guid.NewGuid(), new CrmQuoteLineUpsertRequest(
                null, "SKU-ERR", "Dòng Lỗi", 1, 10000m)));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC_CRM_070_GetQuoteDetail_ReturnsQuoteHeaderAndLines()
    {
        var quote = await _salesSvc.UpsertQuoteAsync(_tenant, _userAdmin, new CrmQuoteUpsertRequest(
            null, null, null, null, DateTimeOffset.UtcNow.AddDays(30), 0m, null));

        var detail = await _salesSvc.GetQuoteDetailAsync(_tenant, quote.Id);

        Assert.NotNull(detail);
        Assert.NotNull(detail.Quote);
    }
}
