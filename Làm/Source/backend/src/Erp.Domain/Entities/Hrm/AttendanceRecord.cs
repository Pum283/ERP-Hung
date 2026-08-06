using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Bản ghi công theo ngày (UC_HRM_109–119).</summary>
public class AttendanceRecord : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public DateOnly WorkDate { get; set; }
    public DateTimeOffset? CheckInAt { get; set; }
    public DateTimeOffset? CheckOutAt { get; set; }
    /// <summary>App | Qr | Fingerprint | DeviceSync | Manual</summary>
    public string? CheckInMethod { get; set; }
    public string? CheckOutMethod { get; set; }
    public Guid? DeviceId { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int LateMinutes { get; set; }
    public decimal DeductedWorkUnit { get; set; }
    public int OtMinutes { get; set; }
    public decimal WorkUnit { get; set; } = 1m;
    /// <summary>Open | Closed | MissingCheckout | Missing | Adjusted</summary>
    public string Status { get; set; } = "Open";
    public string? Tag { get; set; }
    public string? Note { get; set; }
    public bool IsConfirmed { get; set; }
}
