using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Tích điểm & đổi quà (UC_CRM_117).</summary>
public class CrmRewardRedemption : TenantEntity
{
    public Guid CustomerId { get; set; }
    public string RewardItemName { get; set; } = "";
    public int PointsRedeemed { get; set; }
    /// <summary>Pending | Fulfilled | Cancelled</summary>
    public string Status { get; set; } = "Fulfilled";
    public DateTimeOffset RedeemedAt { get; set; } = DateTimeOffset.UtcNow;
}
