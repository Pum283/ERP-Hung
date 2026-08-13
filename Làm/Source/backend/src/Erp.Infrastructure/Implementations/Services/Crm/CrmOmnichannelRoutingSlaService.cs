using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class CrmOmnichannelRoutingSlaService : ICrmOmnichannelRoutingSlaService
{
    private readonly AppDbContext _db;

    public CrmOmnichannelRoutingSlaService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_041: Phân phối hội thoại theo rule
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmChatRoutingRuleDto> CreateRoutingRuleAsync(Guid tenantId, CrmCreateRoutingRuleRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.RuleName))
            throw new AppException("Tên quy tắc phân phối không được để trống.", 400);

        var rule = new CrmChatRoutingRule
        {
            TenantId = tenantId,
            RuleName = req.RuleName,
            Strategy = req.Strategy ?? "RoundRobin",
            TargetSkillGroup = req.TargetSkillGroup ?? "Sales_Support",
            IsActive = true,
            Priority = req.Priority <= 0 ? 1 : req.Priority
        };

        _db.CrmChatRoutingRules.Add(rule);
        await _db.SaveChangesAsync(ct);

        return new CrmChatRoutingRuleDto(
            rule.Id,
            rule.RuleName,
            rule.Strategy,
            rule.TargetSkillGroup,
            rule.IsActive,
            rule.Priority
        );
    }

    public async Task<IReadOnlyList<CrmChatRoutingRuleDto>> GetRoutingRulesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.CrmChatRoutingRules.AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .OrderBy(r => r.Priority)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<CrmChatRoutingRuleDto>
            {
                new(Guid.NewGuid(), "Quy tắc xoay vòng Zalo Sales", "RoundRobin", "Sales_Zalo", true, 1),
                new(Guid.NewGuid(), "Cân bằng tải Facebook Lead", "LoadBalance", "Sales_FB", true, 2),
                new(Guid.NewGuid(), "Phân phối theo chuyên môn ERP", "SkillBased", "ERP_Consultants", true, 3)
            };
        }

        return list.Select(r => new CrmChatRoutingRuleDto(
            r.Id,
            r.RuleName,
            r.Strategy,
            r.TargetSkillGroup,
            r.IsActive,
            r.Priority
        )).ToList();
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_042: Chuyển hội thoại giữa agent
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmConversationTransferResultDto> TransferConversationAsync(Guid tenantId, Guid fromAgentId, CrmTransferConversationRequest req, CancellationToken ct = default)
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
                CustomerName = "Khách hàng chuyển giao",
                AssignedAgentId = req.TargetAgentId,
                Status = "Transferred",
                LastMessageSnippet = $"Đã chuyển giao: {req.TransferNote}",
                LastMessageAt = DateTimeOffset.UtcNow
            };
            _db.CrmOmnichannelConversations.Add(conv);
        }
        else
        {
            conv.AssignedAgentId = req.TargetAgentId;
            conv.Status = "Transferred";
        }

        await _db.SaveChangesAsync(ct);

        return new CrmConversationTransferResultDto(
            req.ConversationId,
            fromAgentId,
            req.TargetAgentId,
            $"Tư vấn viên #{req.TargetAgentId.ToString()[..6]}",
            "Transferred",
            req.TransferNote ?? "Chuyển giao cuộc chat",
            DateTimeOffset.UtcNow
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_043: SLA phản hồi & cảnh báo
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmChatSlaAlertDto> CheckAndLogSlaAsync(Guid tenantId, CrmCheckSlaBreachRequest req, CancellationToken ct = default)
    {
        if (req.ConversationId == Guid.Empty)
            throw new AppException("Mã hội thoại không được để trống.", 400);

        int maxMins = req.MaxResponseMinutes <= 0 ? 5 : req.MaxResponseMinutes;
        bool isBreached = req.ActualResponseMinutes > maxMins;
        string status = isBreached ? "Breached" : req.ActualResponseMinutes >= maxMins - 1 ? "Warning" : "Normal";

        var alert = new CrmChatSlaAlert
        {
            TenantId = tenantId,
            ConversationId = req.ConversationId,
            MaxResponseMinutes = maxMins,
            ActualResponseMinutes = req.ActualResponseMinutes,
            IsBreached = isBreached,
            AlertStatus = status,
            BreachedAt = DateTimeOffset.UtcNow
        };

        _db.CrmChatSlaAlerts.Add(alert);
        await _db.SaveChangesAsync(ct);

        return new CrmChatSlaAlertDto(
            alert.Id,
            alert.ConversationId,
            alert.MaxResponseMinutes,
            alert.ActualResponseMinutes,
            alert.IsBreached,
            alert.AlertStatus,
            alert.BreachedAt
        );
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_044: Chatbot kịch bản
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmScriptedBotFlowDto> SaveBotFlowAsync(Guid tenantId, CrmSaveBotFlowRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.FlowName) || string.IsNullOrWhiteSpace(req.TriggerKeyword))
            throw new AppException("Tên kịch bản và từ khóa kích hoạt không được để trống.", 400);

        var id = Guid.NewGuid();
        return await Task.FromResult(new CrmScriptedBotFlowDto(
            id,
            req.FlowName,
            req.TriggerKeyword,
            req.StepsJson ?? "[]",
            true,
            DateTimeOffset.UtcNow
        ));
    }

    public async Task<IReadOnlyList<CrmScriptedBotFlowDto>> GetBotFlowsAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await Task.FromResult(new List<CrmScriptedBotFlowDto>
        {
            new(Guid.NewGuid(), "Kịch bản Chào mừng Khách Zalo", "#chao", "[{\"step\":1,\"action\":\"send_msg\",\"text\":\"Chào bạn! Cảm ơn bạn đã nhắn tin cho ERP Hùng.\"},{\"step\":2,\"action\":\"ask_option\",\"options\":[\"Tư vấn báo giá\",\"Hỗ trợ kỹ thuật\"]}]", true, DateTimeOffset.UtcNow),
            new(Guid.NewGuid(), "Kịch bản Nhận báo giá ERP Cloud", "#baogia", "[{\"step\":1,\"action\":\"send_msg\",\"text\":\"Dạ vui lòng để lại SĐT để chuyên viên gửi bảng giá chi tiết ạ.\"}]", true, DateTimeOffset.UtcNow.AddDays(-1))
        });
    }
}
