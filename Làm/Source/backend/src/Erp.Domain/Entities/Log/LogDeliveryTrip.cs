using Erp.Domain.Base;

namespace Erp.Domain.Entities.Log;

/// <summary>Gộp nhiều đơn thành chuyến giao hàng (UC_LOG_007).</summary>
public class LogDeliveryTrip : TenantEntity
{
    public string TripNumber { get; set; } = "";
    public Guid DriverVehicleId { get; set; }
    public string DriverName { get; set; } = "";
    public string VehiclePlateNumber { get; set; } = "";
    public string ConsolidatedOrderIdsJson { get; set; } = "[]";
    public int TotalOrdersCount { get; set; }
    public decimal TotalWeightKg { get; set; }
    public string Status { get; set; } = "Planned"; // Planned | InTransit | Completed
    public DateTimeOffset ScheduledDepartureAt { get; set; } = DateTimeOffset.UtcNow;
}
