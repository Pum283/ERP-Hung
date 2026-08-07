using Erp.Application.Common.Exceptions;
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

namespace Erp.UnitTests;

/// <summary>UC_SYS_060/061/004/019 — Email/SMS stub + forgot OTP + invite.</summary>
public sealed class SysChannelPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly SysPlatformService _platform;
    private readonly SysMasterService _master;
    private readonly AuthService _auth;
    private readonly Guid _tenant = Guid.Parse("11111111-2222-3333-4444-555555555555");
    private readonly Guid _actor = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    private sealed class NoopScope : IDataScopeService
    {
        public Task<UserScopeContext> GetUserScopeContextAsync(Guid userId, CancellationToken ct = default)
            => Task.FromResult(new UserScopeContext(ScopeType.All, true, userId, null, Array.Empty<Guid>()));
    }

    private sealed class NoopAuthz : IAuthorizationService
    {
        public Task<bool> HasPermissionAsync(Guid userId, string permissionCode, CancellationToken ct = default)
            => Task.FromResult(true);
        public Task EnsurePermissionAsync(Guid userId, string permissionCode, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    public SysChannelPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("sys-channel-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        var outbox = new OutboxWriter(_db);
        _platform = new SysPlatformService(_db, outbox);
        _master = new SysMasterService(_db, new NoopScope(), new NoopAuthz(), _platform);
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        _auth = new AuthService(
            _db, new JwtTokenService(cfg), new NoopScope(), _platform, cfg, NullLogger<AuthService>.Instance);

        _db.Tenants.Add(new Tenant { Id = _tenant, Code = "T1", Name = "T1", CreatedBy = _actor });
        _db.Licenses.Add(new License
        {
            TenantId = _tenant, PlanCode = "DEV", Status = "Active",
            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            ValidTo = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
            MaxUsers = 100, MaxOrgUnits = 50, CreatedBy = _actor,
        });
        _db.Users.Add(new AppUser
        {
            Id = _actor, TenantId = _tenant, Username = "admin", Email = "admin@test.local",
            PasswordHash = PasswordHasher.Hash("!Abc123"), Status = UserStatus.Active, CreatedBy = _actor,
        });
        _db.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task SendChannel_Email_WritesIntegrationLogAndOutbox()
    {
        var res = await _platform.SendChannelMessageAsync(_tenant, _actor, new ChannelSendRequest(
            "Email", "FORGOT_PASSWORD", "a@b.com",
            new Dictionary<string, string> { ["otp"] = "123456", ["displayName"] = "A", ["expiresMinutes"] = "15" },
            "sys.auth.forgot_password"));

        Assert.Equal("Email", res.Channel);
        Assert.Equal("Logged", res.Status);
        Assert.Contains("123456", res.Body);
        Assert.Equal(1, await _db.IntegrationCallLogs.CountAsync(x => x.TenantId == _tenant && x.Kind == "Email"));
        Assert.Equal(1, await _db.OutboxMessages.CountAsync(x => x.TenantId == _tenant && x.SourceModule == "SYS"));
        Assert.True(await _db.MessageTemplates.AnyAsync(x => x.Code == "FORGOT_PASSWORD" && x.Channel == "Email"));
        Assert.True(await _db.ExternalIntegrations.AnyAsync(x => x.Kind == "Email" && x.IsActive));
    }

    [Fact]
    public async Task SendChannel_Sms_WritesLog()
    {
        var res = await _platform.SendChannelMessageAsync(_tenant, _actor, new ChannelSendRequest(
            "SMS", "FORGOT_PASSWORD", "0901000000",
            new Dictionary<string, string> { ["otp"] = "654321", ["expiresMinutes"] = "15", ["displayName"] = "B" }));

        Assert.Equal("SMS", res.Channel);
        Assert.Contains("654321", res.Body);
        Assert.Equal(1, await _db.IntegrationCallLogs.CountAsync(x => x.Kind == "SMS"));
    }

    [Fact]
    public async Task SendChannel_InvalidChannel_Throws()
    {
        await Assert.ThrowsAsync<AppException>(() =>
            _platform.SendChannelMessageAsync(_tenant, _actor, new ChannelSendRequest("Push", "X", "t")));
    }

    [Fact]
    public async Task ForgotPassword_WithEmail_CreatesTokenAndChannelLog()
    {
        await _auth.ForgotPasswordAsync(new ForgotPasswordRequest("admin@test.local"));

        Assert.Equal(1, await _db.PasswordResetTokens.CountAsync(x => x.UserId == _actor));
        var log = await _db.IntegrationCallLogs.SingleAsync(x => x.Kind == "Email");
        Assert.Equal("admin@test.local", log.Target);
        Assert.Contains("FORGOT_PASSWORD", log.RequestSummary);
    }

    [Fact]
    public async Task ForgotPassword_UnknownUser_SilentNoLog()
    {
        await _auth.ForgotPasswordAsync(new ForgotPasswordRequest("nobody@x.com"));
        Assert.Equal(0, await _db.IntegrationCallLogs.CountAsync());
    }

    [Fact]
    public async Task ForgotPassword_PhoneOnly_UsesSms()
    {
        var u = new AppUser
        {
            TenantId = _tenant, Username = "phoneuser", Phone = "0912345678",
            PasswordHash = PasswordHasher.Hash("!Abc123"), Status = UserStatus.Active, CreatedBy = _actor,
        };
        _db.Users.Add(u);
        await _db.SaveChangesAsync();

        await _auth.ForgotPasswordAsync(new ForgotPasswordRequest("phoneuser"));
        var log = await _db.IntegrationCallLogs.SingleAsync();
        Assert.Equal("SMS", log.Kind);
        Assert.Equal("0912345678", log.Target);
    }

    [Fact]
    public async Task InviteUser_CreatesUser_SendsEmail_AndOtp()
    {
        var res = await _master.InviteUserAsync(_tenant, _actor, new InviteUserRequest(
            "newbie", "New Bee", "new@test.local", null));

        Assert.Equal("Email", res.Channel);
        Assert.Equal("new@test.local", res.Target);
        Assert.True(await _db.Users.AnyAsync(x => x.Username == "newbie" && x.MustChangePassword));
        Assert.Equal(1, await _db.PasswordResetTokens.CountAsync(x => x.UserId == res.UserId));
        Assert.Equal(1, await _db.IntegrationCallLogs.CountAsync(x => x.Target == "new@test.local"));
        Assert.Contains("USER_INVITE", (await _db.IntegrationCallLogs.FirstAsync()).RequestSummary);
    }

    [Fact]
    public async Task InviteUser_RequiresContact()
    {
        await Assert.ThrowsAsync<AppException>(() =>
            _master.InviteUserAsync(_tenant, _actor, new InviteUserRequest("x", null, null, null)));
    }

    [Fact]
    public async Task InviteUser_SmsWhenNoEmail()
    {
        var res = await _master.InviteUserAsync(_tenant, _actor, new InviteUserRequest(
            "smsuser", null, null, "0909888777"));
        Assert.Equal("SMS", res.Channel);
        Assert.Equal("0909888777", res.Target);
    }

    [Fact]
    public async Task ResetPasswordWithOtp_AfterInvite_Works()
    {
        var invite = await _master.InviteUserAsync(_tenant, _actor, new InviteUserRequest(
            "resetme", null, "r@test.local", null));
        var otp = await _db.PasswordResetTokens
            .Where(x => x.UserId == invite.UserId).Select(x => x.OtpCode).FirstAsync();

        await _auth.ResetPasswordWithOtpAsync(new ResetPasswordWithOtpRequest(
            "resetme", otp, "!NewPass9"));

        var user = await _db.Users.SingleAsync(x => x.Id == invite.UserId);
        Assert.False(user.MustChangePassword);
        Assert.True(PasswordHasher.Verify("!NewPass9", user.PasswordHash!));
    }
}
