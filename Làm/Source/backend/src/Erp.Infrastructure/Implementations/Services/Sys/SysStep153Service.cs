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

namespace Erp.Infrastructure.Implementations.Services.Sys;

/// <summary>
/// Bước 153 — UC_SYS_009 (SSO), UC_SYS_031 (field ACL), UC_SYS_058 (config versions), UC_SYS_062 (push).
/// </summary>
public sealed class SysStep153Service : ISysStep153Service
{
    private static readonly string[] AccessOrder = ["None", "Masked", "Read", "Write"];
    private static readonly HashSet<string> AllowedAccess = new(AccessOrder, StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> Platforms = new(StringComparer.OrdinalIgnoreCase) { "Fcm", "Apns", "Web" };

    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwt;
    private readonly IDataScopeService _scope;
    private readonly ISysPlatformService _platform;
    private readonly ISysStep154Service _step154;
    private readonly IConfiguration _config;

    public SysStep153Service(
        AppDbContext db,
        IJwtTokenService jwt,
        IDataScopeService scope,
        ISysPlatformService platform,
        ISysStep154Service step154,
        IConfiguration config)
    {
        _db = db;
        _jwt = jwt;
        _scope = scope;
        _platform = platform;
        _step154 = step154;
        _config = config;
    }

    // ── SSO ─────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<SysSsoProviderDto>> ListSsoProvidersAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.SysSsoProviders.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.Code)
            .Select(x => MapProvider(x))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<SysSsoProviderPublicDto>> ListPublicSsoProvidersAsync(Guid tenantId, CancellationToken ct = default)
    {
        var providers = await _db.SysSsoProviders.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.DisplayName)
            .ToListAsync(ct);
        return providers.Select(p => new SysSsoProviderPublicDto(
            p.Code, p.DisplayName, BuildAuthorizeUrl(p, "public-preview"))).ToList();
    }

    public async Task<SysSsoProviderDto> UpsertSsoProviderAsync(
        Guid tenantId, Guid userId, SysSsoProviderUpsertRequest req, CancellationToken ct = default)
    {
        var code = (req.Code ?? "").Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code) || code.Length > 40)
            throw new AppException("Mã IdP bắt buộc, tối đa 40 ký tự.");
        if (string.IsNullOrWhiteSpace(req.DisplayName))
            throw new AppException("Tên hiển thị IdP bắt buộc.");
        if (string.IsNullOrWhiteSpace(req.ClientId))
            throw new AppException("ClientId bắt buộc.");
        if (string.IsNullOrWhiteSpace(req.RedirectUri))
            throw new AppException("RedirectUri bắt buộc.");

        SysSsoProvider entity;
        if (req.Id is Guid id)
        {
            entity = await _db.SysSsoProviders.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                     ?? throw new AppException("IdP không tồn tại.", 404);
            if (!string.Equals(entity.Code, code, StringComparison.OrdinalIgnoreCase))
            {
                var clash = await _db.SysSsoProviders.AnyAsync(
                    x => x.TenantId == tenantId && x.Code == code && x.Id != id && !x.IsDeleted, ct);
                if (clash) throw new AppException($"Mã IdP '{code}' đã tồn tại.");
                entity.Code = code;
            }
        }
        else
        {
            var clash = await _db.SysSsoProviders.AnyAsync(
                x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct);
            if (clash) throw new AppException($"Mã IdP '{code}' đã tồn tại.");
            entity = new SysSsoProvider { TenantId = tenantId, Code = code, CreatedBy = userId };
            _db.SysSsoProviders.Add(entity);
        }

        entity.DisplayName = req.DisplayName.Trim();
        entity.ClientId = req.ClientId.Trim();
        if (!string.IsNullOrWhiteSpace(req.ClientSecret))
            entity.ClientSecret = req.ClientSecret.Trim();
        entity.AuthorityUrl = string.IsNullOrWhiteSpace(req.AuthorityUrl) ? null : req.AuthorityUrl.Trim();
        entity.RedirectUri = req.RedirectUri.Trim();
        entity.Scopes = string.IsNullOrWhiteSpace(req.Scopes) ? "openid profile email" : req.Scopes.Trim();
        entity.JitProvisioning = req.JitProvisioning;
        entity.IsActive = req.IsActive;
        entity.Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim();
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapProvider(entity);
    }

    public async Task<SysSsoStartResponse> StartSsoAsync(Guid tenantId, string providerCode, CancellationToken ct = default)
    {
        var code = (providerCode ?? "").Trim().ToUpperInvariant();
        var p = await _db.SysSsoProviders.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Code == code && !x.IsDeleted, ct)
                ?? throw new AppException("IdP không tồn tại.", 404);
        if (!p.IsActive) throw new AppException("IdP đang tắt.");

        var state = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        return new SysSsoStartResponse(p.Code, state, BuildAuthorizeUrl(p, state));
    }

    public async Task<LoginResponse> CompleteSsoAsync(
        Guid tenantId, SysSsoCallbackRequest req, string? ip, string? ua, CancellationToken ct = default)
    {
        var providerCode = (req.ProviderCode ?? "").Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(providerCode))
            throw new AppException("ProviderCode bắt buộc.");

        var provider = await _db.SysSsoProviders
                           .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Code == providerCode && !x.IsDeleted, ct)
                       ?? throw new AppException("IdP không tồn tại.", 404);
        if (!provider.IsActive) throw new AppException("IdP đang tắt.");

        // Day-1: chấp nhận code dạng "dev:{email}|{subject}" hoặc body Email/Subject trực tiếp.
        var (email, subject) = ResolveSsoIdentity(req);
        if (string.IsNullOrWhiteSpace(subject))
            throw new AppException("Thiếu subject từ IdP (dùng code=dev:email|subject hoặc Subject).");
        if (string.IsNullOrWhiteSpace(email))
            throw new AppException("Thiếu email từ IdP.");

        email = email.Trim().ToLowerInvariant();
        subject = subject.Trim();

        var link = await _db.SysExternalLogins
            .FirstOrDefaultAsync(x =>
                x.TenantId == tenantId && x.ProviderCode == providerCode &&
                x.ProviderSubject == subject && !x.IsDeleted, ct);

        AppUser user;
        if (link is not null)
        {
            user = await _db.Users.FirstOrDefaultAsync(u => u.Id == link.UserId && !u.IsDeleted, ct)
                   ?? throw new AppException("Tài khoản liên kết không còn tồn tại.", 404);
            link.Email = email;
            link.UpdatedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            var byEmail = await _db.Users.FirstOrDefaultAsync(
                u => u.TenantId == tenantId && u.Email != null && u.Email.ToLower() == email && !u.IsDeleted, ct);

            if (byEmail is null)
            {
                if (!provider.JitProvisioning)
                    throw new UnauthorizedAppException("Chưa liên kết tài khoản SSO và JIT đang tắt.");

                var baseUser = email.Split('@')[0];
                var username = await UniqueUsernameAsync(tenantId, baseUser, ct);
                user = new AppUser
                {
                    TenantId = tenantId,
                    Username = username,
                    Email = email,
                    DisplayName = email,
                    Status = UserStatus.Active,
                    MustChangePassword = false,
                    PasswordHash = null
                };
                _db.Users.Add(user);
                await _db.SaveChangesAsync(ct);
            }
            else
            {
                user = byEmail;
                var other = await _db.SysExternalLogins.AnyAsync(x =>
                    x.TenantId == tenantId && x.ProviderCode == providerCode &&
                    x.UserId == user.Id && !x.IsDeleted, ct);
                if (other)
                    throw new AppException("Email đã liên kết IdP này với subject khác.");
            }

            _db.SysExternalLogins.Add(new SysExternalLogin
            {
                TenantId = tenantId,
                UserId = user.Id,
                ProviderCode = providerCode,
                ProviderSubject = subject,
                Email = email,
                LinkedAt = DateTimeOffset.UtcNow
            });
        }

        if (user.Status == UserStatus.Disabled)
            throw new ForbiddenException("Tài khoản đã bị vô hiệu.");
        if (user.Status == UserStatus.Locked || (user.LockedUntil is { } until && until > DateTimeOffset.UtcNow))
            throw new ForbiddenException("Tài khoản đang bị khóa.");

        await _step154.EnsureIpAllowedAsync(tenantId, ip, ct);

        var policyOk = await _platform.GetPasswordPolicyAsync(user.TenantId, ct);
        var maxSessions = 5;
        var activeSessions = await _db.UserSessions.CountAsync(
            x => x.TenantId == user.TenantId && x.UserId == user.Id && !x.IsRevoked &&
                 x.ExpiresAt > DateTimeOffset.UtcNow && !x.IsDeleted, ct);
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
            IpAddress = ip, UserAgent = ua, AttemptedAt = DateTimeOffset.UtcNow,
            FailureReason = $"sso:{providerCode}"
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

    // ── Field permissions ───────────────────────────────────────────────────

    public async Task<IReadOnlyList<SysSensitiveFieldDto>> ListSensitiveFieldsAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.SysSensitiveFields.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.ModuleCode).ThenBy(x => x.EntityName).ThenBy(x => x.FieldKey)
            .Select(x => new SysSensitiveFieldDto(
                x.Id, x.ModuleCode, x.EntityName, x.FieldKey, x.DisplayName, x.DefaultMask, x.IsActive))
            .ToListAsync(ct);

    public async Task<SysSensitiveFieldDto> UpsertSensitiveFieldAsync(
        Guid tenantId, Guid userId, SysSensitiveFieldUpsertRequest req, CancellationToken ct = default)
    {
        var module = (req.ModuleCode ?? "SYS").Trim().ToUpperInvariant();
        var entityName = (req.EntityName ?? "").Trim();
        var fieldKey = (req.FieldKey ?? "").Trim();
        if (string.IsNullOrWhiteSpace(entityName) || string.IsNullOrWhiteSpace(fieldKey))
            throw new AppException("EntityName và FieldKey bắt buộc.");
        if (string.IsNullOrWhiteSpace(req.DisplayName))
            throw new AppException("DisplayName bắt buộc.");

        var mask = string.IsNullOrWhiteSpace(req.DefaultMask) ? "Mask" : req.DefaultMask.Trim();
        if (mask is not ("Hide" or "Mask" or "ReadOnly"))
            throw new AppException("DefaultMask phải là Hide|Mask|ReadOnly.");

        SysSensitiveField entity;
        if (req.Id is Guid id)
        {
            entity = await _db.SysSensitiveFields.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                     ?? throw new AppException("Trường nhạy cảm không tồn tại.", 404);
        }
        else
        {
            var clash = await _db.SysSensitiveFields.AnyAsync(x =>
                x.TenantId == tenantId && x.ModuleCode == module && x.EntityName == entityName &&
                x.FieldKey == fieldKey && !x.IsDeleted, ct);
            if (clash) throw new AppException("Trường nhạy cảm đã tồn tại.");
            entity = new SysSensitiveField
            {
                TenantId = tenantId, ModuleCode = module, EntityName = entityName, FieldKey = fieldKey,
                CreatedBy = userId
            };
            _db.SysSensitiveFields.Add(entity);
        }

        entity.ModuleCode = module;
        entity.EntityName = entityName;
        entity.FieldKey = fieldKey;
        entity.DisplayName = req.DisplayName.Trim();
        entity.DefaultMask = mask;
        entity.IsActive = req.IsActive;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new SysSensitiveFieldDto(
            entity.Id, entity.ModuleCode, entity.EntityName, entity.FieldKey,
            entity.DisplayName, entity.DefaultMask, entity.IsActive);
    }

    public async Task<IReadOnlyList<SysRoleFieldPermissionDto>> ListRoleFieldPermissionsAsync(
        Guid tenantId, Guid roleId, CancellationToken ct = default)
    {
        return await (
            from p in _db.SysRoleFieldPermissions.AsNoTracking()
            join f in _db.SysSensitiveFields.AsNoTracking() on p.SensitiveFieldId equals f.Id
            where p.TenantId == tenantId && p.RoleId == roleId && !p.IsDeleted && !f.IsDeleted
            orderby f.FieldKey
            select new SysRoleFieldPermissionDto(p.Id, p.RoleId, p.SensitiveFieldId, f.FieldKey, f.DisplayName, p.Access)
        ).ToListAsync(ct);
    }

    public async Task<SysRoleFieldPermissionDto> UpsertRoleFieldPermissionAsync(
        Guid tenantId, Guid userId, SysRoleFieldPermissionUpsertRequest req, CancellationToken ct = default)
    {
        var access = (req.Access ?? "").Trim();
        if (!AllowedAccess.Contains(access))
            throw new AppException("Access phải là None|Masked|Read|Write.");
        access = AccessOrder.First(a => a.Equals(access, StringComparison.OrdinalIgnoreCase));

        var roleOk = await _db.Roles.AnyAsync(r => r.Id == req.RoleId && r.TenantId == tenantId && !r.IsDeleted, ct);
        if (!roleOk) throw new AppException("Vai trò không tồn tại.", 404);

        var field = await _db.SysSensitiveFields.FirstOrDefaultAsync(
                        f => f.Id == req.SensitiveFieldId && f.TenantId == tenantId && !f.IsDeleted, ct)
                    ?? throw new AppException("Trường nhạy cảm không tồn tại.", 404);

        var entity = await _db.SysRoleFieldPermissions.FirstOrDefaultAsync(x =>
            x.TenantId == tenantId && x.RoleId == req.RoleId && x.SensitiveFieldId == req.SensitiveFieldId && !x.IsDeleted, ct);
        if (entity is null)
        {
            entity = new SysRoleFieldPermission
            {
                TenantId = tenantId, RoleId = req.RoleId, SensitiveFieldId = req.SensitiveFieldId,
                CreatedBy = userId
            };
            _db.SysRoleFieldPermissions.Add(entity);
        }

        entity.Access = access;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new SysRoleFieldPermissionDto(
            entity.Id, entity.RoleId, entity.SensitiveFieldId, field.FieldKey, field.DisplayName, entity.Access);
    }

    public async Task<IReadOnlyList<SysEffectiveFieldAccessDto>> GetMyFieldAccessAsync(
        Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var roleIds = await (
            from ur in _db.UserRoles.AsNoTracking()
            join r in _db.Roles.AsNoTracking() on ur.RoleId equals r.Id
            where ur.UserId == userId && ur.TenantId == tenantId
                  && ur.IsActive && !ur.IsDeleted && ur.RevokedAt == null
                  && (ur.ValidFrom == null || ur.ValidFrom <= now)
                  && (ur.ValidTo == null || ur.ValidTo >= now)
                  && r.IsActive && !r.IsDeleted
            select r.Id
        ).Distinct().ToListAsync(ct);

        var fields = await _db.SysSensitiveFields.AsNoTracking()
            .Where(f => f.TenantId == tenantId && f.IsActive && !f.IsDeleted)
            .ToListAsync(ct);

        var perms = await _db.SysRoleFieldPermissions.AsNoTracking()
            .Where(p => p.TenantId == tenantId && roleIds.Contains(p.RoleId) && !p.IsDeleted)
            .ToListAsync(ct);

        var result = new List<SysEffectiveFieldAccessDto>();
        foreach (var f in fields)
        {
            var roleAccess = perms.Where(p => p.SensitiveFieldId == f.Id).Select(p => p.Access).ToList();
            var effective = roleAccess.Count == 0
                ? MapDefaultToAccess(f.DefaultMask)
                : roleAccess.OrderByDescending(RankAccess).First();
            result.Add(new SysEffectiveFieldAccessDto(f.FieldKey, effective));
        }
        return result.OrderBy(x => x.FieldKey).ToList();
    }

    public string ApplyFieldMask(string? rawValue, string access)
    {
        var a = (access ?? "None").Trim();
        if (a.Equals("None", StringComparison.OrdinalIgnoreCase) || a.Equals("Hide", StringComparison.OrdinalIgnoreCase))
            return "••••";
        if (a.Equals("Masked", StringComparison.OrdinalIgnoreCase) || a.Equals("Mask", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(rawValue)) return "";
            if (rawValue.Length <= 4) return new string('•', rawValue.Length);
            return rawValue[..2] + new string('•', Math.Max(2, rawValue.Length - 4)) + rawValue[^2..];
        }
        return rawValue ?? "";
    }

    // ── Config versions ─────────────────────────────────────────────────────

    public async Task UpsertSettingVersionedAsync(
        Guid tenantId, Guid userId, SysConfigUpsertVersionedRequest req, CancellationToken ct = default)
    {
        var key = (req.Key ?? "").Trim();
        if (string.IsNullOrWhiteSpace(key)) throw new AppException("ConfigKey bắt buộc.");
        if (key.Length > 100) throw new AppException("ConfigKey tối đa 100 ký tự.");

        await _platform.UpsertSettingAsync(tenantId, key, req.ValueJson ?? "", ct);

        var current = await _db.SysConfigVersions
            .Where(x => x.TenantId == tenantId && x.ConfigKey == key && !x.IsDeleted)
            .OrderByDescending(x => x.VersionNumber)
            .FirstOrDefaultAsync(ct);

        if (current is not null)
            current.IsCurrent = false;

        var next = (current?.VersionNumber ?? 0) + 1;
        _db.SysConfigVersions.Add(new SysConfigVersion
        {
            TenantId = tenantId,
            ConfigKey = key,
            ConfigValue = req.ValueJson ?? "",
            VersionNumber = next,
            CommitNote = string.IsNullOrWhiteSpace(req.CommitNote) ? null : req.CommitNote.Trim(),
            IsCurrent = true,
            CreatedByUserId = userId,
            CreatedBy = userId
        });
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<SysConfigVersionDto>> ListConfigVersionsAsync(
        Guid tenantId, string key, CancellationToken ct = default)
    {
        var k = (key ?? "").Trim();
        if (string.IsNullOrWhiteSpace(k)) throw new AppException("ConfigKey bắt buộc.");
        return await _db.SysConfigVersions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ConfigKey == k && !x.IsDeleted)
            .OrderByDescending(x => x.VersionNumber)
            .Select(x => new SysConfigVersionDto(
                x.Id, x.ConfigKey, x.ConfigValue, x.VersionNumber,
                x.CommitNote, x.IsCurrent, x.CreatedAt, x.CreatedByUserId))
            .ToListAsync(ct);
    }

    public async Task<SysConfigVersionDto> RollbackConfigAsync(
        Guid tenantId, Guid userId, SysConfigRollbackRequest req, CancellationToken ct = default)
    {
        var key = (req.Key ?? "").Trim();
        if (string.IsNullOrWhiteSpace(key)) throw new AppException("ConfigKey bắt buộc.");
        if (req.VersionNumber <= 0) throw new AppException("VersionNumber phải > 0.");

        var target = await _db.SysConfigVersions.AsNoTracking()
                         .FirstOrDefaultAsync(x =>
                             x.TenantId == tenantId && x.ConfigKey == key &&
                             x.VersionNumber == req.VersionNumber && !x.IsDeleted, ct)
                     ?? throw new AppException("Phiên bản cấu hình không tồn tại.", 404);

        var note = string.IsNullOrWhiteSpace(req.CommitNote)
            ? $"Rollback to v{req.VersionNumber}"
            : req.CommitNote.Trim();
        await UpsertSettingVersionedAsync(tenantId, userId,
            new SysConfigUpsertVersionedRequest(key, target.ConfigValue, note), ct);

        return (await ListConfigVersionsAsync(tenantId, key, ct)).First(x => x.IsCurrent);
    }

    // ── Push ────────────────────────────────────────────────────────────────

    public async Task<SysPushDeviceDto> RegisterPushDeviceAsync(
        Guid tenantId, Guid userId, SysPushDeviceRegisterRequest req, CancellationToken ct = default)
    {
        var platform = (req.Platform ?? "").Trim();
        if (!Platforms.Contains(platform))
            throw new AppException("Platform phải là Fcm|Apns|Web.");
        platform = Platforms.First(p => p.Equals(platform, StringComparison.OrdinalIgnoreCase));

        var token = (req.DeviceToken ?? "").Trim();
        if (string.IsNullOrWhiteSpace(token) || token.Length < 8)
            throw new AppException("DeviceToken bắt buộc, tối thiểu 8 ký tự.");
        if (token.Length > 500) throw new AppException("DeviceToken tối đa 500 ký tự.");

        var existing = await _db.SysPushDevices
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.DeviceToken == token && !x.IsDeleted, ct);

        if (existing is not null)
        {
            existing.UserId = userId;
            existing.Platform = platform;
            existing.AppVersion = string.IsNullOrWhiteSpace(req.AppVersion) ? existing.AppVersion : req.AppVersion.Trim();
            existing.IsValid = true;
            existing.LastSeenAt = DateTimeOffset.UtcNow;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            existing.UpdatedBy = userId;
            await _db.SaveChangesAsync(ct);
            return MapDevice(existing);
        }

        var entity = new SysPushDevice
        {
            TenantId = tenantId,
            UserId = userId,
            Platform = platform,
            DeviceToken = token,
            AppVersion = string.IsNullOrWhiteSpace(req.AppVersion) ? null : req.AppVersion.Trim(),
            IsValid = true,
            LastSeenAt = DateTimeOffset.UtcNow,
            CreatedBy = userId
        };
        _db.SysPushDevices.Add(entity);
        await _db.SaveChangesAsync(ct);
        return MapDevice(entity);
    }

    public async Task RevokePushDeviceAsync(Guid tenantId, Guid userId, Guid deviceId, CancellationToken ct = default)
    {
        var entity = await _db.SysPushDevices.FirstOrDefaultAsync(
                         x => x.Id == deviceId && x.TenantId == tenantId && x.UserId == userId && !x.IsDeleted, ct)
                     ?? throw new AppException("Thiết bị push không tồn tại.", 404);
        entity.IsValid = false;
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<SysPushDeviceDto>> ListMyPushDevicesAsync(
        Guid tenantId, Guid userId, CancellationToken ct = default)
        => await _db.SysPushDevices.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UserId == userId && x.IsValid && !x.IsDeleted)
            .OrderByDescending(x => x.LastSeenAt)
            .Select(x => MapDevice(x))
            .ToListAsync(ct);

    public async Task<SysPushSendResult> SendTestPushAsync(
        Guid tenantId, Guid actorUserId, SysPushTestRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Title)) throw new AppException("Title bắt buộc.");
        if (string.IsNullOrWhiteSpace(req.Body)) throw new AppException("Body bắt buộc.");

        var targetUserId = req.UserId ?? actorUserId;
        var devices = await _db.SysPushDevices
            .Where(x => x.TenantId == tenantId && x.UserId == targetUserId && x.IsValid && !x.IsDeleted)
            .ToListAsync(ct);

        var delivered = 0;
        foreach (var d in devices)
        {
            // Stub FCM/APNs — ghi IntegrationCallLog, không gọi mạng thật.
            _db.IntegrationCallLogs.Add(new IntegrationCallLog
            {
                TenantId = tenantId,
                Kind = "Push",
                Target = $"{d.Platform}:{d.DeviceToken[..Math.Min(12, d.DeviceToken.Length)]}…",
                StatusCode = 200,
                RequestSummary = $"{req.Title}|{req.Body}",
                ResponseSummary = "stub-ok",
                CalledAt = DateTimeOffset.UtcNow,
                CreatedBy = actorUserId
            });
            d.LastSeenAt = DateTimeOffset.UtcNow;
            delivered++;
        }

        await _db.SaveChangesAsync(ct);
        return new SysPushSendResult(devices.Count, delivered, delivered > 0 ? "IntegrationCallLog" : null);
    }

    // ── helpers ─────────────────────────────────────────────────────────────

    private static SysSsoProviderDto MapProvider(SysSsoProvider x) => new(
        x.Id, x.Code, x.DisplayName, x.ClientId, x.AuthorityUrl,
        x.RedirectUri, x.Scopes, x.JitProvisioning, x.IsActive, x.Note);

    private static SysPushDeviceDto MapDevice(SysPushDevice x) => new(
        x.Id, x.UserId, x.Platform, x.DeviceToken, x.AppVersion, x.IsValid, x.LastSeenAt);

    private static string BuildAuthorizeUrl(SysSsoProvider p, string state)
    {
        var authority = string.IsNullOrWhiteSpace(p.AuthorityUrl)
            ? "https://login.example.com/oauth/authorize"
            : p.AuthorityUrl.TrimEnd('/');
        if (!authority.Contains('?', StringComparison.Ordinal) &&
            !authority.EndsWith("/authorize", StringComparison.OrdinalIgnoreCase))
            authority += "/authorize";
        var q =
            $"client_id={Uri.EscapeDataString(p.ClientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(p.RedirectUri)}" +
            $"&response_type=code" +
            $"&scope={Uri.EscapeDataString(p.Scopes)}" +
            $"&state={Uri.EscapeDataString(state)}";
        return authority.Contains('?', StringComparison.Ordinal) ? $"{authority}&{q}" : $"{authority}?{q}";
    }

    private static (string? email, string? subject) ResolveSsoIdentity(SysSsoCallbackRequest req)
    {
        if (!string.IsNullOrWhiteSpace(req.Email) && !string.IsNullOrWhiteSpace(req.Subject))
            return (req.Email, req.Subject);

        var code = (req.Code ?? "").Trim();
        if (code.StartsWith("dev:", StringComparison.OrdinalIgnoreCase))
        {
            var payload = code[4..];
            var parts = payload.Split('|', 2);
            if (parts.Length == 2)
                return (parts[0].Trim(), parts[1].Trim());
            return (parts[0].Trim(), parts[0].Trim());
        }

        return (req.Email, req.Subject ?? req.Code);
    }

    private async Task<string> UniqueUsernameAsync(Guid tenantId, string baseName, CancellationToken ct)
    {
        var clean = new string(baseName.Where(c => char.IsLetterOrDigit(c) || c is '_' or '.').ToArray());
        if (string.IsNullOrWhiteSpace(clean)) clean = "sso_user";
        clean = clean.Length > 40 ? clean[..40] : clean;
        var candidate = clean;
        var i = 1;
        while (await _db.Users.AnyAsync(u => u.TenantId == tenantId && u.Username == candidate && !u.IsDeleted, ct))
        {
            candidate = $"{clean}_{i++}";
            if (i > 999) throw new AppException("Không tạo được username JIT.");
        }
        return candidate;
    }

    private static string MapDefaultToAccess(string defaultMask) => defaultMask switch
    {
        "Hide" => "None",
        "ReadOnly" => "Read",
        _ => "Masked"
    };

    private static int RankAccess(string access)
    {
        var i = Array.FindIndex(AccessOrder, a => a.Equals(access, StringComparison.OrdinalIgnoreCase));
        return i < 0 ? 0 : i;
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
                .Select(p => p.Code).ToListAsync(ct);

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
