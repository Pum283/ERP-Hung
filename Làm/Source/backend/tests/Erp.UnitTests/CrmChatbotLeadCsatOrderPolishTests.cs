using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Domain.Entities.Crm;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class CrmChatbotLeadCsatOrderPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CrmChatbotLeadCsatOrderService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _agentId = Guid.NewGuid();
    private readonly Guid _conversationId = Guid.NewGuid();

    public CrmChatbotLeadCsatOrderPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("crm-bot-csat-order-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "TCRM170", Name = "Tenant CRM 170" });
        _db.CrmOmnichannelConversations.Add(new CrmOmnichannelConversation
        {
            Id = _conversationId,
            TenantId = _tenant,
            Channel = "Zalo",
            ExternalId = "ZALO-170",
            CustomerName = "Đặng Hoàng Nam",
            AssignedAgentId = _agentId,
            Status = "Active"
        });

        _db.SaveChanges();

        _svc = new CrmChatbotLeadCsatOrderService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_045: Chatbot thu thập lead
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CaptureBotLead_CapturesLeadFromChatbot()
    {
        var req = new CrmCaptureBotLeadRequest(
            "Phạm Văn Hải",
            "0909999888",
            "hai.pham@gmail.com",
            "Khách nhắn tin qua Zalo MiniApp"
        );

        var res = await _svc.CaptureBotLeadAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("Phạm Văn Hải", res.CustomerName);
        Assert.Equal("0909999888", res.Phone);
        Assert.Equal("New", res.Status);

        var dbLead = await _db.CrmLeads.FirstOrDefaultAsync(l => l.TenantId == _tenant && l.Phone == "0909999888");
        Assert.NotNull(dbLead);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_046: Chuyển bot sang agent
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task HandoffBotToAgent_HandoffsSessionToHumanAgent()
    {
        var req = new CrmBotHandoffRequest(
            _conversationId,
            _agentId,
            "Khách hàng yêu cầu gặp trực tiếp tư vấn viên"
        );

        var res = await _svc.HandoffBotToAgentAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(_conversationId, res.ConversationId);
        Assert.Equal(_agentId, res.AssignedAgentId);
        Assert.Equal("HumanAssigned", res.HandoffStatus);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_048: Đánh giá CSAT
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitCsat_SubmitsCustomerSatisfactionScore()
    {
        var req = new CrmSubmitCsatRequest(
            _conversationId,
            _agentId,
            5,
            "Dịch vụ tuyệt vời!"
        );

        var res = await _svc.SubmitCsatAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal(5, res.Score);
        Assert.Equal("Dịch vụ tuyệt vời!", res.FeedbackText);

        var list = await _svc.GetCsatRatingsAsync(_tenant);
        Assert.NotEmpty(list);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_080: Tiếp nhận đơn từ kênh online
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ReceiveOnlineOrder_ReceivesOrderFromChannel()
    {
        var req = new CrmReceiveOnlineOrderRequest(
            "Zalo MiniApp",
            "ORD-ZL-2026",
            "Nguyễn Thị Mai",
            "0908123456",
            3500000m
        );

        var res = await _svc.ReceiveOnlineOrderAsync(_tenant, req);

        Assert.NotNull(res);
        Assert.Equal("ORD-ZL-2026", res.ExternalOrderCode);
        Assert.Equal(3500000m, res.TotalAmount);
        Assert.Equal("Received", res.Status);

        var list = await _svc.GetOnlineOrdersAsync(_tenant);
        Assert.NotEmpty(list);
        Assert.Contains(list, o => o.ExternalOrderCode == "ORD-ZL-2026");
    }
}
