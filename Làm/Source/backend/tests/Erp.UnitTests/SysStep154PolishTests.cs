using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Sys;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Sys;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class SysStep154PolishTests
{
    private static (AppDbContext db, SysStep154Service svc, Guid tenantId) Create(string name)
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(name).Options;
        var db = new AppDbContext(opts);
        return (db, new SysStep154Service(db), Guid.NewGuid());
    }

    [Fact]
    public async Task Prefs_DefaultAndUpsert_Succeeds()
    {
        var (_, svc, tenantId) = Create(nameof(Prefs_DefaultAndUpsert_Succeeds));
        var userId = Guid.NewGuid();
        var d = await svc.GetMyNotificationPreferencesAsync(tenantId, userId);
        Assert.True(d.ChannelInApp);
        Assert.False(d.MuteAll);

        var u = await svc.UpsertMyNotificationPreferencesAsync(tenantId, userId,
            new SysNotificationPreferenceUpsertRequest(false, true, false, false, true, null, null));
        Assert.True(u.MuteAll);
        Assert.False(u.ChannelInApp);
    }

    [Fact]
    public void Prefs_SecurityEvent_BypassesMute()
    {
        var prefs = new SysNotificationPreferenceDto(Guid.NewGuid(), false, false, false, false, true, null, null);
        Assert.True(SysStep154Service.ShouldDeliverInAppStatic(prefs, "security.account_locked", DateTimeOffset.UtcNow));
        Assert.False(SysStep154Service.ShouldDeliverInAppStatic(prefs, "wf.task.assigned", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Prefs_QuietHours_BlocksNormalEvents()
    {
        var prefs = new SysNotificationPreferenceDto(Guid.NewGuid(), true, true, false, true, false, "22:00", "06:00");
        var night = new DateTimeOffset(2026, 8, 12, 23, 0, 0, TimeSpan.Zero);
        var day = new DateTimeOffset(2026, 8, 12, 10, 0, 0, TimeSpan.Zero);
        Assert.False(SysStep154Service.ShouldDeliverInAppStatic(prefs, "wf.task.assigned", night));
        Assert.True(SysStep154Service.ShouldDeliverInAppStatic(prefs, "wf.task.assigned", day));
    }

    [Fact]
    public async Task Prefs_InvalidQuietHours_Fails()
    {
        var (_, svc, tenantId) = Create(nameof(Prefs_InvalidQuietHours_Fails));
        await Assert.ThrowsAsync<AppException>(() =>
            svc.UpsertMyNotificationPreferencesAsync(tenantId, Guid.NewGuid(),
                new SysNotificationPreferenceUpsertRequest(true, true, false, true, false, "25:00", "06:00")));
    }

    [Fact]
    public async Task Scan_EicarFileName_InfectedAndBlocksDownload()
    {
        var (db, svc, tenantId) = Create(nameof(Scan_EicarFileName_InfectedAndBlocksDownload));
        var file = new FileObject
        {
            TenantId = tenantId, FileName = "eicar.com", StorageKey = "k1", SizeBytes = 68, ScanStatus = "Pending"
        };
        db.FileObjects.Add(file);
        await db.SaveChangesAsync();

        var scan = await svc.ScanFileAsync(tenantId, Guid.NewGuid(), file.Id, null);
        Assert.Equal("Infected", scan.ScanStatus);
        await Assert.ThrowsAsync<ForbiddenException>(() => svc.EnsureFileDownloadAllowedAsync(tenantId, file.Id));
    }

    [Fact]
    public async Task Scan_CleanFile_AllowsDownload()
    {
        var (db, svc, tenantId) = Create(nameof(Scan_CleanFile_AllowsDownload));
        var file = new FileObject
        {
            TenantId = tenantId, FileName = "report.pdf", StorageKey = "k2", SizeBytes = 100, ScanStatus = "Pending"
        };
        db.FileObjects.Add(file);
        await db.SaveChangesAsync();
        var scan = await svc.ScanFileAsync(tenantId, Guid.NewGuid(), file.Id, "normal content");
        Assert.Equal("Clean", scan.ScanStatus);
        await svc.EnsureFileDownloadAllowedAsync(tenantId, file.Id);
        Assert.NotEmpty(await svc.ListFileScanLogsAsync(tenantId, file.Id));
    }

    [Fact]
    public async Task BulkExport_MultiEntity_AndDownload()
    {
        var (db, svc, tenantId) = Create(nameof(BulkExport_MultiEntity_AndDownload));
        db.Users.Add(new AppUser { TenantId = tenantId, Username = "u1", Email = "u1@t.com" });
        await db.SaveChangesAsync();

        var job = await svc.StartBulkExportAsync(tenantId, Guid.NewGuid(),
            new SysBulkExportRequest(new[] { "Users", "Files" }, "Csv"));
        Assert.Equal("Completed", job.Status);
        Assert.True(job.RowCount >= 1);

        var dl = await svc.DownloadExportJobAsync(tenantId, job.Id);
        Assert.Contains("Users", System.Text.Encoding.UTF8.GetString(dl.Content));
    }

    [Fact]
    public async Task BulkExport_EmptyTypes_Fails()
    {
        var (_, svc, tenantId) = Create(nameof(BulkExport_EmptyTypes_Fails));
        await Assert.ThrowsAsync<AppException>(() =>
            svc.StartBulkExportAsync(tenantId, Guid.NewGuid(), new SysBulkExportRequest(Array.Empty<string>(), "Csv")));
    }

    [Fact]
    public async Task BulkExport_UnsupportedEntity_Fails()
    {
        var (_, svc, tenantId) = Create(nameof(BulkExport_UnsupportedEntity_Fails));
        await Assert.ThrowsAsync<AppException>(() =>
            svc.StartBulkExportAsync(tenantId, Guid.NewGuid(), new SysBulkExportRequest(new[] { "Products" }, "Csv")));
    }

    [Fact]
    public async Task BulkExport_Expired_Fails()
    {
        var (db, svc, tenantId) = Create(nameof(BulkExport_Expired_Fails));
        var job = new ImportExportJob
        {
            TenantId = tenantId, JobType = "BulkExport", EntityType = "Users", Status = "Completed",
            ResultContent = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("a")),
            ResultFileName = "a.csv", ResultContentType = "text/csv",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1)
        };
        db.ImportExportJobs.Add(job);
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<AppException>(() => svc.DownloadExportJobAsync(tenantId, job.Id));
    }

    [Fact]
    public async Task Ip_DenyWins()
    {
        var (_, svc, tenantId) = Create(nameof(Ip_DenyWins));
        await svc.UpsertIpRuleAsync(tenantId, Guid.NewGuid(),
            new SysIpRuleUpsertRequest(null, "10.0.0.0/8", "Allow", "corp", true));
        await svc.UpsertIpRuleAsync(tenantId, Guid.NewGuid(),
            new SysIpRuleUpsertRequest(null, "10.1.2.3", "Deny", "bad", true));
        var r = await svc.EvaluateIpAsync(tenantId, "10.1.2.3");
        Assert.False(r.Allowed);
        Assert.StartsWith("deny:", r.Reason);
    }

    [Fact]
    public async Task Ip_Allowlist_BlocksUnknown()
    {
        var (_, svc, tenantId) = Create(nameof(Ip_Allowlist_BlocksUnknown));
        await svc.UpsertIpRuleAsync(tenantId, Guid.NewGuid(),
            new SysIpRuleUpsertRequest(null, "192.168.1.10", "Allow", null, true));
        var ok = await svc.EvaluateIpAsync(tenantId, "192.168.1.10");
        Assert.True(ok.Allowed);
        var bad = await svc.EvaluateIpAsync(tenantId, "8.8.8.8");
        Assert.False(bad.Allowed);
        await Assert.ThrowsAsync<ForbiddenException>(() => svc.EnsureIpAllowedAsync(tenantId, "8.8.8.8"));
    }

    [Fact]
    public async Task Ip_NoRules_AllowsAll()
    {
        var (_, svc, tenantId) = Create(nameof(Ip_NoRules_AllowsAll));
        var r = await svc.EvaluateIpAsync(tenantId, "1.2.3.4");
        Assert.True(r.Allowed);
    }

    [Fact]
    public async Task Ip_InvalidCidr_Fails()
    {
        var (_, svc, tenantId) = Create(nameof(Ip_InvalidCidr_Fails));
        await Assert.ThrowsAsync<AppException>(() =>
            svc.UpsertIpRuleAsync(tenantId, Guid.NewGuid(),
                new SysIpRuleUpsertRequest(null, "not-an-ip", "Allow", null, true)));
    }

    [Fact]
    public void IpMatches_CidrWorks()
    {
        Assert.True(SysStep154Service.IpMatches("10.5.6.7", "10.0.0.0/8"));
        Assert.False(SysStep154Service.IpMatches("11.0.0.1", "10.0.0.0/8"));
        Assert.True(SysStep154Service.IpMatches("192.168.1.1", "192.168.1.1"));
    }
}
