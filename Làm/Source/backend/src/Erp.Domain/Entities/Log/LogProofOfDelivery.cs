using Erp.Domain.Base;

namespace Erp.Domain.Entities.Log;

/// <summary>Chứng từ ký nhận giao hàng POD (UC_LOG_016).</summary>
public class LogProofOfDelivery : TenantEntity
{
    public Guid DeliveryOrderId { get; set; }
    public string DeliveryOrderNumber { get; set; } = "";
    public string RecipientName { get; set; } = "";
    public string RecipientPhone { get; set; } = "";
    public string SignatureImageUrl { get; set; } = "";
    public string DeliveryPhotoUrl { get; set; } = "";
    public string Notes { get; set; } = "Giao hàng đầy đủ nguyên vẹn";
    public DateTimeOffset SignedAt { get; set; } = DateTimeOffset.UtcNow;
}
