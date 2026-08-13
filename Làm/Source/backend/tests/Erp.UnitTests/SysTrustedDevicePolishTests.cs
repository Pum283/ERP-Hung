using Erp.Application.DTOs.Auth;
using Erp.Application.DTOs.Sys;
using Erp.Application.Interfaces.Services.Auth;
using Erp.Domain.Entities.Sys;
using Erp.Domain.Enums.Sys;
using Erp.Infrastructure.Implementations.Services.Auth;
using Erp.Infrastructure.Implementations.Services.Sys;
using Erp.Infrastructure.Persistence;
using Erp.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Erp.UnitTests;

public sealed class SysTrustedDevicePolishTests
{
    private sealed class DummyScope : Erp.Application.Interfaces.Services.Auth.IDataScopeService
    {
        public Task<UserScopeContext> GetUserScopeContextAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(new UserScopeContext(ScopeType.All, true, userId, null, Array.Empty<Guid>()));
    }

    private static (AppDbContext db, AuthService svc) CreateSvc(string dbName)
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        var db = new AppDbContext(opts);
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = "SuperSecretKeyForTestingJwt1234567890!",
            ["Jwt:Issuer"] = "ErpTest",
            ["Jwt:Audience"] = "ErpTest"
        }).Build();

        var jwt = new JwtTokenService(config);
        var platform = new SysPlatformService(db, new OutboxWriter(db));
        var svc = new AuthService(db, jwt, new DummyScope(), platform, new SysNotifScanExportIpService(db), config, NullLogger<AuthService>.Instance);
        return (db, svc);
    }

    [Fact]
    public async Task TrustedDevice_RegisterAndRevoke_Succeeds()
    {
        var (db, svc) = CreateSvc(nameof(TrustedDevice_RegisterAndRevoke_Succeeds));
        var tenantId = Guid.NewGuid();
        var user = new AppUser
        {
            TenantId = tenantId, Username = "user-devices", PasswordHash = PasswordHasher.Hash("Pass@123"), Status = UserStatus.Active
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        // Register trusted device
        var device = await svc.RegisterTrustedDeviceAsync(user.Id, new RegisterTrustedDeviceRequest("fp-macbook-123", "MacBook Pro M2"), "192.168.1.100");
        Assert.NotNull(device);
        Assert.Equal("fp-macbook-123", device.DeviceFingerprint);
        Assert.Equal("MacBook Pro M2", device.DeviceName);
        Assert.True(device.IsActive);

        var list = await svc.ListTrustedDevicesAsync(user.Id);
        Assert.Single(list);

        // Revoke trusted device
        await svc.RevokeTrustedDeviceAsync(user.Id, device.Id);
        var listAfter = await svc.ListTrustedDevicesAsync(user.Id);
        Assert.Empty(listAfter);
    }
}
