using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Crm;
using Erp.Infrastructure.Implementations.Services.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class CrmLeadPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmLeadService _service;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public CrmLeadPolishTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "CrmLeadPolishTestDb_" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(options);
        _db.Users.Add(new Erp.Domain.Entities.Sys.AppUser
        {
            Id = _userId,
            TenantId = _tenantId,
            Username = "testuser",
            DisplayName = "Test User",
            Status = Erp.Domain.Enums.Sys.UserStatus.Active
        });
        _db.SaveChanges();

        var sales = new CrmSalesService(_db, null!, null!, null!);
        _service = new CrmLeadService(_db, sales);
    }

    public void Dispose()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    [Fact]
    public async Task CalculateLeadScore_UpdatesScoreBasedOnProfileAndActivities()
    {
        // 1. Arrange: Create a lead
        var lead = await _service.UpsertLeadAsync(_tenantId, _userId, new CrmLeadUpsertRequest(
            Id: null, Code: "LD-SCORE-01", Name: "Nguyen Van Score", Phone: "0988777666", Email: "score@test.com",
            CompanyName: "Score Corp", SourceId: null, OwnerUserId: _userId, CustomerId: null,
            PipelineStatus: "New", Score: null, NextFollowUpAt: null, Note: null, IntakeChannel: "Manual"), default);

        // Add 2 activities (+20 points)
        await _service.AddActivityAsync(_tenantId, _userId, new CrmLeadActivityUpsertRequest(lead.Id, "Call", "Goi trao doi", null), default);
        await _service.AddActivityAsync(_tenantId, _userId, new CrmLeadActivityUpsertRequest(lead.Id, "Email", "Gui bao gia", null), default);

        // 2. Act: Calculate Score (Phone +20, Email +20, Company +20, 2 Acts +20 = 80)
        var scored = await _service.CalculateLeadScoreAsync(_tenantId, _userId, lead.Id, default);

        // 3. Assert
        Assert.Equal(80, scored.Score);
    }

    [Fact]
    public async Task MergeLeads_TransfersTasksAndActivitiesAndMarksSecondaryLost()
    {
        // 1. Arrange: Create primary and secondary leads
        var primary = await _service.UpsertLeadAsync(_tenantId, _userId, new CrmLeadUpsertRequest(
            Id: null, Code: "LD-PRIM-01", Name: "Primary Lead", Phone: "0912345678", Email: null,
            CompanyName: "Primary Corp", SourceId: null, OwnerUserId: _userId, CustomerId: null,
            PipelineStatus: "New", Score: null, NextFollowUpAt: null, Note: null, IntakeChannel: "Manual"), default);

        var secondary = await _service.UpsertLeadAsync(_tenantId, _userId, new CrmLeadUpsertRequest(
            Id: null, Code: "LD-SEC-01", Name: "Secondary Lead", Phone: null, Email: "secondary@test.com",
            CompanyName: null, SourceId: null, OwnerUserId: _userId, CustomerId: null,
            PipelineStatus: "Contacted", Score: null, NextFollowUpAt: null, Note: null, IntakeChannel: "Manual"), default);

        await _service.UpsertTaskAsync(_tenantId, _userId, new CrmLeadTaskUpsertRequest(null, secondary.Id, "Call secondary", DateTimeOffset.UtcNow, _userId, "Open", false, null), default);
        await _service.AddActivityAsync(_tenantId, _userId, new CrmLeadActivityUpsertRequest(secondary.Id, "Note", "Ghi chu lead phu", null), default);

        // 2. Act: Merge secondary into primary
        var merged = await _service.MergeLeadsAsync(_tenantId, _userId, new CrmLeadMergeRequest(primary.Id, secondary.Id, "Trung lead"), default);

        // 3. Assert
        Assert.Equal("secondary@test.com", merged.Email); // Copied email from secondary
        var secLeadInDb = await _db.CrmLeads.FirstAsync(x => x.Id == secondary.Id);
        Assert.Equal(primary.Id, secLeadInDb.MergedIntoId);
        Assert.Equal("Lost", secLeadInDb.PipelineStatus);

        // Verify task and activity moved to primary
        var tasks = await _db.CrmLeadTasks.Where(x => x.LeadId == primary.Id).ToListAsync();
        var acts = await _db.CrmLeadActivities.Where(x => x.LeadId == primary.Id).ToListAsync();
        Assert.Single(tasks);
        Assert.Single(acts);
    }

    [Fact]
    public async Task ForecastAndWinRate_ComputesWeightedRevenueAndWinRateMetrics()
    {
        // 1. Arrange: Create opportunities in different stages
        var opp1 = await _service.UpsertOpportunityAsync(_tenantId, _userId, new CrmOpportunityUpsertRequest(
            null, "OPP-01", "Deal A", null, null, _userId, "Proposal", 100_000_000, 50, DateTimeOffset.UtcNow, null, null, null), default);

        var opp2 = await _service.UpsertOpportunityAsync(_tenantId, _userId, new CrmOpportunityUpsertRequest(
            null, "OPP-02", "Deal B", null, null, _userId, "Negotiation", 200_000_000, 80, DateTimeOffset.UtcNow, null, null, null), default);

        var oppWon = await _service.UpsertOpportunityAsync(_tenantId, _userId, new CrmOpportunityUpsertRequest(
            null, "OPP-03", "Deal C", null, null, _userId, "Won", 150_000_000, 100, DateTimeOffset.UtcNow, null, null, null), default);

        var oppLost = await _service.UpsertOpportunityAsync(_tenantId, _userId, new CrmOpportunityUpsertRequest(
            null, "OPP-04", "Deal D", null, null, _userId, "Lost", 50_000_000, 0, DateTimeOffset.UtcNow, null, null, null), default);

        await _service.SetOpportunityStageAsync(_tenantId, _userId, oppLost.Id, new CrmOpportunityStageRequest("Lost", "Gia cao"), default);

        // 2. Act
        var forecast = await _service.GetRevenueForecastAsync(_tenantId, default);
        var winRate = await _service.GetWinRateReportAsync(_tenantId, default);

        // 3. Assert Forecast: 100M*0.5 + 200M*0.8 + 150M*1.0 = 50M + 160M + 150M = 360M
        Assert.Equal(450_000_000, forecast.TotalEstimatedValue); // 100M + 200M + 150M (excluding lost)
        Assert.Equal(360_000_000, forecast.WeightedForecastValue);
        Assert.NotEmpty(forecast.MonthlyForecasts);

        // Assert Win Rate: 4 Total, 1 Won, 1 Lost -> 25% WinRate, 25% LossRate
        Assert.Equal(4, winRate.Total);
        Assert.Equal(1, winRate.Won);
        Assert.Equal(1, winRate.Lost);
        Assert.Equal(2, winRate.InProgress);
        Assert.Equal(25m, winRate.WinRatePercent);
        Assert.Single(winRate.LossReasons);
        Assert.Equal("Gia cao", winRate.LossReasons[0].Reason);
    }

    [Fact]
    public async Task UpdateCompetitorInfo_UpdatesCompetitorAndNotes()
    {
        var opp = await _service.UpsertOpportunityAsync(_tenantId, _userId, new CrmOpportunityUpsertRequest(
            null, "OPP-COMP", "Deal Competitor", null, null, _userId, "Proposal", 50_000_000, 50, DateTimeOffset.UtcNow, null, null, null), default);

        var res = await _service.UpdateCompetitorInfoAsync(_tenantId, _userId, opp.Id, new CrmOpportunityCompetitorRequest("Doi thu A", "Dang dam phan chiet khau 5%"), default);

        Assert.Equal("Doi thu A", res.CompetitorName);
        Assert.Equal("Dang dam phan chiet khau 5%", res.NegotiationNotes);
    }
}
