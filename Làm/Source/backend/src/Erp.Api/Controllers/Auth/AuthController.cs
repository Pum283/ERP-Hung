using System.Security.Claims;
using Erp.Application.Common.Models;
using Erp.Application.DTOs.Auth;
using Erp.Application.DTOs.Sys;
using Erp.Application.Interfaces.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var ua = Request.Headers.UserAgent.ToString();
        var result = await _auth.LoginAsync(request, ip, ua, ct);
        return Ok(ApiResponse<LoginResponse>.Ok(result));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<MeResponse>>> Me(CancellationToken ct)
        => Ok(ApiResponse<MeResponse>.Ok(await _auth.GetMeAsync(UserId, ct)));

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Logout(CancellationToken ct)
    {
        await _auth.LogoutAsync(UserId, null, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] ChangePasswordRequest req, CancellationToken ct)
    {
        await _auth.ChangePasswordAsync(UserId, req, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> Forgot([FromBody] ForgotPasswordRequest req, CancellationToken ct)
    {
        await _auth.ForgotPasswordAsync(req, ct);
        return Ok(ApiResponse<object>.Ok(new
        {
            ok = true,
            message = "Nếu tài khoản tồn tại, OTP đã được gửi qua Email/SMS (stub: xem IntegrationCallLog).",
        }));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> Reset([FromBody] ResetPasswordWithOtpRequest req, CancellationToken ct)
    {
        await _auth.ResetPasswordWithOtpAsync(req, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    [HttpGet("sessions")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserSessionDto>>>> Sessions(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<UserSessionDto>>.Ok(await _auth.ListSessionsAsync(UserId, ct)));

    [HttpDelete("sessions/{id:guid}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> RevokeSession(Guid id, CancellationToken ct)
    {
        await _auth.RevokeSessionAsync(UserId, id, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    [HttpPost("2fa/begin")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<Enable2FaResponse>>> Begin2Fa(CancellationToken ct)
        => Ok(ApiResponse<Enable2FaResponse>.Ok(await _auth.BeginEnable2FaAsync(UserId, ct)));

    [HttpPost("2fa/confirm")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Confirm2Fa([FromBody] Verify2FaRequest req, CancellationToken ct)
    {
        await _auth.ConfirmEnable2FaAsync(UserId, req, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }

    [HttpPost("2fa/disable")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Disable2Fa([FromBody] Verify2FaRequest req, CancellationToken ct)
    {
        await _auth.Disable2FaAsync(UserId, req, ct);
        return Ok(ApiResponse<object>.Ok(new { ok = true }));
    }
}
