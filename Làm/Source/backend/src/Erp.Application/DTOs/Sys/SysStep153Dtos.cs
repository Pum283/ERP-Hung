using Erp.Application.DTOs.Auth;

namespace Erp.Application.DTOs.Sys;

// ── UC_SYS_009 SSO ──────────────────────────────────────────────────────────
public sealed record SysSsoProviderDto(
    Guid Id, string Code, string DisplayName, string ClientId, string? AuthorityUrl,
    string RedirectUri, string Scopes, bool JitProvisioning, bool IsActive, string? Note);

public sealed record SysSsoProviderPublicDto(string Code, string DisplayName, string AuthorizeUrl);

public sealed record SysSsoProviderUpsertRequest(
    Guid? Id, string Code, string DisplayName, string ClientId, string? ClientSecret,
    string? AuthorityUrl, string RedirectUri, string? Scopes, bool JitProvisioning, bool IsActive, string? Note);

public sealed record SysSsoStartResponse(string ProviderCode, string State, string AuthorizeUrl);

public sealed record SysSsoCallbackRequest(string ProviderCode, string? Code, string? State, string? Email, string? Subject);

// ── UC_SYS_031 Field permissions ────────────────────────────────────────────
public sealed record SysSensitiveFieldDto(
    Guid Id, string ModuleCode, string EntityName, string FieldKey, string DisplayName,
    string DefaultMask, bool IsActive);

public sealed record SysSensitiveFieldUpsertRequest(
    Guid? Id, string ModuleCode, string EntityName, string FieldKey, string DisplayName,
    string? DefaultMask, bool IsActive);

public sealed record SysRoleFieldPermissionDto(
    Guid Id, Guid RoleId, Guid SensitiveFieldId, string FieldKey, string DisplayName, string Access);

public sealed record SysRoleFieldPermissionUpsertRequest(Guid RoleId, Guid SensitiveFieldId, string Access);

public sealed record SysEffectiveFieldAccessDto(string FieldKey, string Access);

// ── UC_SYS_058 Config versions ──────────────────────────────────────────────
public sealed record SysConfigVersionDto(
    Guid Id, string ConfigKey, string ConfigValue, int VersionNumber,
    string? CommitNote, bool IsCurrent, DateTimeOffset CreatedAt, Guid? CreatedByUserId);

public sealed record SysConfigUpsertVersionedRequest(string Key, string ValueJson, string? CommitNote);

public sealed record SysConfigRollbackRequest(string Key, int VersionNumber, string? CommitNote);

// ── UC_SYS_062 Push ─────────────────────────────────────────────────────────
public sealed record SysPushDeviceDto(
    Guid Id, Guid UserId, string Platform, string DeviceToken, string? AppVersion,
    bool IsValid, DateTimeOffset LastSeenAt);

public sealed record SysPushDeviceRegisterRequest(string Platform, string DeviceToken, string? AppVersion);

public sealed record SysPushTestRequest(Guid? UserId, string Title, string Body);

public sealed record SysPushSendResult(int TargetedDevices, int DeliveredStub, string? LogRef);
