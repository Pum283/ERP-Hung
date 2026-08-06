using Erp.Domain.Base;

namespace Erp.Domain.Entities.Log;

/// <summary>Tối ưu tuyến đường giao hàng (UC_LOG).</summary>
public class LogRouteOptimization : TenantEntity
{
    public string RouteCode { get; set; } = "";
    public string DriverName { get; set; } = "";
    public string VehiclePlate { get; set; } = "";
    public decimal TotalDistanceKm { get; set; }
    public decimal EstimatedFuelLiters { get; set; }
    public int StopCount { get; set; }
    public string Status { get; set; } = "Planned";
}
