using Erp.Domain.Base;

namespace Erp.Domain.Entities.Log;

/// <summary>Hẹn giao lại (UC_LOG_018).</summary>
public class LogRedeliveryRequest : TenantEntity
{
    public string RequestNumber { get; set; } = "";
    public Guid DeliveryOrderId { get; set; }
    public string OriginalOrderNumber { get; set; } = "";
    public string FailedReason { get; set; } = "Khách hàng bận, hẹn giao lại";
    public DateTimeOffset RescheduledDeliveryDate { get; set; } = DateTimeOffset.UtcNow.AddDays(1);
    public string PreferredShift { get; set; } = "Morning"; // Morning | Afternoon | Evening
    public string Status { get; set; } = "PendingReassignment"; // PendingReassignment | Reassigned | Cancelled
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
}
