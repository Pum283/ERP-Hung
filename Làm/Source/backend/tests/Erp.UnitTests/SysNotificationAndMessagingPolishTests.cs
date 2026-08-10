using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Sys;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Sys;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class SysNotificationAndMessagingPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly SysPlatformService _sysPlatformSvc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _user = Guid.NewGuid();
    private readonly Guid _orgUnitId = Guid.NewGuid();

    public SysNotificationAndMessagingPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("sys-notif-msg-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);

        var outbox = new OutboxWriter(_db);
        _sysPlatformSvc = new SysPlatformService(_db, outbox);

        // Seed OrgUnit
        var org = new OrgUnit { Id = _orgUnitId, TenantId = _tenant, Code = "CN_HANOI", Name = "Chi nhánh Hà Nội", Path = $"/{_orgUnitId:N}/" };
        _db.OrgUnits.Add(org);
        _db.SaveChangesAsync().GetAwaiter().GetResult();
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task UC_SYS_052_UpsertOrgUnitSetting_CreateBranchSetting_Succeeds()
    {
        await _sysPlatformSvc.UpsertOrgUnitSettingAsync(_tenant, _orgUnitId, "POS_PRINTER", "{\"PrinterName\": \"Epson TM-T88\"}");

        var val = await _sysPlatformSvc.GetSettingValueAsync(_tenant, "POS_PRINTER", _orgUnitId);
        Assert.Equal("{\"PrinterName\": \"Epson TM-T88\"}", val);
    }

    [Fact]
    public async Task UC_SYS_052_GetSettingValue_FallbackToTenantDefault_WhenOrgSettingMissing()
    {
        await _sysPlatformSvc.UpsertSettingAsync(_tenant, "DEFAULT_CURRENCY", "{\"Symbol\": \"VND\"}");

        var val = await _sysPlatformSvc.GetSettingValueAsync(_tenant, "DEFAULT_CURRENCY", _orgUnitId);
        Assert.Equal("{\"Symbol\": \"VND\"}", val);
    }

    [Fact]
    public async Task UC_SYS_052_UpsertOrgUnitSetting_InvalidOrgUnit_ThrowsAppException()
    {
        var invalidOrgId = Guid.NewGuid();
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _sysPlatformSvc.UpsertOrgUnitSettingAsync(_tenant, invalidOrgId, "TAX_RATE", "{\"Rate\": 10}"));

        Assert.Contains("Chi nhánh không tồn tại", ex.Message);
    }

    [Fact]
    public async Task UC_SYS_052_UpsertOrgUnitSetting_EmptyKey_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _sysPlatformSvc.UpsertOrgUnitSettingAsync(_tenant, _orgUnitId, "  ", "{\"Rate\": 10}"));

        Assert.Contains("không được để trống", ex.Message);
    }

    [Fact]
    public async Task UC_SYS_059_InAppNotification_NotifyEvent_ReplacesTemplateVarsAndAddsNotification()
    {
        await _sysPlatformSvc.UpsertNotificationRuleAsync(_tenant, _user, new NotificationRuleDto(
            Guid.Empty, "task.assigned", "Nhiệm vụ mới: {taskTitle}", "Bạn nhận được nhiệm vụ {taskTitle} từ {assigner}", true));

        var vars = new Dictionary<string, string>
        {
            { "taskTitle", "Duyệt Đơn Hàng #1024" },
            { "assigner", "Quản lý Nguyễn Văn A" }
        };

        await _sysPlatformSvc.NotifyEventAsync(_tenant, _user, "task.assigned", "/tasks/1024", vars);

        var list = await _sysPlatformSvc.ListNotificationsAsync(_tenant, _user);
        Assert.Single(list);
        Assert.Equal("Nhiệm vụ mới: Duyệt Đơn Hàng #1024", list[0].Title);
        Assert.Equal("Bạn nhận được nhiệm vụ Duyệt Đơn Hàng #1024 từ Quản lý Nguyễn Văn A", list[0].Body);
        Assert.Equal("/tasks/1024", list[0].Link);
    }

    [Fact]
    public async Task UC_SYS_059_MarkNotificationRead_NonExistentNotification_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _sysPlatformSvc.MarkNotificationReadAsync(_tenant, _user, Guid.NewGuid()));

        Assert.Contains("không tồn tại", ex.Message);
    }

    [Fact]
    public async Task UC_SYS_059_MarkAllNotificationsRead_UpdatesUnreadNotifications()
    {
        _db.AppNotifications.Add(new AppNotification { TenantId = _tenant, UserId = _user, Title = "N1", Body = "B1", IsRead = false });
        _db.AppNotifications.Add(new AppNotification { TenantId = _tenant, UserId = _user, Title = "N2", Body = "B2", IsRead = false });
        await _db.SaveChangesAsync();

        Assert.Equal(2, await _sysPlatformSvc.UnreadNotificationCountAsync(_tenant, _user));

        await _sysPlatformSvc.MarkAllNotificationsReadAsync(_tenant, _user);
        Assert.Equal(0, await _sysPlatformSvc.UnreadNotificationCountAsync(_tenant, _user));
    }

    [Fact]
    public async Task UC_SYS_059_UnreadNotificationCount_ExcludesDeletedAndReadNotifications()
    {
        _db.AppNotifications.Add(new AppNotification { TenantId = _tenant, UserId = _user, Title = "Unread", Body = "B", IsRead = false, IsDeleted = false });
        _db.AppNotifications.Add(new AppNotification { TenantId = _tenant, UserId = _user, Title = "Read", Body = "B", IsRead = true, IsDeleted = false });
        _db.AppNotifications.Add(new AppNotification { TenantId = _tenant, UserId = _user, Title = "Deleted", Body = "B", IsRead = false, IsDeleted = true });
        await _db.SaveChangesAsync();

        var count = await _sysPlatformSvc.UnreadNotificationCountAsync(_tenant, _user);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task UC_SYS_060_SendChannelMessage_EmailChannel_ValidTarget_WritesLogAndOutbox()
    {
        await _sysPlatformSvc.UpsertMessageTemplateAsync(_tenant, _user, new MessageTemplateDto(
            Guid.Empty, "WELCOME_EMAIL", "Email", "Chào mừng {userName}", "Xin chào {userName}, chào mừng bạn đến với ERP!", true));

        var req = new ChannelSendRequest("Email", "WELCOME_EMAIL", "customer@example.com",
            new Dictionary<string, string> { { "userName", "Trần Thị B" } }, "USER_WELCOME");

        var res = await _sysPlatformSvc.SendChannelMessageAsync(_tenant, _user, req);

        Assert.Equal("Email", res.Channel);
        Assert.Equal("customer@example.com", res.Target);
        Assert.Equal("WELCOME_EMAIL", res.TemplateCode);
        Assert.Equal("Chào mừng Trần Thị B", res.Subject);
        Assert.Equal("Xin chào Trần Thị B, chào mừng bạn đến với ERP!", res.Body);
        Assert.Equal("Success", res.Status);

        var log = await _db.IntegrationCallLogs.FirstAsync(x => x.Id == res.LogId);
        Assert.Equal("Email", log.Kind);
        Assert.Equal(200, log.StatusCode);
    }

    [Fact]
    public async Task UC_SYS_060_SendChannelMessage_EmailChannel_InvalidEmailTarget_ThrowsAppException()
    {
        await _sysPlatformSvc.UpsertMessageTemplateAsync(_tenant, _user, new MessageTemplateDto(
            Guid.Empty, "INV_EMAIL", "Email", "Subject", "Body", true));

        var req = new ChannelSendRequest("Email", "INV_EMAIL", "not-an-email", null, null);
        var ex = await Assert.ThrowsAsync<AppException>(() => _sysPlatformSvc.SendChannelMessageAsync(_tenant, _user, req));
        Assert.Contains("Email người nhận không hợp lệ", ex.Message);
    }

    [Fact]
    public async Task UC_SYS_060_UpsertMessageTemplate_EmailChannel_EmptySubject_ThrowsAppException()
    {
        var req = new MessageTemplateDto(Guid.Empty, "TPL_NO_SUBJ", "Email", "  ", "Nội dung", true);
        var ex = await Assert.ThrowsAsync<AppException>(() => _sysPlatformSvc.UpsertMessageTemplateAsync(_tenant, _user, req));
        Assert.Contains("Tiêu đề email không được để trống", ex.Message);
    }

    [Fact]
    public async Task UC_SYS_061_SendChannelMessage_SmsChannel_ValidPhoneTarget_WritesLogAndOutbox()
    {
        await _sysPlatformSvc.UpsertMessageTemplateAsync(_tenant, _user, new MessageTemplateDto(
            Guid.Empty, "OTP_SMS", "Sms", "", "Ma OTP cua ban la {otpCode}", true));

        var req = new ChannelSendRequest("Sms", "OTP_SMS", "0912345678",
            new Dictionary<string, string> { { "otpCode", "987654" } }, "OTP_AUTH");

        var res = await _sysPlatformSvc.SendChannelMessageAsync(_tenant, _user, req);

        Assert.Equal("Sms", res.Channel);
        Assert.Equal("0912345678", res.Target);
        Assert.Equal("Ma OTP cua ban la 987654", res.Body);
        Assert.Equal("Success", res.Status);

        var log = await _db.IntegrationCallLogs.FirstAsync(x => x.Id == res.LogId);
        Assert.Equal("Sms", log.Kind);
    }

    [Fact]
    public async Task UC_SYS_061_SendChannelMessage_SmsChannel_InvalidPhoneTarget_ThrowsAppException()
    {
        await _sysPlatformSvc.UpsertMessageTemplateAsync(_tenant, _user, new MessageTemplateDto(
            Guid.Empty, "SMS_TEST", "Sms", "", "Content", true));

        var req = new ChannelSendRequest("Sms", "SMS_TEST", "123", null, null);
        var ex = await Assert.ThrowsAsync<AppException>(() => _sysPlatformSvc.SendChannelMessageAsync(_tenant, _user, req));
        Assert.Contains("Số điện thoại người nhận không hợp lệ", ex.Message);
    }

    [Fact]
    public async Task UC_SYS_061_SendChannelMessage_NonExistentTemplate_ThrowsAppException()
    {
        var req = new ChannelSendRequest("Email", "NON_EXISTENT", "test@domain.com", null, null);
        var ex = await Assert.ThrowsAsync<AppException>(() => _sysPlatformSvc.SendChannelMessageAsync(_tenant, _user, req));
        Assert.Contains("không tồn tại hoặc đã bị khóa", ex.Message);
    }
}
