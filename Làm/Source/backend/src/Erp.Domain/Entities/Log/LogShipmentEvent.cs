using Erp.Domain.Base;

namespace Erp.Domain.Entities.Log;

/// <summary>Lịch sử trạng thái vận đơn (UC_LOG_014, 017).</summary>
public class LogShipmentEvent : TenantEntity
{
    public Guid DeliveryOrderId { get; set; }
    public string Status { get; set; } = "";
    public string? Note { get; set; }
    public Guid ActorUserId { get; set; }
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}
