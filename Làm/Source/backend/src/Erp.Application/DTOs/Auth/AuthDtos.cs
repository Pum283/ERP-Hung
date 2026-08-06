using Erp.Domain.Enums.Sys;

namespace Erp.Application.DTOs.Auth;

public sealed record LoginRequest(string Username, string Password);

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
