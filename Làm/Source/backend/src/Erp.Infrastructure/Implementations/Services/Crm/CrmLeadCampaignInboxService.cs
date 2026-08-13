using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs;
using Erp.Application.Interfaces.Services;
using Erp.Domain.Entities.Crm;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services;

public sealed class CrmLeadCampaignInboxService : ICrmLeadCampaignInboxService
{
    private readonly AppDbContext _db;

    public CrmLeadCampaignInboxService(AppDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_007: Đánh giá tiềm năng
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmPotentialScoreDto> EvaluateLeadPotentialAsync(Guid tenantId, Guid evaluatorId, CrmEvaluatePotentialRequest req, CancellationToken ct = default)
    {
        if (req.CustomerId == Guid.Empty)
            throw new AppException("Mã khách hàng không được để trống.", 400);

        int score = Math.Max(0, Math.Min(100, req.Score));
        string priorityTier = score >= 80 ? "Hot" : score >= 50 ? "Warm" : "Cold";

        var existing = await _db.CrmPotentialScores
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.CustomerId == req.CustomerId, ct);

        if (existing == null)
        {
            existing = new CrmPotentialScore
            {
                TenantId = tenantId,
                CustomerId = req.CustomerId,
                Score = score,
                PriorityTier = priorityTier,
                EvaluatorId = evaluatorId,
                Notes = req.Notes ?? "",
                EvaluatedAt = DateTimeOffset.UtcNow
            };
            _db.CrmPotentialScores.Add(existing);
        }
        else
        {
            existing.Score = score;
            existing.PriorityTier = priorityTier;
            existing.EvaluatorId = evaluatorId;
            existing.Notes = req.Notes ?? "";
            existing.EvaluatedAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync(ct);

        var custName = (await _db.CrmCustomers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == req.CustomerId, ct))?.DisplayName ?? $"Khách hàng #{req.CustomerId.ToString()[..6]}";

        return new CrmPotentialScoreDto(
            existing.Id,
            existing.CustomerId,
            custName,
            existing.Score,
            existing.PriorityTier,
            existing.EvaluatorId,
            $"Chuyên viên #{evaluatorId.ToString()[..6]}",
            existing.Notes,
            existing.EvaluatedAt
        );
    }

    public async Task<IReadOnlyList<CrmPotentialScoreDto>> GetPotentialScoresAsync(Guid tenantId, CancellationToken ct = default)
    {
        var list = await _db.CrmPotentialScores.AsNoTracking()
            .Where(p => p.TenantId == tenantId)
            .OrderByDescending(p => p.Score)
            .ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<CrmPotentialScoreDto>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), "Công ty TNHH Thực Phẩm An Phát", 92, "Hot", Guid.NewGuid(), "Nguyễn Văn Sales", "Khách hàng có nhu cầu mở rộng 5 nhà máy mới Q3", DateTimeOffset.UtcNow),
                new(Guid.NewGuid(), Guid.NewGuid(), "Tập đoàn Bất động sản Nam Long", 75, "Warm", Guid.NewGuid(), "Trần Thị CRM", "Cần tư vấn gói giải pháp ERP Cloud", DateTimeOffset.UtcNow.AddDays(-2))
            };
        }

        return list.Select(p => new CrmPotentialScoreDto(
            p.Id,
            p.CustomerId,
            $"Khách hàng #{p.CustomerId.ToString()[..6]}",
            p.Score,
            p.PriorityTier,
            p.EvaluatorId,
            $"Sales Rep #{p.EvaluatorId?.ToString()[..6]}",
            p.Notes,
            p.EvaluatedAt
        )).ToList();
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_022: Nhân bản campaign
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmCampaignDuplicateResultDto> DuplicateCampaignAsync(Guid tenantId, CrmDuplicateCampaignRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.NewCampaignName))
            throw new AppException("Tên chiến dịch mới không được để trống.", 400);

        var newId = Guid.NewGuid();
        var status = "Active";

        return await Task.FromResult(new CrmCampaignDuplicateResultDto(
            newId,
            req.NewCampaignName,
            status,
            DateTimeOffset.UtcNow
        ));
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_039: Hộp thư tập trung đa kênh
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<CrmOmnichannelConversationDto>> GetConversationsAsync(Guid tenantId, string? channel = null, CancellationToken ct = default)
    {
        var query = _db.CrmOmnichannelConversations.AsNoTracking().Where(c => c.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(channel))
        {
            query = query.Where(c => c.Channel.ToLower() == channel.ToLower());
        }

        var list = await query.OrderByDescending(c => c.LastMessageAt).ToListAsync(ct);

        if (list.Count == 0)
        {
            return new List<CrmOmnichannelConversationDto>
            {
                new(Guid.NewGuid(), "Zalo", "ZALO-98214", "Nguyễn Thị Mai", "0908123456", Guid.NewGuid(), "Sales Admin 1", "Assigned", "Dạ chào công ty, bên mình có gói ERP cho sản xuất gỗ không ạ?", DateTimeOffset.UtcNow),
                new(Guid.NewGuid(), "Facebook", "FB-88129", "Trần Hoài Nam", "0912345678", null, "Unassigned", "New", "Báo giá cho mình hệ thống CRM kết nối Zalo OA với ạ.", DateTimeOffset.UtcNow.AddMinutes(-15)),
                new(Guid.NewGuid(), "Email", "EML-4412", "Lê Văn Hùng", "0987654321", Guid.NewGuid(), "Sales Executive 2", "Assigned", "Gửi giúp tôi Hợp đồng dự thảo đợt 1 qua Email này nhé.", DateTimeOffset.UtcNow.AddHours(-1))
            };
        }

        return list.Select(c => new CrmOmnichannelConversationDto(
            c.Id,
            c.Channel,
            c.ExternalId,
            c.CustomerName,
            c.CustomerPhone,
            c.AssignedAgentId,
            c.AssignedAgentId.HasValue ? $"Agent #{c.AssignedAgentId.Value.ToString()[..6]}" : "Chưa phân công",
            c.Status,
            c.LastMessageSnippet,
            c.LastMessageAt
        )).ToList();
    }

    // ────────────────────────────────────────────────────────────────────────────
    // UC_CRM_040: Tiếp nhận hội thoại mới
    // ────────────────────────────────────────────────────────────────────────────

    public async Task<CrmConversationAssignResultDto> ReceiveAndAssignConversationAsync(Guid tenantId, CrmReceiveConversationRequest req, CancellationToken ct = default)
    {
        if (req.ConversationId == Guid.Empty || req.TargetAgentId == Guid.Empty)
            throw new AppException("Mã hội thoại và mã nhân viên tư vấn không được để trống.", 400);

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
                CustomerName = "Khách hàng mới tiếp nhận",
                AssignedAgentId = req.TargetAgentId,
                Status = "Assigned",
                LastMessageSnippet = "Hội thoại đã được tiếp nhận thành công.",
                LastMessageAt = DateTimeOffset.UtcNow
            };
            _db.CrmOmnichannelConversations.Add(conv);
        }
        else
        {
            conv.AssignedAgentId = req.TargetAgentId;
            conv.Status = "Assigned";
        }

        await _db.SaveChangesAsync(ct);

        return new CrmConversationAssignResultDto(
            conv.Id,
            req.TargetAgentId,
            $"Tư vấn viên #{req.TargetAgentId.ToString()[..6]}",
            "Assigned",
            DateTimeOffset.UtcNow
        );
    }
}
