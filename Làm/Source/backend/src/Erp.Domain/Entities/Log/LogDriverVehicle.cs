using Erp.Domain.Base;

namespace Erp.Domain.Entities.Log;

/// <summary>Danh mục tài xế / xe vận tải (UC_LOG_002).</summary>
public class LogDriverVehicle : TenantEntity
{
    public string DriverName { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string DriverLicenseNumber { get; set; } = "";
    public string VehiclePlateNumber { get; set; } = "";
    public string VehicleType { get; set; } = "Truck-2.5T"; // Van | Truck-1.25T | Truck-2.5T | Truck-5T | Container
    public decimal MaxPayloadKg { get; set; } = 2500;
    public bool IsActive { get; set; } = true;
}
