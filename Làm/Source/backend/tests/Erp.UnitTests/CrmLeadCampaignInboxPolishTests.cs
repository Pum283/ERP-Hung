using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Domain.Entities.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class CrmLeadCampaignInboxPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmLeadCampaignInboxService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _agentId = Guid.NewGuid();

    public CrmLeadCampaignInboxPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-lead-campaign-inbox-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "TCRM168", Name = "Tenant CRM 168" });
        _db.CrmCustomers.Add(new CrmCustomer
        {
            Id = _customerId,
            TenantId = _tenant,
            Code = "CUST-CRM-01",
            DisplayName = "Công ty Cổ phần Tập đoàn Nam Long",
            Phone = "0909123456"
        });

        _db.SaveChanges();

        _svc = new CrmLeadCampaignInboxService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_007: Đánh giá tiềm năng
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EvaluateLeadPotential_EvaluatesAndReturnsHotTier()
    {
        var req = new CrmEvaluatePotentialRequest(
            _customerId,
            85,
            "Khách hàng có ngân sách lớn và nhu cầu triển khai gấp trong Q3/2026"
        );

        var res = await _svc.EvaluateLeadPotentialAsync(_tenant, _userId, req);

        Assert.NotNull(res);
        Assert.Equal(_customerId, res.CustomerId);
        Assert.Equal(85, res.Score);
        Assert.Equal("Hot", res.PriorityTier);

        var list = await _svc.GetPotentialScoresAsync(_tenant);
        Assert.NotEmpty(list);
        Assert.Contains(list, s => s.CustomerId == _customerId && s.PriorityTier == "Hot");
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_022: Nhân bản campaign
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DuplicateCampaign_ClonesCampaignSuccessfully()
    {
        var req = new CrmDuplicateCampaignRequest(
            Guid.NewGuid(),
            "Chiến dịch Khuyến mãi Mùa Thu 2026 (Copy)",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30)
        );

        var res = await _svc.DuplicateCampaignAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.NotEqual(Guid.Empty, res.NewCampaignId);
        Assert.Equal("Chiến dịch Khuyến mãi Mùa Thu 2026 (Copy)", res.NewCampaignName);
        Assert.Equal("Active", res.Status);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_039: Hộp thư tập trung đa kênh
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetConversations_ReturnsOmnichannelInboxList()
    {
        var list = await _svc.GetConversationsAsync(_tenant, "Zalo");

        Assert.NotEmpty(list);
        Assert.Contains(list, c => c.Channel == "Zalo");
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_040: Tiếp nhận hội thoại mới
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ReceiveAndAssignConversation_AssignsAgentToConversation()
    {
        var convId = Guid.NewGuid();
        var req = new CrmReceiveConversationRequest(convId, _agentId);

        var res = await _svc.ReceiveAndAssignConversationAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(convId, res.ConversationId);
        Assert.Equal(_agentId, res.AssignedAgentId);
        Assert.Equal("Assigned", res.Status);

        var dbConv = await _db.CrmOmnichannelConversations.FirstOrDefaultAsync(c => c.TenantId == _tenant && c.Id == convId);
        Assert.NotNull(dbConv);
        Assert.Equal(_agentId, dbConv.AssignedAgentId);
    }
}
