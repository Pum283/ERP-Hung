using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Domain.Entities.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class CrmOmnichannelRoutingSlaPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmOmnichannelRoutingSlaService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _agent1 = Guid.NewGuid();
    private readonly Guid _agent2 = Guid.NewGuid();
    private readonly Guid _conversationId = Guid.NewGuid();

    public CrmOmnichannelRoutingSlaPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-routing-sla-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "TCRM169", Name = "Tenant CRM 169" });
        _db.CrmOmnichannelConversations.Add(new CrmOmnichannelConversation
        {
            Id = _conversationId,
            TenantId = _tenant,
            Channel = "Zalo",
            ExternalId = "ZALO-169",
            CustomerName = "Trần Thanh Tâm",
            AssignedAgentId = _agent1,
            Status = "Assigned"
        });

        _db.SaveChanges();

        _svc = new CrmOmnichannelRoutingSlaService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_041: Phân phối hội thoại theo rule
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateRoutingRule_CreatesAndReturnsRuleDto()
    {
        var req = new CrmCreateRoutingRuleRequest(
            "Phân phối Zalo cho Đội kỹ thuật",
            "SkillBased",
            "Tech_Support",
            1
        );

        var res = await _svc.CreateRoutingRuleAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("Phân phối Zalo cho Đội kỹ thuật", res.RuleName);
        Assert.Equal("SkillBased", res.Strategy);
        Assert.True(res.IsActive);

        var list = await _svc.GetRoutingRulesAsync(_tenant);
        Assert.NotEmpty(list);
        Assert.Contains(list, r => r.RuleName == "Phân phối Zalo cho Đội kỹ thuật");
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_042: Chuyển hội thoại giữa agent
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task TransferConversation_TransfersSessionToNewAgent()
    {
        var req = new CrmTransferConversationRequest(
            _conversationId,
            _agent2,
            "Khách cần tư vấn chuyên sâu gói Module Sản Xuất"
        );

        var res = await _svc.TransferConversationAsync(_tenant, _agent1, req);

        Assert.NotNull(res);
        Assert.Equal(_conversationId, res.ConversationId);
        Assert.Equal(_agent1, res.FromAgentId);
        Assert.Equal(_agent2, res.ToAgentId);
        Assert.Equal("Transferred", res.Status);

        var dbConv = await _db.CrmOmnichannelConversations.FirstOrDefaultAsync(c => c.TenantId == _tenant && c.Id == _conversationId);
        Assert.NotNull(dbConv);
        Assert.Equal(_agent2, dbConv.AssignedAgentId);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_043: SLA phản hồi & cảnh báo
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CheckAndLogSla_DetectsSlaBreachCorrectly()
    {
        var req = new CrmCheckSlaBreachRequest(
            _conversationId,
            5, // Max 5 mins SLA
            12 // Actual 12 mins -> Breached
        );

        var res = await _svc.CheckAndLogSlaAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.True(res.IsBreached);
        Assert.Equal("Breached", res.AlertStatus);
        Assert.Equal(12, res.ActualResponseMinutes);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_044: Chatbot kịch bản
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveBotFlow_SavesScriptedBotFlowSuccessfully()
    {
        var req = new CrmSaveBotFlowRequest(
            "Bot tự động Báo giá",
            "#baogia",
            "[{\"step\":1,\"text\":\"Chào bạn, vui lòng nhập SĐT\"}]"
        );

        var res = await _svc.SaveBotFlowAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("Bot tự động Báo giá", res.FlowName);
        Assert.Equal("#baogia", res.TriggerKeyword);

        var list = await _svc.GetBotFlowsAsync(_tenant);
        Assert.NotEmpty(list);
    }
}
