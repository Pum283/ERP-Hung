using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

/// <summary>UC_SYS_064 — Tùy chọn thông báo cá nhân theo user.</summary>
public class SysUserNotificationPreference : TenantEntity
{
    public Guid UserId { get; set; }
    public bool ChannelInApp { get; set; } = true;
    public bool ChannelEmail { get; set; } = true;
    public bool ChannelSms { get; set; }
    public bool ChannelPush { get; set; } = true;
    /// <summary>Tắt mọi kênh trừ sự kiện bảo mật bắt buộc.</summary>
    public bool MuteAll { get; set; }
    /// <summary>HH:mm — bắt đầu giờ yên lặng (UTC), null = tắt.</summary>
    public string? QuietHoursStart { get; set; }
    /// <summary>HH:mm — kết thúc giờ yên lặng (UTC).</summary>
    public string? QuietHoursEnd { get; set; }
}

/// <summary>UC_SYS_071 — Nhật ký quét virus / bảo mật file.</summary>
public class SysFileScanLog : TenantEntity
{
    public Guid FileObjectId { get; set; }
    /// <summary>Pending | Scanning | Clean | Infected | Error | Skipped</summary>
    public string ScanStatus { get; set; } = "Pending";
    public string? Engine { get; set; }
    public string? ThreatName { get; set; }
    public string? Detail { get; set; }
    public DateTimeOffset ScannedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid? ScannedByUserId { get; set; }
}
