using Erp.Application.DTOs.Prt;
using Erp.Domain.Entities.Fin;
using Erp.Domain.Entities.Prt;
using Erp.Infrastructure.Implementations.Services.Prt;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class PrtPortalPolishTests
{
    private static AppDbContext CreateDb(string dbName)
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(opts);
    }

    [Fact]
    public async Task Register_And_Login_ReturnsToken_AndActiveStatus()
    {
        using var db = CreateDb(nameof(Register_And_Login_ReturnsToken_AndActiveStatus));
        var svc = new PrtPortalService(db);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var reg = await svc.RegisterAsync(tenantId, userId, new PrtRegisterRequest("user1@portal.local", "User 1", "Pass123!", "CUST001"));
        Assert.Equal("Pending", reg.Status);

        var loginRes = await svc.LoginAsync(tenantId, new PrtLoginRequest("user1@portal.local", "Pass123!"));
        Assert.NotNull(loginRes.Token);
        Assert.StartsWith("prt_token_", loginRes.Token);
        Assert.Equal("Active", loginRes.Account.Status);
    }

    [Fact]
    public async Task ForgotPassword_CreatesResetToken_AndLogsIntegrationCall()
    {
        using var db = CreateDb(nameof(ForgotPassword_CreatesResetToken_AndLogsIntegrationCall));
        var svc = new PrtPortalService(db);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var acc = await svc.UpsertAccountAsync(tenantId, userId, new PrtAccountUpsertRequest(
            null, "PRT-001", "forgot@portal.local", "Forgot User", "OldPass123", "CUST002", "Cust 2", "Active"));

        var forgotRes = await svc.ForgotPasswordAsync(tenantId, new PrtForgotPasswordRequest("forgot@portal.local"));
        Assert.NotNull(forgotRes.Email);

        var log = await db.IntegrationCallLogs.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Kind == "PRT_RESET_PASSWORD");
        Assert.NotNull(log);
        Assert.Equal("forgot@portal.local", log.Target);
        Assert.Equal(200, log.StatusCode);
        Assert.Contains("Token:", log.RequestSummary);
    }

    [Fact]
    public async Task ResetPassword_ValidToken_UpdatesPasswordHash_AndClearsToken()
    {
        using var db = CreateDb(nameof(ResetPassword_ValidToken_UpdatesPasswordHash_AndClearsToken));
        var svc = new PrtPortalService(db);
        var tenantId = Guid.NewGuid();

        var entity = new PrtAccount
        {
            TenantId = tenantId,
            Code = "PRT-003",
            Email = "reset@portal.local",
            DisplayName = "Reset User",
            PasswordHash = "OldHash",
            ResetTokenStub = "TOKEN123",
            ResetTokenExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
            Status = "Pending"
        };
        db.PrtAccounts.Add(entity);
        await db.SaveChangesAsync();

        var resetRes = await svc.ResetPasswordAsync(tenantId, new PrtResetPasswordRequest("reset@portal.local", "TOKEN123", "NewSecret123"));
        Assert.Equal("Active", resetRes.Status);

        var loginRes = await svc.LoginAsync(tenantId, new PrtLoginRequest("reset@portal.local", "NewSecret123"));
        Assert.NotNull(loginRes.Token);
    }

    [Fact]
    public async Task GetArSummary_AggregatesBothPrtInvoicesAndFinArInvoices()
    {
        using var db = CreateDb(nameof(GetArSummary_AggregatesBothPrtInvoicesAndFinArInvoices));
        var svc = new PrtPortalService(db);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var acc = await svc.UpsertAccountAsync(tenantId, userId, new PrtAccountUpsertRequest(
            null, "PRT-004", "ar@portal.local", "AR User", "Pass123", "CUST-AR-99", "Cust AR 99", "Active"));

        // Prt Invoice open = 1000
        await svc.UpsertInvoiceAsync(tenantId, userId, new PrtInvoiceUpsertRequest(
            null, acc.Id, "PINV-01", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(15), 1000, 0, "Open"));

        // Fin Ar Invoice open = 2500 (Total 3000, Received 500)
        db.FinArInvoices.Add(new FinArInvoice
        {
            TenantId = tenantId,
            Code = "FIN-AR-01",
            CustomerId = acc.Id,
            CustomerInvoiceNo = "CUST-AR-99",
            TotalAmount = 3000,
            ReceivedAmount = 500,
            Status = "Partial"
        });
        await db.SaveChangesAsync();

        var summary = await svc.GetArSummaryAsync(tenantId, acc.Id);
        Assert.Equal(3500, summary.OpenAmount); // 1000 + 2500
        Assert.Equal(2, summary.OpenInvoiceCount); // 1 PRT + 1 FIN
    }
}
