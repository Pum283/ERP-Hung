using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Đánh giá chỉ số hài lòng CSAT sau hội thoại (UC_CRM_048).</summary>
public class CrmCsatRating : TenantEntity
{
    public Guid ConversationId { get; set; }
    public Guid? AgentId { get; set; }
    public int Score { get; set; } = 5; // 1 to 5 stars
    public string FeedbackText { get; set; } = "";
    public DateTimeOffset RatedAt { get; set; } = DateTimeOffset.UtcNow;
}
