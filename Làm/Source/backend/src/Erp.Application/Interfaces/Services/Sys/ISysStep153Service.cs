using Erp.Application.DTOs.Auth;
using Erp.Application.DTOs.Sys;

namespace Erp.Application.Interfaces.Services.Sys;

public interface ISysStep153Service
{
    // SSO
    Task<IReadOnlyList<SysSsoProviderDto>> ListSsoProvidersAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<SysSsoProviderPublicDto>> ListPublicSsoProvidersAsync(Guid tenantId, CancellationToken ct = default);
    Task<SysSsoProviderDto> UpsertSsoProviderAsync(Guid tenantId, Guid userId, SysSsoProviderUpsertRequest req, CancellationToken ct = default);
    Task<SysSsoStartResponse> StartSsoAsync(Guid tenantId, string providerCode, CancellationToken ct = default);
    Task<LoginResponse> CompleteSsoAsync(Guid tenantId, SysSsoCallbackRequest req, string? ip, string? ua, CancellationToken ct = default);

    // Field permissions
    Task<IReadOnlyList<SysSensitiveFieldDto>> ListSensitiveFieldsAsync(Guid tenantId, CancellationToken ct = default);
    Task<SysSensitiveFieldDto> UpsertSensitiveFieldAsync(Guid tenantId, Guid userId, SysSensitiveFieldUpsertRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<SysRoleFieldPermissionDto>> ListRoleFieldPermissionsAsync(Guid tenantId, Guid roleId, CancellationToken ct = default);
    Task<SysRoleFieldPermissionDto> UpsertRoleFieldPermissionAsync(Guid tenantId, Guid userId, SysRoleFieldPermissionUpsertRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<SysEffectiveFieldAccessDto>> GetMyFieldAccessAsync(Guid tenantId, Guid userId, CancellationToken ct = default);
    string ApplyFieldMask(string? rawValue, string access);

    // Config versions
    Task UpsertSettingVersionedAsync(Guid tenantId, Guid userId, SysConfigUpsertVersionedRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<SysConfigVersionDto>> ListConfigVersionsAsync(Guid tenantId, string key, CancellationToken ct = default);
    Task<SysConfigVersionDto> RollbackConfigAsync(Guid tenantId, Guid userId, SysConfigRollbackRequest req, CancellationToken ct = default);

    // Push
    Task<SysPushDeviceDto> RegisterPushDeviceAsync(Guid tenantId, Guid userId, SysPushDeviceRegisterRequest req, CancellationToken ct = default);
    Task RevokePushDeviceAsync(Guid tenantId, Guid userId, Guid deviceId, CancellationToken ct = default);
    Task<IReadOnlyList<SysPushDeviceDto>> ListMyPushDevicesAsync(Guid tenantId, Guid userId, CancellationToken ct = default);
    Task<SysPushSendResult> SendTestPushAsync(Guid tenantId, Guid actorUserId, SysPushTestRequest req, CancellationToken ct = default);
}
