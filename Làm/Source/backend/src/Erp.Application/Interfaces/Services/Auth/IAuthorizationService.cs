namespace Erp.Application.Interfaces.Services.Auth;

public interface IAuthorizationService
{
    Task EnsurePermissionAsync(Guid userId, string permissionCode, CancellationToken ct = default);
    Task<bool> HasPermissionAsync(Guid userId, string permissionCode, CancellationToken ct = default);
}
