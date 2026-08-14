using Erp.Domain.Base;

namespace Erp.Domain.Entities.Log;

/// <summary>Cấu hình khu vực giao hàng (UC_LOG_004).</summary>
public class LogDeliveryZoneConfig : TenantEntity
{
    public string ZoneCode { get; set; } = "";
    public string ZoneName { get; set; } = "";
    public string CityProvince { get; set; } = "TP. Hồ Chí Minh";
    public string DistrictCoverageListJson { get; set; } = "[]";
    public int EstimatedTransitHours { get; set; } = 4;
    public bool IsActive { get; set; } = true;
}
