using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Auth;
using Erp.Application.DTOs.Sys;
using Erp.Application.Interfaces.Services.Auth;
using Erp.Domain.Entities.Sys;
using Erp.Domain.Enums.Sys;
using Erp.Infrastructure.Implementations.Services.Sys;
using Erp.Infrastructure.Persistence;
using Erp.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Erp.UnitTests;

public sealed class SysSsoFieldConfigPushPolishTests
{
    private sealed class DummyScope : IDataScopeService
    {
        public Task<UserScopeContext> GetUserScopeContextAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(new UserScopeContext(ScopeType.All, true, userId, null, Array.Empty<Guid>()));
    }

    private static (AppDbContext db, SysSsoFieldConfigPushService svc, Guid tenantId) Create(string dbName)
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        var db = new AppDbContext(opts);
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = "SuperSecretKeyForTestingJwt1234567890!",
            ["Jwt:Issuer"] = "ErpTest",
            ["Jwt:Audience"] = "ErpTest",
            ["Jwt:AccessTokenMinutes"] = "120"
        }).Build();
        var jwt = new JwtTokenService(config);
        var platform = new SysPlatformService(db, new OutboxWriter(db));
        var svc = new SysSsoFieldConfigPushService(db, jwt, new DummyScope(), platform, new SysNotifScanExportIpService(db), config);
        return (db, svc, Guid.NewGuid());
    }

    [Fact]
    public async Task Sso_UpsertAndStart_Succeeds()
    {
        var (db, svc, tenantId) = Create(nameof(Sso_UpsertAndStart_Succeeds));
        var admin = Guid.NewGuid();
        var dto = await svc.UpsertSsoProviderAsync(tenantId, admin, new SysSsoProviderUpsertRequest(
            null, "GOOGLE", "Google", "cid", "secret", "https://accounts.google.com/o/oauth2/v2",
            "http://localhost/cb", "openid email", true, true, null));
        Assert.Equal("GOOGLE", dto.Code);
        var start = await svc.StartSsoAsync(tenantId, "GOOGLE");
        Assert.Contains("client_id=cid", start.AuthorizeUrl);
        Assert.False(string.IsNullOrWhiteSpace(start.State));
    }

    [Fact]
    public async Task Sso_InactiveProvider_StartFails()
    {
        var (db, svc, tenantId) = Create(nameof(Sso_InactiveProvider_StartFails));
        await svc.UpsertSsoProviderAsync(tenantId, Guid.NewGuid(), new SysSsoProviderUpsertRequest(
            null, "OFF", "Off", "c", null, null, "http://x", null, false, false, null));
        await Assert.ThrowsAsync<AppException>(() => svc.StartSsoAsync(tenantId, "OFF"));
    }

    [Fact]
    public async Task Sso_Complete_JitCreatesUserAndToken()
    {
        var (db, svc, tenantId) = Create(nameof(Sso_Complete_JitCreatesUserAndToken));
        await svc.UpsertSsoProviderAsync(tenantId, Guid.NewGuid(), new SysSsoProviderUpsertRequest(
            null, "GOOGLE", "Google", "c", null, null, "http://x", null, true, true, null));
        var login = await svc.CompleteSsoAsync(tenantId, new SysSsoCallbackRequest(
            "GOOGLE", "dev:jit@test.com|sub-jit", null, null, null), "127.0.0.1", "test-ua");
        Assert.False(string.IsNullOrWhiteSpace(login.AccessToken));
        Assert.Equal(1, await db.SysExternalLogins.CountAsync());
        Assert.Equal(1, await db.LoginAudits.CountAsync(x => x.Success));
    }

    [Fact]
    public async Task Sso_Complete_JitOffWithoutLink_Fails()
    {
        var (_, svc, tenantId) = Create(nameof(Sso_Complete_JitOffWithoutLink_Fails));
        await svc.UpsertSsoProviderAsync(tenantId, Guid.NewGuid(), new SysSsoProviderUpsertRequest(
            null, "GOOGLE", "Google", "c", null, null, "http://x", null, false, true, null));
        await Assert.ThrowsAsync<UnauthorizedAppException>(() =>
            svc.CompleteSsoAsync(tenantId, new SysSsoCallbackRequest(
                "GOOGLE", "dev:nobody@test.com|sub-x", null, null, null), null, null));
    }

    [Fact]
    public async Task Sso_LockedUser_Fails()
    {
        var (db, svc, tenantId) = Create(nameof(Sso_LockedUser_Fails));
        await svc.UpsertSsoProviderAsync(tenantId, Guid.NewGuid(), new SysSsoProviderUpsertRequest(
            null, "GOOGLE", "Google", "c", null, null, "http://x", null, false, true, null));
        var user = new AppUser
        {
            TenantId = tenantId, Username = "locked", Email = "locked@test.com",
            Status = UserStatus.Locked, LockedUntil = DateTimeOffset.UtcNow.AddHours(1)
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        db.SysExternalLogins.Add(new SysExternalLogin
        {
            TenantId = tenantId, UserId = user.Id, ProviderCode = "GOOGLE",
            ProviderSubject = "sub-locked", Email = user.Email
        });
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            svc.CompleteSsoAsync(tenantId, new SysSsoCallbackRequest(
                "GOOGLE", null, null, user.Email, "sub-locked"), null, null));
    }

    [Fact]
    public async Task Field_UpsertAndEffectiveMostPermissive()
    {
        var (db, svc, tenantId) = Create(nameof(Field_UpsertAndEffectiveMostPermissive));
        var userId = Guid.NewGuid();
        var roleA = new Role { TenantId = tenantId, Code = "A", Name = "A", IsActive = true };
        var roleB = new Role { TenantId = tenantId, Code = "B", Name = "B", IsActive = true };
        db.Roles.AddRange(roleA, roleB);
        db.UserRoles.AddRange(
            new UserRole { TenantId = tenantId, UserId = userId, RoleId = roleA.Id, IsActive = true },
            new UserRole { TenantId = tenantId, UserId = userId, RoleId = roleB.Id, IsActive = true });
        await db.SaveChangesAsync();

        var field = await svc.UpsertSensitiveFieldAsync(tenantId, userId, new SysSensitiveFieldUpsertRequest(
            null, "HRM", "Employee", "salary", "Lương", "Mask", true));
        await svc.UpsertRoleFieldPermissionAsync(tenantId, userId, new SysRoleFieldPermissionUpsertRequest(roleA.Id, field.Id, "Masked"));
        await svc.UpsertRoleFieldPermissionAsync(tenantId, userId, new SysRoleFieldPermissionUpsertRequest(roleB.Id, field.Id, "Read"));

        var effective = await svc.GetMyFieldAccessAsync(tenantId, userId);
        Assert.Contains(effective, x => x.FieldKey == "salary" && x.Access == "Read");
        Assert.Equal("12••••••90", svc.ApplyFieldMask("1234567890", "Masked"));
        Assert.Equal("••••", svc.ApplyFieldMask("secret", "None"));
    }

    [Fact]
    public async Task Field_InvalidAccess_Fails()
    {
        var (db, svc, tenantId) = Create(nameof(Field_InvalidAccess_Fails));
        var role = new Role { TenantId = tenantId, Code = "R", Name = "R" };
        db.Roles.Add(role);
        await db.SaveChangesAsync();
        var field = await svc.UpsertSensitiveFieldAsync(tenantId, Guid.NewGuid(), new SysSensitiveFieldUpsertRequest(
            null, "SYS", "User", "phone", "Phone", "Mask", true));
        await Assert.ThrowsAsync<AppException>(() =>
            svc.UpsertRoleFieldPermissionAsync(tenantId, Guid.NewGuid(),
                new SysRoleFieldPermissionUpsertRequest(role.Id, field.Id, "Full")));
    }

    [Fact]
    public async Task Config_VersionIncrementAndRollback()
    {
        var (_, svc, tenantId) = Create(nameof(Config_VersionIncrementAndRollback));
        var uid = Guid.NewGuid();
        await svc.UpsertSettingVersionedAsync(tenantId, uid, new SysConfigUpsertVersionedRequest("k1", "v1", "init"));
        await svc.UpsertSettingVersionedAsync(tenantId, uid, new SysConfigUpsertVersionedRequest("k1", "v2", "upd"));
        var list = await svc.ListConfigVersionsAsync(tenantId, "k1");
        Assert.Equal(2, list.Count);
        Assert.True(list.First(x => x.VersionNumber == 2).IsCurrent);

        var rolled = await svc.RollbackConfigAsync(tenantId, uid, new SysConfigRollbackRequest("k1", 1, null));
        Assert.Equal(3, rolled.VersionNumber);
        Assert.Equal("v1", rolled.ConfigValue);
        Assert.True(rolled.IsCurrent);
    }

    [Fact]
    public async Task Config_RollbackMissing_Fails()
    {
        var (_, svc, tenantId) = Create(nameof(Config_RollbackMissing_Fails));
        await svc.UpsertSettingVersionedAsync(tenantId, Guid.NewGuid(), new SysConfigUpsertVersionedRequest("k", "1", null));
        await Assert.ThrowsAsync<AppException>(() =>
            svc.RollbackConfigAsync(tenantId, Guid.NewGuid(), new SysConfigRollbackRequest("k", 99, null)));
    }

    [Fact]
    public async Task Config_EmptyKey_Fails()
    {
        var (_, svc, tenantId) = Create(nameof(Config_EmptyKey_Fails));
        await Assert.ThrowsAsync<AppException>(() =>
            svc.UpsertSettingVersionedAsync(tenantId, Guid.NewGuid(), new SysConfigUpsertVersionedRequest("  ", "x", null)));
    }

    [Fact]
    public async Task Push_RegisterReassignRevokeAndTest()
    {
        var (db, svc, tenantId) = Create(nameof(Push_RegisterReassignRevokeAndTest));
        var u1 = Guid.NewGuid();
        var u2 = Guid.NewGuid();
        var d = await svc.RegisterPushDeviceAsync(tenantId, u1, new SysPushDeviceRegisterRequest("Fcm", "token-abcdef", "1.0"));
        Assert.True(d.IsValid);

        var re = await svc.RegisterPushDeviceAsync(tenantId, u2, new SysPushDeviceRegisterRequest("Fcm", "token-abcdef", "1.1"));
        Assert.Equal(u2, re.UserId);
        Assert.Equal(1, await db.SysPushDevices.CountAsync(x => !x.IsDeleted));

        var send = await svc.SendTestPushAsync(tenantId, u2, new SysPushTestRequest(u2, "Hi", "Body"));
        Assert.Equal(1, send.DeliveredStub);
        Assert.Equal(1, await db.IntegrationCallLogs.CountAsync());

        await svc.RevokePushDeviceAsync(tenantId, u2, re.Id);
        Assert.Empty(await svc.ListMyPushDevicesAsync(tenantId, u2));
    }

    [Fact]
    public async Task Push_InvalidToken_Fails()
    {
        var (_, svc, tenantId) = Create(nameof(Push_InvalidToken_Fails));
        await Assert.ThrowsAsync<AppException>(() =>
            svc.RegisterPushDeviceAsync(tenantId, Guid.NewGuid(), new SysPushDeviceRegisterRequest("Fcm", "short", null)));
    }

    [Fact]
    public async Task Push_InvalidPlatform_Fails()
    {
        var (_, svc, tenantId) = Create(nameof(Push_InvalidPlatform_Fails));
        await Assert.ThrowsAsync<AppException>(() =>
            svc.RegisterPushDeviceAsync(tenantId, Guid.NewGuid(),
                new SysPushDeviceRegisterRequest("Sms", "token-12345678", null)));
    }

    [Fact]
    public async Task Sso_DuplicateCode_Fails()
    {
        var (_, svc, tenantId) = Create(nameof(Sso_DuplicateCode_Fails));
        var uid = Guid.NewGuid();
        await svc.UpsertSsoProviderAsync(tenantId, uid, new SysSsoProviderUpsertRequest(
            null, "X", "X", "c", null, null, "http://x", null, true, true, null));
        await Assert.ThrowsAsync<AppException>(() =>
            svc.UpsertSsoProviderAsync(tenantId, uid, new SysSsoProviderUpsertRequest(
                null, "X", "X2", "c2", null, null, "http://x", null, true, true, null)));
    }

    [Fact]
    public async Task Field_DuplicateKey_Fails()
    {
        var (_, svc, tenantId) = Create(nameof(Field_DuplicateKey_Fails));
        var uid = Guid.NewGuid();
        await svc.UpsertSensitiveFieldAsync(tenantId, uid, new SysSensitiveFieldUpsertRequest(
            null, "HRM", "E", "salary", "Lương", "Mask", true));
        await Assert.ThrowsAsync<AppException>(() =>
            svc.UpsertSensitiveFieldAsync(tenantId, uid, new SysSensitiveFieldUpsertRequest(
                null, "HRM", "E", "salary", "Lương 2", "Mask", true)));
    }
}
