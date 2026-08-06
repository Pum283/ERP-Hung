using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

/// <summary>Thực thể thiết bị tin cậy (UC_SYS_012).</summary>
public class SysTrustedDevice : TenantEntity
{
    public Guid UserId { get; set; }
    public string DeviceFingerprint { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTimeOffset LastUsedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddDays(30);
    public bool IsActive { get; set; } = true;
}
