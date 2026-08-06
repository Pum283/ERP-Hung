using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Tích điểm & hạng thành viên POS (UC_POS).</summary>
public class PosLoyaltyPoint : TenantEntity
{
    public Guid CustomerId { get; set; }
    public int PointsEarned { get; set; }
    public int PointsRedeemed { get; set; }
    public int Balance { get; set; }
    /// <summary>Silver · Gold · Platinum · Diamond</summary>
    public string Tier { get; set; } = "Silver";
}
