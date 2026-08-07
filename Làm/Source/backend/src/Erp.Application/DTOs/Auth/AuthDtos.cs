using Erp.Domain.Enums.Sys;

namespace Erp.Application.DTOs.Auth;

public sealed record LoginRequest(string Username, string Password, string? TwoFactorCode = null);

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    Guid UserId,
    string Username,
    string? DisplayName,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,
    ScopeType EffectiveScopeType,
    bool BypassDataScope);

public sealed record MeResponse(
    Guid UserId,
    Guid TenantId,
    string Username,
    string? DisplayName,
    string? Email,
    Guid? DepartmentId,
    Guid? JobLevelId,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,
    ScopeType EffectiveScopeType,
    bool BypassDataScope,
    IReadOnlyList<string> EnabledModules,
    string? TenantLogoUrl,
    string? TenantName);

public sealed record TrustedDeviceDto(
    Guid Id,
    string DeviceFingerprint,
    string DeviceName,
    string IpAddress,
    DateTimeOffset LastUsedAt,
    DateTimeOffset ExpiresAt,
    bool IsActive);

public sealed record RegisterTrustedDeviceRequest(
    string DeviceFingerprint,
    string DeviceName);
