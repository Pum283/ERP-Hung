using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Thực thể AI xếp hạng lead & gợi ý ưu tiên (UC_CRM_097).</summary>
public class CrmAiLeadScore : TenantEntity
{
    public Guid LeadId { get; set; }
    public int Score { get; set; } = 85;
    public string PriorityLevel { get; set; } = "High"; // High | Medium | Low
    public string NextRecommendedAction { get; set; } = string.Empty;
}
