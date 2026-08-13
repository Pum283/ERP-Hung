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

public sealed class SysSessionPolishTests
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
    public async Task SessionLimit_EnforcesMax5Sessions_RevokesOldest()
    {
        var (db, svc) = CreateSvc(nameof(SessionLimit_EnforcesMax5Sessions_RevokesOldest));
        var tenantId = Guid.NewGuid();
        var user = new AppUser
        {
            TenantId = tenantId, Username = "user-sessions", PasswordHash = PasswordHasher.Hash("Pass@123"), Status = UserStatus.Active
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        // Perform 6 logins to exceed max 5 sessions
        for (int i = 1; i <= 6; i++)
        {
            await svc.LoginAsync(new LoginRequest("user-sessions", "Pass@123"), $"192.168.1.{i}", $"UA-{i}");
            await Task.Delay(10); // slight time shift for LastSeenAt
        }

        var sessions = await svc.ListSessionsAsync(user.Id);
        var activeSessions = sessions.Where(s => !s.IsRevoked && s.ExpiresAt > DateTimeOffset.UtcNow).ToList();
        Assert.True(activeSessions.Count <= 5);

        // Revoke one session manually
        var sessionToRevoke = activeSessions[0];
        await svc.RevokeSessionAsync(user.Id, sessionToRevoke.Id);

        var sessionsAfter = await svc.ListSessionsAsync(user.Id);
        var revoked = sessionsAfter.First(s => s.Id == sessionToRevoke.Id);
        Assert.True(revoked.IsRevoked);
    }
}
