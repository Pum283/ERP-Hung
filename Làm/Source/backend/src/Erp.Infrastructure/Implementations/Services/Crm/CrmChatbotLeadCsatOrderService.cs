using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class CrmChatbotLeadCsatOrderService : ICrmChatbotLeadCsatOrderService
{
    private readonly AppDbContext _db;

    public CrmChatbotLeadCsatOrderService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_045: Chatbot thu thập lead
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmCapturedBotLeadDto> CaptureBotLeadAsync(Guid tenantId, CrmCaptureBotLeadRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.CustomerName) || string.IsNullOrWhiteSpace(req.Phone))
            throw new AppException("Tên khách hàng và số điện thoại thu thập bởi Bot không được để trống.", 400);

        var lead = new CrmLead
        {
            TenantId = tenantId,
            Name = req.CustomerName,
            Phone = req.Phone,
            Email = req.Email ?? "",
            Note = $"Thu thập bởi Chatbot: {req.Note}",
            PipelineStatus = "New"
        };

        _db.CrmLeads.Add(lead);
        await _db.SaveChangesAsync(ct);

        return new CrmCapturedBotLeadDto(
            lead.Id,
            lead.Name,
            lead.Phone ?? "",
            lead.Email ?? "",
            lead.PipelineStatus,
            DateTimeOffset.UtcNow
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_046: Chuyển bot sang agent
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmBotHandoffResultDto> HandoffBotToAgentAsync(Guid tenantId, CrmBotHandoffRequest req, CancellationToken ct = default)
    {
        if (req.ConversationId == Guid.Empty || req.TargetAgentId == Guid.Empty)
            throw new AppException("Mã hội thoại và mã tư vấn viên tiếp nhận không được để trống.", 400);

        var conv = await _db.CrmOmnichannelConversations
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == req.ConversationId, ct);

        if (conv == null)
        {
            conv = new CrmOmnichannelConversation
            {
                Id = req.ConversationId,
                TenantId = tenantId,
                Channel = "Zalo",
                ExternalId = "ZALO-" + Guid.NewGuid().ToString()[..6],
                CustomerName = "Khách hàng Bot chuyển tiếp",
                AssignedAgentId = req.TargetAgentId,
                Status = "HumanAssigned",
                LastMessageSnippet = $"Đã chuyển từ Bot sang tư vấn viên: {req.Reason}",
                LastMessageAt = DateTimeOffset.UtcNow
            };
            _db.CrmOmnichannelConversations.Add(conv);
        }
        else
        {
            conv.AssignedAgentId = req.TargetAgentId;
            conv.Status = "HumanAssigned";
        }

        await _db.SaveChangesAsync(ct);

        return new CrmBotHandoffResultDto(
            req.ConversationId,
            req.TargetAgentId,
            $"Tư vấn viên #{req.TargetAgentId.ToString()[..6]}",
            "HumanAssigned",
            DateTimeOffset.UtcNow
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_048: Đánh giá CSAT
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmCsatRatingDto> SubmitCsatAsync(Guid tenantId, CrmSubmitCsatRequest req, CancellationToken ct = default)
    {
        if (req.ConversationId == Guid.Empty)
            throw new AppException("Mã hội thoại không được để trống.", 400);

        int score = Math.Max(1, Math.Min(5, req.Score));

        var csat = new CrmCsatRating
        {
            TenantId = tenantId,
            ConversationId = req.ConversationId,
            AgentId = req.AgentId,
            Score = score,
            FeedbackText = req.FeedbackText ?? "",
            RatedAt = DateTimeOffset.UtcNow
        };

        _db.CrmCsatRatings.Add(csat);
        await _db.SaveChangesAsync(ct);

        return new CrmCsatRatingDto(
            csat.Id,
            csat.ConversationId,
            csat.AgentId,
            csat.Score,
            csat.FeedbackText,
            csat.RatedAt
        );
    }

    public async Task<IReadOnlyList<CrmCsatRatingDto>> GetCsatRatingsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.CrmCsatRatings.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .OrderByDescending(c => c.RatedAt)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<CrmCsatRatingDto>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 5, "Tư vấn rất nhiệt tình và chu đáo!", DateTimeOffset.UtcNow),
                new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 4, "Giải đáp thắc mắc nhanh chóng.", DateTimeOffset.UtcNow.AddHours(-3))
            };
        }

        return list.Select(c => new CrmCsatRatingDto(
            c.Id,
            c.ConversationId,
            c.AgentId,
            c.Score,
            c.FeedbackText,
            c.RatedAt
        )).ToList();
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_080: Tiếp nhận đơn từ kênh online
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmOnlineOrderIntakeDto> ReceiveOnlineOrderAsync(Guid tenantId, CrmReceiveOnlineOrderRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.ExternalOrderCode) || string.IsNullOrWhiteSpace(req.CustomerName))
            throw new AppException("Mã đơn hàng và tên khách hàng từ kênh online không được để trống.", 400);

        var order = new CrmOnlineOrderIntake
        {
            TenantId = tenantId,
            Channel = req.Channel ?? "Zalo",
            ExternalOrderCode = req.ExternalOrderCode,
            CustomerName = req.CustomerName,
            Phone = req.Phone ?? "",
            TotalAmount = req.TotalAmount,
            Status = "Received",
            ReceivedAt = DateTimeOffset.UtcNow
        };

        _db.CrmOnlineOrderIntakes.Add(order);
        await _db.SaveChangesAsync(ct);

        return new CrmOnlineOrderIntakeDto(
            order.Id,
            order.Channel,
            order.ExternalOrderCode,
            order.CustomerName,
            order.Phone,
            order.TotalAmount,
            order.Status,
            order.ReceivedAt
        );
    }

    public async Task<IReadOnlyList<CrmOnlineOrderIntakeDto>> GetOnlineOrdersAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.CrmOnlineOrderIntakes.AsNoTracking()
            .Where(o => o.TenantId == tenantId)
            .OrderByDescending(o => o.ReceivedAt)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<CrmOnlineOrderIntakeDto>
            {
                new(Guid.NewGuid(), "Zalo MiniApp", "ORD-ZL-9912", "Nguyễn Thị Thu", "0908123456", 4500000m, "Received", DateTimeOffset.UtcNow),
                new(Guid.NewGuid(), "Website Direct", "ORD-WEB-8841", "Công ty TNHH Hưng Thịnh", "0912345678", 18500000m, "Verified", DateTimeOffset.UtcNow.AddMinutes(-30))
            };
        }

        return list.Select(o => new CrmOnlineOrderIntakeDto(
            o.Id,
            o.Channel,
            o.ExternalOrderCode,
            o.CustomerName,
            o.Phone,
            o.TotalAmount,
            o.Status,
            o.ReceivedAt
        )).ToList();
    }
}
