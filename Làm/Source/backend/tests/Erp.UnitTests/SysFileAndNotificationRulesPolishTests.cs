using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Sys;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Sys;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class SysFileAndNotificationRulesPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly SysPlatformService _sysPlatformSvc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _user = Guid.NewGuid();

    public SysFileAndNotificationRulesPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("sys-file-rules-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        var outbox = new OutboxWriter(_db);
        _sysPlatformSvc = new SysPlatformService(_db, outbox);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task UC_SYS_063_UpsertNotificationRule_CreateRule_Succeeds()
    {
        var req = new NotificationRuleDto(Guid.Empty, "ORDER_CREATED", "Đơn hàng mới {orderCode}", "Đơn hàng {orderCode} đã tạo thành công.", true);
        var res = await _sysPlatformSvc.UpsertNotificationRuleAsync(_tenant, _user, req);

        Assert.NotEqual(Guid.Empty, res.Id);
        Assert.Equal("ORDER_CREATED", res.EventType);
        Assert.Equal("Đơn hàng mới {orderCode}", res.TitleTemplate);
    }

    [Fact]
    public async Task UC_SYS_063_UpsertNotificationRule_DuplicateEventType_ThrowsAppException()
    {
        await _sysPlatformSvc.UpsertNotificationRuleAsync(_tenant, _user, new NotificationRuleDto(Guid.Empty, "RULE_DUP", "Title 1", "Body 1", true));

        var reqDup = new NotificationRuleDto(Guid.Empty, "RULE_DUP", "Title 2", "Body 2", true);
        var ex = await Assert.ThrowsAsync<AppException>(() => _sysPlatformSvc.UpsertNotificationRuleAsync(_tenant, _user, reqDup));
        Assert.Contains("đã tồn tại", ex.Message);
    }

    [Fact]
    public async Task UC_SYS_063_UpsertNotificationRule_EmptyEventType_ThrowsAppException()
    {
        var req = new NotificationRuleDto(Guid.Empty, "  ", "Title", "Body", true);
        var ex = await Assert.ThrowsAsync<AppException>(() => _sysPlatformSvc.UpsertNotificationRuleAsync(_tenant, _user, req));
        Assert.Contains("không được để trống", ex.Message);
    }

    [Fact]
    public async Task UC_SYS_063_ListNotificationRules_FiltersByTenantAndDeletedStatus()
    {
        _db.NotificationRules.Add(new NotificationRule { TenantId = _tenant, EventType = "EV_1", TitleTemplate = "T1", BodyTemplate = "B1", IsDeleted = false });
        _db.NotificationRules.Add(new NotificationRule { TenantId = _tenant, EventType = "EV_2", TitleTemplate = "T2", BodyTemplate = "B2", IsDeleted = true });
        _db.NotificationRules.Add(new NotificationRule { TenantId = Guid.NewGuid(), EventType = "EV_3", TitleTemplate = "T3", BodyTemplate = "B3", IsDeleted = false });
        await _db.SaveChangesAsync();

        var rules = await _sysPlatformSvc.ListNotificationRulesAsync(_tenant);
        Assert.Single(rules);
        Assert.Equal("EV_1", rules[0].EventType);
    }

    [Fact]
    public async Task UC_SYS_065_ListIntegrationLogs_TakesRequestedCount_AndSortsDescending()
    {
        for (int i = 1; i <= 10; i++)
        {
            _db.IntegrationCallLogs.Add(new IntegrationCallLog
            {
                TenantId = _tenant, Kind = "Email", Target = $"user{i}@test.com", StatusCode = 200, CalledAt = DateTimeOffset.UtcNow.AddMinutes(i)
            });
        }
        await _db.SaveChangesAsync();

        var logs = await _sysPlatformSvc.ListIntegrationLogsAsync(_tenant, 5);
        Assert.Equal(5, logs.Count);
        Assert.Equal("user10@test.com", logs[0].Target);
    }

    [Fact]
    public async Task UC_SYS_065_ListIntegrationLogs_FiltersByTenant()
    {
        _db.IntegrationCallLogs.Add(new IntegrationCallLog { TenantId = _tenant, Kind = "Sms", Target = "0900000001", StatusCode = 200 });
        _db.IntegrationCallLogs.Add(new IntegrationCallLog { TenantId = Guid.NewGuid(), Kind = "Sms", Target = "0900000002", StatusCode = 200 });
        await _db.SaveChangesAsync();

        var logs = await _sysPlatformSvc.ListIntegrationLogsAsync(_tenant, 10);
        Assert.Single(logs);
        Assert.Equal("0900000001", logs[0].Target);
    }

    [Fact]
    public async Task UC_SYS_066_UploadFileMetadata_ValidFile_CreatesFileObjectWithUniqueStorageKey()
    {
        var req = new FileUploadRequest("BaoCaoTaichinh2026.pdf", "application/pdf", 1024 * 500, null);
        var res = await _sysPlatformSvc.UploadFileMetadataAsync(_tenant, _user, req);

        Assert.NotEqual(Guid.Empty, res.Id);
        Assert.Equal("BaoCaoTaichinh2026.pdf", res.FileName);
        Assert.Equal("application/pdf", res.ContentType);
        Assert.Equal(1024 * 500, res.SizeBytes);
        Assert.Contains("BaoCaoTaichinh2026.pdf", res.StorageKey);
    }

    [Fact]
    public async Task UC_SYS_066_UploadFileMetadata_EmptyFileName_ThrowsAppException()
    {
        var req = new FileUploadRequest("  ", "application/pdf", 100, null);
        var ex = await Assert.ThrowsAsync<AppException>(() => _sysPlatformSvc.UploadFileMetadataAsync(_tenant, _user, req));
        Assert.Contains("Tên file không được để trống", ex.Message);
    }

    [Fact]
    public async Task UC_SYS_066_UploadFileMetadata_ZeroSizeBytes_ThrowsAppException()
    {
        var req = new FileUploadRequest("empty.txt", "text/plain", 0, null);
        var ex = await Assert.ThrowsAsync<AppException>(() => _sysPlatformSvc.UploadFileMetadataAsync(_tenant, _user, req));
        Assert.Contains("Kích thước file phải lớn hơn 0 byte", ex.Message);
    }

    [Fact]
    public async Task UC_SYS_066_UploadFileMetadata_Exceeds50MBLimit_ThrowsAppException()
    {
        var req = new FileUploadRequest("large_video.mp4", "video/mp4", 51L * 1024 * 1024, null);
        var ex = await Assert.ThrowsAsync<AppException>(() => _sysPlatformSvc.UploadFileMetadataAsync(_tenant, _user, req));
        Assert.Contains("không được vượt quá 50MB", ex.Message);
    }

    [Fact]
    public async Task UC_SYS_066_UploadFileMetadata_NonExistentFolderId_ThrowsAppException()
    {
        var invalidFolderId = Guid.NewGuid();
        var req = new FileUploadRequest("doc.docx", "application/msword", 2048, invalidFolderId);
        var ex = await Assert.ThrowsAsync<AppException>(() => _sysPlatformSvc.UploadFileMetadataAsync(_tenant, _user, req));
        Assert.Contains("Thư mục lưu trữ không tồn tại", ex.Message);
    }

    [Fact]
    public async Task UC_SYS_067_GetFileObject_ExistingFile_ReturnsFileObjectDto()
    {
        var uploaded = await _sysPlatformSvc.UploadFileMetadataAsync(_tenant, _user, new FileUploadRequest("avatar.png", "image/png", 2048, null));

        var fetched = await _sysPlatformSvc.GetFileObjectAsync(_tenant, uploaded.Id);
        Assert.Equal(uploaded.Id, fetched.Id);
        Assert.Equal("avatar.png", fetched.FileName);
    }

    [Fact]
    public async Task UC_SYS_067_GetFileObject_DeletedFile_ThrowsAppException()
    {
        var uploaded = await _sysPlatformSvc.UploadFileMetadataAsync(_tenant, _user, new FileUploadRequest("temp.tmp", "application/octet-stream", 100, null));
        await _sysPlatformSvc.SoftDeleteFileAsync(_tenant, uploaded.Id);

        var ex = await Assert.ThrowsAsync<AppException>(() => _sysPlatformSvc.GetFileObjectAsync(_tenant, uploaded.Id));
        Assert.Contains("File không tồn tại hoặc đã bị xóa", ex.Message);
    }

    [Fact]
    public async Task UC_SYS_067_SoftDeleteAndRestoreFile_TogglesDeletedState()
    {
        var uploaded = await _sysPlatformSvc.UploadFileMetadataAsync(_tenant, _user, new FileUploadRequest("contract.pdf", "application/pdf", 4096, null));

        await _sysPlatformSvc.SoftDeleteFileAsync(_tenant, uploaded.Id);
        var filesDeleted = await _sysPlatformSvc.ListFilesAsync(_tenant, null);
        Assert.True(filesDeleted.First(f => f.Id == uploaded.Id).IsDeleted);

        await _sysPlatformSvc.RestoreFileAsync(_tenant, uploaded.Id);
        var filesRestored = await _sysPlatformSvc.ListFilesAsync(_tenant, null);
        Assert.False(filesRestored.First(f => f.Id == uploaded.Id).IsDeleted);
    }
}
