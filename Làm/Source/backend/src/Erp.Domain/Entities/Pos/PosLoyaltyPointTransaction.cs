using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Lịch sử tích điểm / tiêu điểm khách hàng tại quầy POS (UC_POS_051, UC_POS_052).</summary>
public class PosLoyaltyPointTransaction : TenantEntity
{
    public Guid CustomerId { get; set; }
    public Guid OrderId { get; set; }
    /// <summary>Earn | Redeem</summary>
    public string TransactionType { get; set; } = "Earn";
    public int PointsAmount { get; set; }
    public decimal EquivalentValueVnd { get; set; }
    public DateTimeOffset TransactionTime { get; set; } = DateTimeOffset.UtcNow;
}
