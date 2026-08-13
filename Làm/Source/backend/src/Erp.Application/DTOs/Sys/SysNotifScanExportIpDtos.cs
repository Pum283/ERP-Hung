namespace Erp.Application.DTOs.Sys;

// ── UC_SYS_064 Notification preferences ─────────────────────────────────────
public sealed record SysNotificationPreferenceDto(
    Guid UserId,
    bool ChannelInApp,
    bool ChannelEmail,
    bool ChannelSms,
    bool ChannelPush,
    bool MuteAll,
    string? QuietHoursStart,
    string? QuietHoursEnd);

public sealed record SysNotificationPreferenceUpsertRequest(
    bool ChannelInApp,
    bool ChannelEmail,
    bool ChannelSms,
    bool ChannelPush,
    bool MuteAll,
    string? QuietHoursStart,
    string? QuietHoursEnd);

// ── UC_SYS_071 File virus scan ──────────────────────────────────────────────
public sealed record SysFileScanStatusDto(
    Guid FileObjectId,
    string FileName,
    string ScanStatus,
    DateTimeOffset? ScannedAt,
    string? ThreatName,
    string? Engine);

public sealed record SysFileScanLogDto(
    Guid Id,
    Guid FileObjectId,
    string ScanStatus,
    string? Engine,
    string? ThreatName,
    string? Detail,
    DateTimeOffset ScannedAt);

// ── UC_SYS_077 Bulk export ──────────────────────────────────────────────────
public sealed record SysBulkExportRequest(IReadOnlyList<string> EntityTypes, string? Format);

public sealed record SysBulkExportJobDto(
    Guid Id,
    string JobType,
    string EntityType,
    string? Format,
    string Status,
    int RowCount,
    int ErrorCount,
    string? ErrorDetails,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    Guid? ActorId,
    string? ResultFileName,
    DateTimeOffset? ExpiresAt);

public sealed record SysBulkExportDownloadDto(
    string FileName,
    string ContentType,
    byte[] Content);

// ── UC_SYS_082 IP rules ─────────────────────────────────────────────────────
public sealed record SysIpRuleDto(
    Guid Id, string IpAddressOrCidr, string RuleType, string Description, bool IsActive);

public sealed record SysIpRuleUpsertRequest(
    Guid? Id, string IpAddressOrCidr, string RuleType, string? Description, bool IsActive);

public sealed record SysIpCheckResult(bool Allowed, string Reason);
