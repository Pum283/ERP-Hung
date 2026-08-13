using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Đánh giá tiềm năng khách hàng (UC_CRM_007).</summary>
public class CrmPotentialScore : TenantEntity
{
    public Guid CustomerId { get; set; }
    public int Score { get; set; } // 0 - 100
    /// <summary>Hot | Warm | Cold</summary>
    public string PriorityTier { get; set; } = "Warm";
    public Guid? EvaluatorId { get; set; }
    public string Notes { get; set; } = "";
    public DateTimeOffset EvaluatedAt { get; set; } = DateTimeOffset.UtcNow;
}
