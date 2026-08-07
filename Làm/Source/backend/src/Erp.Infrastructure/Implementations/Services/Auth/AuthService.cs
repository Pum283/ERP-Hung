using System.Security.Cryptography;
using System.Text;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Auth;
using Erp.Application.DTOs.Sys;
using Erp.Application.Interfaces.Services.Auth;
using Erp.Application.Interfaces.Services.Sys;
using Erp.Domain.Entities.Sys;
using Erp.Domain.Enums.Sys;
using Erp.Infrastructure.Persistence;
using Erp.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Erp.Infrastructure.Implementations.Services.Auth;

public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwt;
    private readonly IDataScopeService _scope;
    private readonly ISysPlatformService _platform;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _log;

    public AuthService(
        AppDbContext db, IJwtTokenService jwt, IDataScopeService scope,
        ISysPlatformService platform, IConfiguration config, ILogger<AuthService> log)
    {
        _db = db;
        _jwt = jwt;
        _scope = scope;
        _platform = platform;
        _config = config;
        _log = log;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, string? ip, string? ua, CancellationToken ct = default)
    {
        var login = request.Username.Trim();
        var user = await _db.Users.FirstOrDefaultAsync(
            u => !u.IsDeleted && (u.Username == login || u.Email == login), ct);

        async Task Fail(string reason, Guid? tenantId = null, Guid? userId = null)
        {
            if (tenantId is Guid tid)
            {
                _db.LoginAudits.Add(new LoginAudit
                {
                    TenantId = tid, UserId = userId, Username = login, Success = false,
                    IpAddress = ip, UserAgent = ua, FailureReason = reason, AttemptedAt = DateTimeOffset.UtcNow
                });
                await _db.SaveChangesAsync(ct);
            }
            throw new UnauthorizedAppException("Sai tên đăng nhập hoặc mật khẩu.");
        }

        if (user is null || string.IsNullOrEmpty(user.PasswordHash) || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            if (user is not null)
            {
                var policy = await _platform.GetPasswordPolicyAsync(user.TenantId, ct);
                user.FailedLoginCount++;
                if (user.FailedLoginCount >= policy.MaxFailedLogins)
                {
                    user.Status = UserStatus.Locked;
                    user.LockedUntil = DateTimeOffset.UtcNow.AddMinutes(policy.LockMinutes);
                }
                await _db.SaveChangesAsync(ct);
                await Fail("bad_password", user.TenantId, user.Id);
            }
            throw new UnauthorizedAppException("Sai tên đăng nhập hoặc mật khẩu.");
        }

        if (user.Status == UserStatus.Disabled)
            throw new ForbiddenException("Tài khoản đã bị vô hiệu.");

        if (user.Status == UserStatus.Locked || (user.LockedUntil is { } until && until > DateTimeOffset.UtcNow))
            throw new ForbiddenException("Tài khoản đang bị khóa.");

        if (user.LockedUntil is not null && user.LockedUntil <= DateTimeOffset.UtcNow && user.Status == UserStatus.Locked)
        {
            user.Status = UserStatus.Active;
            user.LockedUntil = null;
            user.FailedLoginCount = 0;
        }

        if (user.TotpEnabled)
        {
            if (string.IsNullOrWhiteSpace(request.TwoFactorCode))
                throw new AppException("Tài khoản đã bật 2FA, vui lòng nhập mã TOTP 2FA.", 401);
            if (!VerifyDevTotp(user.TotpSecret ?? "", request.TwoFactorCode))
            {
                await Fail("invalid_2fa", user.TenantId, user.Id);
                throw new AppException("Mã 2FA không đúng.", 401);
            }
        }

        var policyOk = await _platform.GetPasswordPolicyAsync(user.TenantId, ct);
        var maxSessions = 5;
        var activeSessions = await _db.UserSessions.CountAsync(
            x => x.TenantId == user.TenantId && x.UserId == user.Id && !x.IsRevoked && x.ExpiresAt > DateTimeOffset.UtcNow && !x.IsDeleted, ct);
        if (activeSessions >= maxSessions)
        {
            var oldest = await _db.UserSessions
                .Where(x => x.TenantId == user.TenantId && x.UserId == user.Id && !x.IsRevoked && !x.IsDeleted)
                .OrderBy(x => x.LastSeenAt).FirstOrDefaultAsync(ct);
            if (oldest is not null) oldest.IsRevoked = true;
        }

        user.FailedLoginCount = 0;
        user.LastLoginAt = DateTimeOffset.UtcNow;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        var sessionKey = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        var minutes = policyOk.SessionMinutes > 0
            ? policyOk.SessionMinutes
            : _config.GetValue("Jwt:AccessTokenMinutes", 120);
        _db.UserSessions.Add(new UserSession
        {
            TenantId = user.TenantId, UserId = user.Id, SessionKey = sessionKey,
            IpAddress = ip, UserAgent = ua,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(minutes),
            LastSeenAt = DateTimeOffset.UtcNow
        });

        _db.LoginAudits.Add(new LoginAudit
        {
            TenantId = user.TenantId, UserId = user.Id, Username = user.Username, Success = true,
            IpAddress = ip, UserAgent = ua, AttemptedAt = DateTimeOffset.UtcNow
        });

        await _db.SaveChangesAsync(ct);

        var roles = await GetRoleCodesAsync(user.Id, ct);
        var permissions = await GetPermissionCodesAsync(user.Id, ct);
        var scope = await _scope.GetUserScopeContextAsync(user.Id, ct);
        var (token, exp) = _jwt.CreateAccessToken(user.Id, user.TenantId, user.Username, roles);

        return new LoginResponse(
            token, exp, user.Id, user.Username, user.DisplayName,
            roles, permissions, scope.Scope, scope.BypassDataScope);
    }

    public async Task LogoutAsync(Guid userId, string? sessionKey, CancellationToken ct = default)
    {
        var q = _db.UserSessions.Where(x => x.UserId == userId && !x.IsRevoked && !x.IsDeleted);
        if (!string.IsNullOrEmpty(sessionKey))
            q = q.Where(x => x.SessionKey == sessionKey);
        var sessions = await q.ToListAsync(ct);
        foreach (var s in sessions) s.IsRevoked = true;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest req, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, ct)
                   ?? throw new UnauthorizedAppException();
        if (string.IsNullOrEmpty(user.PasswordHash) || !PasswordHasher.Verify(req.CurrentPassword, user.PasswordHash))
            throw new AppException("Mật khẩu hiện tại không đúng.");
        await _platform.ValidatePasswordAsync(user.TenantId, req.NewPassword, ct);
        user.PasswordHash = PasswordHasher.Hash(req.NewPassword);
        user.MustChangePassword = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest req, CancellationToken ct = default)
    {
        var login = req.UsernameOrEmail.Trim();
        var user = await _db.Users.FirstOrDefaultAsync(
            u => !u.IsDeleted && (u.Username == login || u.Email == login), ct);
        if (user is null) return; // không lộ user

        var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        _db.PasswordResetTokens.Add(new PasswordResetToken
        {
            TenantId = user.TenantId,
            UserId = user.Id,
            OtpCode = otp,
            TokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(otp))),
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15)
        });
        await _db.SaveChangesAsync(ct);

        // UC_SYS_004/060/061 — gửi OTP qua channel stub (Email ưu tiên, SMS nếu có SĐT).
        var vars = new Dictionary<string, string>
        {
            ["otp"] = otp,
            ["username"] = user.Username,
            ["displayName"] = user.DisplayName ?? user.Username,
            ["expiresMinutes"] = "15",
        };
        try
        {
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                await _platform.SendChannelMessageAsync(user.TenantId, user.Id, new ChannelSendRequest(
                    "Email", "FORGOT_PASSWORD", user.Email.Trim(), vars, "sys.auth.forgot_password"), ct);
            }
            else if (!string.IsNullOrWhiteSpace(user.Phone))
            {
                await _platform.SendChannelMessageAsync(user.TenantId, user.Id, new ChannelSendRequest(
                    "SMS", "FORGOT_PASSWORD", user.Phone.Trim(), vars, "sys.auth.forgot_password"), ct);
            }
            else
            {
                _log.LogWarning(
                    "Forgot password: user {User} không có email/SĐT — OTP chỉ lưu DB (không gửi channel).",
                    user.Username);
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Forgot password: gửi channel thất bại cho {User}", user.Username);
        }
    }

    public async Task ResetPasswordWithOtpAsync(ResetPasswordWithOtpRequest req, CancellationToken ct = default)
    {
        var login = req.UsernameOrEmail.Trim();
        var user = await _db.Users.FirstOrDefaultAsync(
            u => !u.IsDeleted && (u.Username == login || u.Email == login), ct)
            ?? throw new AppException("OTP không hợp lệ.");
        var token = await _db.PasswordResetTokens
            .Where(x => x.UserId == user.Id && x.UsedAt == null && x.ExpiresAt > DateTimeOffset.UtcNow && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync(ct)
            ?? throw new AppException("OTP không hợp lệ hoặc đã hết hạn.");
        if (token.OtpCode != req.Otp.Trim())
            throw new AppException("OTP không hợp lệ.");
        await _platform.ValidatePasswordAsync(user.TenantId, req.NewPassword, ct);
        user.PasswordHash = PasswordHasher.Hash(req.NewPassword);
        user.MustChangePassword = false;
        user.Status = UserStatus.Active;
        user.LockedUntil = null;
        user.FailedLoginCount = 0;
        token.UsedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Enable2FaResponse> BeginEnable2FaAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstAsync(u => u.Id == userId && !u.IsDeleted, ct);
        var secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(20)).TrimEnd('=').Replace('+', 'A').Replace('/', 'B')[..16];
        user.TotpSecret = secret;
        await _db.SaveChangesAsync(ct);
        var uri = $"otpauth://totp/PumsERP:{user.Username}?secret={secret}&issuer=PumsERP";
        return new Enable2FaResponse(secret, uri);
    }

    public async Task ConfirmEnable2FaAsync(Guid userId, Verify2FaRequest req, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstAsync(u => u.Id == userId && !u.IsDeleted, ct);
        if (string.IsNullOrEmpty(user.TotpSecret)) throw new AppException("Chưa khởi tạo 2FA.");
        if (!VerifyDevTotp(user.TotpSecret, req.Code)) throw new AppException("Mã 2FA không đúng.");
        user.TotpEnabled = true;
        await _db.SaveChangesAsync(ct);
    }

    public async Task Disable2FaAsync(Guid userId, Verify2FaRequest req, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstAsync(u => u.Id == userId && !u.IsDeleted, ct);
        if (user.TotpEnabled && !VerifyDevTotp(user.TotpSecret ?? "", req.Code))
            throw new AppException("Mã 2FA không đúng.");
        user.TotpEnabled = false;
        user.TotpSecret = null;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<UserSessionDto>> ListSessionsAsync(Guid userId, CancellationToken ct = default)
        => await _db.UserSessions.AsNoTracking()
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .OrderByDescending(x => x.LastSeenAt)
            .Select(x => new UserSessionDto(x.Id, x.SessionKey, x.IpAddress, x.UserAgent, x.LastSeenAt, x.ExpiresAt, x.IsRevoked))
            .ToListAsync(ct);

    public async Task RevokeSessionAsync(Guid userId, Guid sessionId, CancellationToken ct = default)
    {
        var s = await _db.UserSessions.FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId, ct)
                ?? throw new AppException("Phiên không tồn tại.", 404);
        s.IsRevoked = true;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<MeResponse> GetMeAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, ct)
            ?? throw new UnauthorizedAppException();

        var roles = await GetRoleCodesAsync(userId, ct);
        var permissions = await GetPermissionCodesAsync(userId, ct);
        var scope = await _scope.GetUserScopeContextAsync(userId, ct);

        var modules = await (
            from l in _db.Licenses.AsNoTracking()
            join lm in _db.LicenseModules.AsNoTracking() on l.Id equals lm.LicenseId
            where l.TenantId == user.TenantId && l.Status == "Active" && lm.IsEnabled && !lm.IsDeleted
            select lm.ModuleCode
        ).Distinct().ToListAsync(ct);

        var tenant = await _db.Tenants.AsNoTracking()
            .Where(t => t.Id == user.TenantId && !t.IsDeleted)
            .Select(t => new { t.Name, t.LogoUrl })
            .FirstOrDefaultAsync(ct);

        return new MeResponse(
            user.Id, user.TenantId, user.Username, user.DisplayName, user.Email,
            user.DepartmentId, user.JobLevelId, roles, permissions,
            scope.Scope, scope.BypassDataScope, modules,
            tenant?.LogoUrl, tenant?.Name);
    }

    public async Task<IReadOnlyList<TrustedDeviceDto>> ListTrustedDevicesAsync(Guid userId, CancellationToken ct = default)
    {
        var devices = await _db.TrustedDevices.AsNoTracking()
            .Where(x => x.UserId == userId && x.IsActive && x.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderByDescending(x => x.LastUsedAt)
            .ToListAsync(ct);

        return devices.Select(d => new TrustedDeviceDto(
            d.Id, d.DeviceFingerprint, d.DeviceName, d.IpAddress, d.LastUsedAt, d.ExpiresAt, d.IsActive)).ToList();
    }

    public async Task<TrustedDeviceDto> RegisterTrustedDeviceAsync(Guid userId, RegisterTrustedDeviceRequest req, string? ip, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new AppException("Người dùng không tồn tại.");

        var fp = (req.DeviceFingerprint ?? "").Trim();
        if (string.IsNullOrEmpty(fp)) throw new AppException("Mã định danh thiết bị không hợp lệ.");

        var existing = await _db.TrustedDevices.FirstOrDefaultAsync(x => x.UserId == userId && x.DeviceFingerprint == fp, ct);
        if (existing != null)
        {
            existing.DeviceName = string.IsNullOrWhiteSpace(req.DeviceName) ? existing.DeviceName : req.DeviceName.Trim();
            existing.IpAddress = ip ?? existing.IpAddress;
            existing.LastUsedAt = DateTimeOffset.UtcNow;
            existing.ExpiresAt = DateTimeOffset.UtcNow.AddDays(30);
            existing.IsActive = true;
            await _db.SaveChangesAsync(ct);
            return new TrustedDeviceDto(existing.Id, existing.DeviceFingerprint, existing.DeviceName, existing.IpAddress, existing.LastUsedAt, existing.ExpiresAt, existing.IsActive);
        }

        var device = new SysTrustedDevice
        {
            TenantId = user.TenantId,
            UserId = userId,
            DeviceFingerprint = fp,
            DeviceName = string.IsNullOrWhiteSpace(req.DeviceName) ? "Trình duyệt tin cậy" : req.DeviceName.Trim(),
            IpAddress = ip ?? "127.0.0.1",
            LastUsedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30),
            IsActive = true
        };
        _db.TrustedDevices.Add(device);
        await _db.SaveChangesAsync(ct);

        return new TrustedDeviceDto(device.Id, device.DeviceFingerprint, device.DeviceName, device.IpAddress, device.LastUsedAt, device.ExpiresAt, device.IsActive);
    }

    public async Task RevokeTrustedDeviceAsync(Guid userId, Guid deviceId, CancellationToken ct = default)
    {
        var device = await _db.TrustedDevices.FirstOrDefaultAsync(x => x.Id == deviceId && x.UserId == userId, ct);
        if (device != null)
        {
            device.IsActive = false;
            await _db.SaveChangesAsync(ct);
        }
    }

    /// <summary>Dev 2FA: chấp nhận 6 số cuối secret hash theo phút UTC, hoặc mã "000000".</summary>
    private static bool VerifyDevTotp(string secret, string code)
    {
        if (code == "000000") return true;
        var slot = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
        using var h = new HMACSHA1(Encoding.UTF8.GetBytes(secret));
        var hash = h.ComputeHash(BitConverter.GetBytes(slot));
        var expected = (BitConverter.ToUInt32(hash, hash.Length - 4) % 1000000).ToString("D6");
        return code.Trim() == expected;
    }

    private async Task<IReadOnlyList<string>> GetRoleCodesAsync(Guid userId, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        return await (
            from ur in _db.UserRoles.AsNoTracking()
            join r in _db.Roles.AsNoTracking() on ur.RoleId equals r.Id
            where ur.UserId == userId
                  && ur.IsActive && !ur.IsDeleted && ur.RevokedAt == null
                  && (ur.ValidFrom == null || ur.ValidFrom <= now)
                  && (ur.ValidTo == null || ur.ValidTo >= now)
                  && r.IsActive && !r.IsDeleted
            select r.Code
        ).Distinct().ToListAsync(ct);
    }

    private async Task<IReadOnlyList<string>> GetPermissionCodesAsync(Guid userId, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        // Digi: BypassDataScope role → trả về toàn bộ permission active (để FE không cần hardcode)
        var bypass = await (
            from ur in _db.UserRoles.AsNoTracking()
            join r in _db.Roles.AsNoTracking() on ur.RoleId equals r.Id
            where ur.UserId == userId
                  && ur.IsActive && !ur.IsDeleted && ur.RevokedAt == null
                  && (ur.ValidFrom == null || ur.ValidFrom <= now)
                  && (ur.ValidTo == null || ur.ValidTo >= now)
                  && r.IsActive && !r.IsDeleted && r.BypassDataScope
            select r.Id).AnyAsync(ct);

        if (bypass)
            return await _db.Permissions.AsNoTracking()
                .Where(p => p.IsActive && !p.IsDeleted)
                .Select(p => p.Code)
                .OrderBy(c => c)
                .ToListAsync(ct);

        return await (
            from ur in _db.UserRoles.AsNoTracking()
            join rp in _db.RolePermissions.AsNoTracking() on ur.RoleId equals rp.RoleId
            join p in _db.Permissions.AsNoTracking() on rp.PermissionId equals p.Id
            where ur.UserId == userId
                  && ur.IsActive && !ur.IsDeleted && ur.RevokedAt == null
                  && (ur.ValidFrom == null || ur.ValidFrom <= now)
                  && (ur.ValidTo == null || ur.ValidTo >= now)
                  && !rp.IsDeleted && p.IsActive && !p.IsDeleted
            select p.Code
        ).Distinct().ToListAsync(ct);
    }
}
