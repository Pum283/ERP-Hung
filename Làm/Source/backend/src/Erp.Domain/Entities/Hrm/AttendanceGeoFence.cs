using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Geo-fence điểm chấm (UC_HRM_102).</summary>
public class AttendanceGeoFence : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid? OrgUnitId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int RadiusMeters { get; set; } = 200;
    public bool IsActive { get; set; } = true;
}
