namespace Erp.Application.DTOs.Sys;

// ── UC_SYS_093 Theme / branding ─────────────────────────────────────────────
public sealed record SysThemeDto(
    Guid TenantId,
    string TenantName,
    string? LogoUrl,
    string? PrimaryColor,
    string? AccentColor,
    string? FaviconUrl);

public sealed record SysThemeUpsertRequest(
    string? PrimaryColor,
    string? AccentColor,
    string? FaviconUrl);

// ── UC_SYS_094 Role home ────────────────────────────────────────────────────
public sealed record SysRoleHomeDto(
    Guid Id, Guid RoleId, string RoleCode, string RoleName,
    string LandingPath, int Priority, bool IsActive, string? Note);

public sealed record SysRoleHomeUpsertRequest(
    Guid? Id, Guid RoleId, string LandingPath, int Priority, bool IsActive, string? Note);

public sealed record SysMyHomeDto(string LandingPath, string? MatchedRoleCode, int? Priority);

// ── UC_SYS_103 Message search ───────────────────────────────────────────────
public sealed record SysMessageSearchHitDto(
    Guid MessageId,
    Guid ConversationId,
    string? ConversationTitle,
    Guid SenderUserId,
    string SenderDisplayName,
    string BodyPreview,
    DateTimeOffset SentAt);

// ── UC_SYS_104 Conversation mute ────────────────────────────────────────────
public sealed record SysConversationMuteDto(
    Guid ConversationId, bool Muted, DateTimeOffset? MuteUntil, bool EffectivelyMuted);

public sealed record SysConversationMuteRequest(bool Muted, DateTimeOffset? MuteUntil);
