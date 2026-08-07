using Erp.Application.Common.Exceptions;
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

public sealed class Auth2FaPolishTests
{
    private sealed class DummyScope : IDataScopeService
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

        var svc = new AuthService(db, jwt, new DummyScope(), platform, config, NullLogger<AuthService>.Instance);
        return (db, svc);
    }

    [Fact]
    public async Task TwoFactor_BeginConfirmDisableFlow_Succeeds()
    {
        var (db, svc) = CreateSvc(nameof(TwoFactor_BeginConfirmDisableFlow_Succeeds));
        var tenantId = Guid.NewGuid();
        var user = new AppUser
        {
            TenantId = tenantId, Username = "user2fa", PasswordHash = PasswordHasher.Hash("Pass@123"), Status = UserStatus.Active
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        // 1. Begin
        var enableRes = await svc.BeginEnable2FaAsync(user.Id);
        Assert.NotNull(enableRes.Secret);
        Assert.Contains("otpauth://totp", enableRes.OtpAuthUri);

        // 2. Confirm with dev code 000000
        await svc.ConfirmEnable2FaAsync(user.Id, new Verify2FaRequest("000000"));
        var updatedUser = await db.Users.FirstAsync(x => x.Id == user.Id);
        Assert.True(updatedUser.TotpEnabled);

        // 3. Login without 2FA code throws AppException
        await Assert.ThrowsAsync<AppException>(() => svc.LoginAsync(new LoginRequest("user2fa", "Pass@123"), "127.0.0.1", "ua"));

        // 4. Login with 2FA code 000000 succeeds
        var loginRes = await svc.LoginAsync(new LoginRequest("user2fa", "Pass@123", "000000"), "127.0.0.1", "ua");
        Assert.NotNull(loginRes.AccessToken);

        // 5. Disable 2FA
        await svc.Disable2FaAsync(user.Id, new Verify2FaRequest("000000"));
        var disabledUser = await db.Users.FirstAsync(x => x.Id == user.Id);
        Assert.False(disabledUser.TotpEnabled);
    }
}
