using Erp.Domain.Base;

namespace Erp.Domain.Entities.Log;

/// <summary>Thực thể theo dõi GPS xe vận chuyển realtime (UC_LOG_019).</summary>
public class LogGpsTracking : TenantEntity
{
    public string VehicleCode { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double SpeedKmH { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
