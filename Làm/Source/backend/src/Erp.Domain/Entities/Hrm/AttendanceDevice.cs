using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Thiết bị chấm (UC_HRM_101).</summary>
public class AttendanceDevice : TenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    /// <summary>Fingerprint | Qr | App | Other</summary>
    public string DeviceType { get; set; } = "Fingerprint";
    public Guid? OrgUnitId { get; set; }
    public string? SerialNo { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Note { get; set; }
}
