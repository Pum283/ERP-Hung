using Erp.Domain.Base;

namespace Erp.Domain.Entities.Log;

/// <summary>Vị trí GPS realtime của xe vận tải (UC_LOG_019).</summary>
public class LogRealtimeGpsPing : TenantEntity
{
    public Guid DriverVehicleId { get; set; }
    public string VehiclePlateNumber { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double CurrentSpeedKmh { get; set; }
    public string CurrentAddress { get; set; } = "";
    public DateTimeOffset PingedAt { get; set; } = DateTimeOffset.UtcNow;
}
