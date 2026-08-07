using Erp.Application.DTOs.Auth;
using Erp.Application.DTOs.Sys;

namespace Erp.Application.Interfaces.Services.Auth;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, string? ip, string? ua, CancellationToken ct = default);
    Task<MeResponse> GetMeAsync(Guid userId, CancellationToken ct = default);
    Task LogoutAsync(Guid userId, string? sessionKey, CancellationToken ct = default);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest req, CancellationToken ct = default);
    Task ForgotPasswordAsync(ForgotPasswordRequest req, CancellationToken ct = default);
    Task ResetPasswordWithOtpAsync(ResetPasswordWithOtpRequest req, CancellationToken ct = default);
    Task<Enable2FaResponse> BeginEnable2FaAsync(Guid userId, CancellationToken ct = default);
    Task ConfirmEnable2FaAsync(Guid userId, Verify2FaRequest req, CancellationToken ct = default);
    Task Disable2FaAsync(Guid userId, Verify2FaRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<UserSessionDto>> ListSessionsAsync(Guid userId, CancellationToken ct = default);
    Task RevokeSessionAsync(Guid userId, Guid sessionId, CancellationToken ct = default);
    Task<IReadOnlyList<TrustedDeviceDto>> ListTrustedDevicesAsync(Guid userId, CancellationToken ct = default);
    Task<TrustedDeviceDto> RegisterTrustedDeviceAsync(Guid userId, RegisterTrustedDeviceRequest req, string? ip, CancellationToken ct = default);
    Task RevokeTrustedDeviceAsync(Guid userId, Guid deviceId, CancellationToken ct = default);
}
