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
            null, "PRT-004", "ar@portal.local", "AR User", "Pass123!", "CUST-AR-99", "Cust AR 99", "Active"));

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

    [Fact]
    public async Task Register_WeakPassword_ThrowsAppException()
    {
        using var db = CreateDb(nameof(Register_WeakPassword_ThrowsAppException));
        var svc = new PrtPortalService(db);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var ex = await Assert.ThrowsAsync<Erp.Application.Common.Exceptions.AppException>(
            () => svc.RegisterAsync(tenantId, userId, new PrtRegisterRequest("weak@portal.local", "Weak User", "12345", null)));
        Assert.Contains("Mật khẩu tối thiểu 8 ký tự", ex.Message);
    }

    [Fact]
    public async Task BruteForceLockout_LocksAccount_After5FailedAttempts()
    {
        using var db = CreateDb(nameof(BruteForceLockout_LocksAccount_After5FailedAttempts));
        var svc = new PrtPortalService(db);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await svc.RegisterAsync(tenantId, userId, new PrtRegisterRequest("lockme@portal.local", "Lock Me", "StrongPass123!", null));

        for (int i = 0; i < 5; i++)
        {
            await Assert.ThrowsAsync<Erp.Application.Common.Exceptions.AppException>(
                () => svc.LoginAsync(tenantId, new PrtLoginRequest("lockme@portal.local", "WrongPass")));
        }

        var ex = await Assert.ThrowsAsync<Erp.Application.Common.Exceptions.AppException>(
            () => svc.LoginAsync(tenantId, new PrtLoginRequest("lockme@portal.local", "StrongPass123!")));
        Assert.Contains("khóa", ex.Message, StringComparison.OrdinalIgnoreCase);

        var lockLog = await db.IntegrationCallLogs.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Kind == "PRT_ACCOUNT_LOCKED");
        Assert.NotNull(lockLog);
    }

    [Fact]
    public async Task ResetPassword_ExpiredToken_ThrowsAppException()
    {
        using var db = CreateDb(nameof(ResetPassword_ExpiredToken_ThrowsAppException));
        var svc = new PrtPortalService(db);
        var tenantId = Guid.NewGuid();

        db.PrtAccounts.Add(new PrtAccount
        {
            TenantId = tenantId,
            Code = "PRT-EXP",
            Email = "expired@portal.local",
            DisplayName = "Expired User",
            PasswordHash = "OldHash",
            ResetTokenStub = "EXPIRED_TOKEN",
            ResetTokenExpiresAt = DateTimeOffset.UtcNow.AddHours(-1),
            Status = "Active"
        });
        await db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<Erp.Application.Common.Exceptions.AppException>(
            () => svc.ResetPasswordAsync(tenantId, new PrtResetPasswordRequest("expired@portal.local", "EXPIRED_TOKEN", "NewStrong123!")));
        Assert.Contains("hết hạn", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetArSummary_CalculatesOverdueAmounts_AndCounts()
    {
        using var db = CreateDb(nameof(GetArSummary_CalculatesOverdueAmounts_AndCounts));
        var svc = new PrtPortalService(db);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var acc = await svc.UpsertAccountAsync(tenantId, userId, new PrtAccountUpsertRequest(
            null, "PRT-OVERDUE", "overdue@portal.local", "Overdue User", "Pass123!", "CUST-OD", "Cust OD", "Active"));

        // PRT Overdue invoice (Due 5 days ago)
        await svc.UpsertInvoiceAsync(tenantId, userId, new PrtInvoiceUpsertRequest(
            null, acc.Id, "INV-OD-01", DateTimeOffset.UtcNow.AddDays(-10), DateTimeOffset.UtcNow.AddDays(-5), 2000, 500, "Open"));

        var summary = await svc.GetArSummaryAsync(tenantId, acc.Id);
        Assert.Equal(1500, summary.OpenAmount);
        Assert.Equal(1500, summary.OverdueAmount);
        Assert.Equal(1, summary.OverdueInvoiceCount);

        var invoices = await svc.ListInvoicesAsync(tenantId, acc.Id, true);
        Assert.Single(invoices);
        Assert.True(invoices[0].IsOverdue);
        Assert.True(invoices[0].OverdueDays >= 4);
    }
}
