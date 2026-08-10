using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Sys;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Sys;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 10: UC_SYS_087 (Outbox Queue), UC_SYS_088 (Email Gateway),
/// UC_SYS_089 (SMS Gateway), UC_SYS_101 (Chat Message Attachments).
/// 20+ test cases bao phủ đầy đủ luồng thành công và luồng lỗi.
/// </summary>
public sealed class SysOutboxGatewaysChatPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly SysPlatformService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _user   = Guid.NewGuid();

    public SysOutboxGatewaysChatPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("sys-outbox-gateways-chat-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new SysPlatformService(_db, new OutboxWriter(_db));
    }

    public void Dispose() => _db.Dispose();

    // ─── UC_SYS_087: Hàng đợi sự kiện liên module (Outbox Queue) ───

    [Fact]
    public async Task UC087_EnqueueOutbox_ValidPayload_CreatesPendingMessage()
    {
        var res = await _svc.EnqueueOutboxAsync(_tenant,
            new EnqueueOutboxRequest("ORDER.CREATED", "CRM", "{\"orderId\":\"123\"}"));

        Assert.Equal("ORDER.CREATED", res.EventType);
        Assert.Equal("CRM", res.SourceModule);
        Assert.Equal("Pending", res.Status);
        Assert.Equal(0, res.AttemptCount);
    }

    [Fact]
    public async Task UC087_EnqueueOutbox_EmptyEventType_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.EnqueueOutboxAsync(_tenant, new EnqueueOutboxRequest("", "CRM", "{}")));
        Assert.Contains("EventType", ex.Message);
    }

    [Fact]
    public async Task UC087_EnqueueOutbox_InvalidJson_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.EnqueueOutboxAsync(_tenant, new EnqueueOutboxRequest("EVT", "CRM", "INVALID_JSON")));
        Assert.Contains("JSON", ex.Message);
    }

    [Fact]
    public async Task UC087_ProcessOutboxQueue_ProcessesPendingItems()
    {
        await _svc.EnqueueOutboxAsync(_tenant, new EnqueueOutboxRequest("EVT1", "CRM", "{\"id\":1}"));
        await _svc.EnqueueOutboxAsync(_tenant, new EnqueueOutboxRequest("EVT2", "INV", "{\"id\":2}"));

        var result = await _svc.ProcessOutboxQueueAsync(_tenant);
        Assert.Equal(2, result.ProcessedCount);
        Assert.Equal(2, result.SuccessCount);
        Assert.Equal(0, result.FailedCount);

        var (items, _) = await _svc.ListOutboxMessagesAsync(_tenant, new OutboxQueryRequest(Status: "Published"));
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task UC087_ProcessOutboxQueue_SimulateError_MarksFailed()
    {
        await _svc.EnqueueOutboxAsync(_tenant, new EnqueueOutboxRequest("EVT_ERR", "CRM", "{\"action\":\"simulate_error\"}"));

        var result = await _svc.ProcessOutboxQueueAsync(_tenant);
        Assert.Equal(1, result.ProcessedCount);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(1, result.FailedCount);

        var (items, _) = await _svc.ListOutboxMessagesAsync(_tenant, new OutboxQueryRequest(Status: "Failed"));
        Assert.Single(items);
        Assert.Equal(1, items[0].AttemptCount);
    }

    [Fact]
    public async Task UC087_RetryOutboxMessage_ResetsFailedMessageToPending()
    {
        var msg = await _svc.EnqueueOutboxAsync(_tenant, new EnqueueOutboxRequest("EVT_ERR", "CRM", "{\"action\":\"simulate_error\"}"));
        await _svc.ProcessOutboxQueueAsync(_tenant);

        await _svc.RetryOutboxMessageAsync(_tenant, msg.Id);

        var (items, _) = await _svc.ListOutboxMessagesAsync(_tenant, new OutboxQueryRequest(Status: "Pending"));
        Assert.Single(items);
        Assert.Equal(0, items[0].AttemptCount);
    }

    // ─── UC_SYS_088: Kết nối Email Gateway ───

    [Fact]
    public async Task UC088_UpsertEmailGateway_ValidSmtp_CreatesGateway()
    {
        var cfg = new EmailGatewayConfigDto("Smtp", "smtp.gmail.com", 587, true, "no-reply@erp.vn", "ERP System", null, "user@erp.vn");
        var gw = await _svc.UpsertEmailGatewayAsync(_tenant, _user, new UpsertEmailGatewayRequest("GW_SMTP", "Gmail Gateway", cfg));

        Assert.Equal("GW_SMTP", gw.Code);
        Assert.Equal("EmailGateway", gw.Kind);
        Assert.True(gw.IsActive);
    }

    [Fact]
    public async Task UC088_UpsertEmailGateway_InvalidProvider_ThrowsAppException()
    {
        var cfg = new EmailGatewayConfigDto("MailChimp", "smtp.gmail.com", 587, true, "no-reply@erp.vn", "ERP", null, null);
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertEmailGatewayAsync(_tenant, _user, new UpsertEmailGatewayRequest("GW_BAD", "Bad", cfg)));
        Assert.Contains("Smtp, SendGrid, AmazonSES", ex.Message);
    }

    [Fact]
    public async Task UC088_TestEmailGateway_ActiveGateway_ReturnsSuccessResult()
    {
        var cfg = new EmailGatewayConfigDto("SendGrid", "", 0, true, "info@company.com", "Company", "sg_key", null);
        var gw = await _svc.UpsertEmailGatewayAsync(_tenant, _user, new UpsertEmailGatewayRequest("GW_SG", "SendGrid", cfg));

        var res = await _svc.TestEmailGatewayAsync(_tenant, gw.Id);
        Assert.True(res.Success);
        Assert.Contains("thành công", res.Message);
    }

    [Fact]
    public async Task UC088_SendTestEmail_ValidEmail_ReturnsSuccessResult()
    {
        var cfg = new EmailGatewayConfigDto("Smtp", "mail.server.com", 25, false, "admin@test.com", "Admin", null, null);
        var gw = await _svc.UpsertEmailGatewayAsync(_tenant, _user, new UpsertEmailGatewayRequest("GW_TEST", "Test", cfg));

        var sendRes = await _svc.SendTestEmailAsync(_tenant, _user, new SendTestEmailRequest(gw.Id, "user@domain.com", "Test Subject", "Test Body"));
        Assert.Equal("Success", sendRes.Status);
        Assert.Equal("user@domain.com", sendRes.Target);
    }

    [Fact]
    public async Task UC088_SendTestEmail_InvalidEmail_ThrowsAppException()
    {
        var cfg = new EmailGatewayConfigDto("Smtp", "mail.server.com", 25, false, "admin@test.com", "Admin", null, null);
        var gw = await _svc.UpsertEmailGatewayAsync(_tenant, _user, new UpsertEmailGatewayRequest("GW_TEST2", "Test", cfg));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.SendTestEmailAsync(_tenant, _user, new SendTestEmailRequest(gw.Id, "invalid-email-address", "Subj", "Body")));
        Assert.Contains("Email", ex.Message);
    }

    // ─── UC_SYS_089: Kết nối SMS Gateway ───

    [Fact]
    public async Task UC089_UpsertSmsGateway_ValidVietGuys_CreatesGateway()
    {
        var cfg = new SmsGatewayConfigDto("VietGuys", "ERP_BRAND", "vietguys_user", "vg_secret_key", "https://api.vietguys.biz");
        var gw = await _svc.UpsertSmsGatewayAsync(_tenant, _user, new UpsertSmsGatewayRequest("GW_VG", "VietGuys SMS", cfg));

        Assert.Equal("GW_VG", gw.Code);
        Assert.Equal("SmsGateway", gw.Kind);
    }

    [Fact]
    public async Task UC089_UpsertSmsGateway_EmptySenderId_ThrowsAppException()
    {
        var cfg = new SmsGatewayConfigDto("Twilio", "", "sid", "token", null);
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.UpsertSmsGatewayAsync(_tenant, _user, new UpsertSmsGatewayRequest("GW_BAD_SMS", "Bad", cfg)));
        Assert.Contains("SenderId", ex.Message);
    }

    [Fact]
    public async Task UC089_TestSmsGateway_ActiveGateway_ReturnsSuccessResult()
    {
        var cfg = new SmsGatewayConfigDto("eSMS", "ERP_NOTIFY", "esms_key", "esms_secret", null);
        var gw = await _svc.UpsertSmsGatewayAsync(_tenant, _user, new UpsertSmsGatewayRequest("GW_ESMS", "eSMS", cfg));

        var res = await _svc.TestSmsGatewayAsync(_tenant, gw.Id);
        Assert.True(res.Success);
        Assert.Contains("ERP_NOTIFY", res.Message);
    }

    [Fact]
    public async Task UC089_SendTestSms_ValidPhone_ReturnsSuccessResult()
    {
        var cfg = new SmsGatewayConfigDto("SpeedSMS", "SPEED_BRAND", "speed_user", "speed_pass", null);
        var gw = await _svc.UpsertSmsGatewayAsync(_tenant, _user, new UpsertSmsGatewayRequest("GW_SPEED", "SpeedSMS", cfg));

        var res = await _svc.SendTestSmsAsync(_tenant, _user, new SendTestSmsRequest(gw.Id, "0901234567", "Mã OTP test là 123456"));
        Assert.Equal("Success", res.Status);
        Assert.Equal("0901234567", res.Target);
    }

    // ─── UC_SYS_101: Đính kèm file trong tin nhắn ───

    [Fact]
    public async Task UC101_SendChatMessage_WithValidFileAttachment_CreatesMessageAndLinksFile()
    {
        var convId = Guid.NewGuid();
        _db.Conversations.Add(new Conversation { Id = convId, TenantId = _tenant, Title = "General Chat" });
        await _db.SaveChangesAsync();

        var file = await _svc.UploadFileMetadataAsync(_tenant, _user, new FileUploadRequest("report.pdf", "application/pdf", 1024 * 1024));

        var msg = await _svc.SendChatMessageAsync(_tenant, _user, new SendChatMessageRequest(convId, "Gửi báo cáo nè", file.Id));

        Assert.Equal(convId, msg.ConversationId);
        Assert.Equal("Gửi báo cáo nè", msg.Body);
        Assert.Equal(file.Id, msg.AttachmentFileId);
        Assert.Equal("report.pdf", msg.AttachmentFileName);

        // Verify auto-linked entity
        var dbFile = await _db.FileObjects.FirstAsync(f => f.Id == file.Id);
        Assert.Equal("ChatMessage", dbFile.LinkedEntityType);
        Assert.Equal(msg.Id, dbFile.LinkedEntityId);
    }

    [Fact]
    public async Task UC101_SendChatMessage_ForbiddenExtension_ThrowsAppException()
    {
        var convId = Guid.NewGuid();
        _db.Conversations.Add(new Conversation { Id = convId, TenantId = _tenant, Title = "General Chat" });
        await _db.SaveChangesAsync();

        var file = await _svc.UploadFileMetadataAsync(_tenant, _user, new FileUploadRequest("virus.exe", "application/x-msdownload", 512));

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.SendChatMessageAsync(_tenant, _user, new SendChatMessageRequest(convId, "Chạy file này đi", file.Id)));
        Assert.Contains("bảo mật", ex.Message);
    }

    [Fact]
    public async Task UC101_SendChatMessage_OversizedFile_ThrowsAppException()
    {
        var convId = Guid.NewGuid();
        _db.Conversations.Add(new Conversation { Id = convId, TenantId = _tenant, Title = "General Chat" });
        await _db.SaveChangesAsync();

        var file = await _svc.UploadFileMetadataAsync(_tenant, _user, new FileUploadRequest("big_video.mp4", "video/mp4", 30 * 1024 * 1024)); // 30MB

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.SendChatMessageAsync(_tenant, _user, new SendChatMessageRequest(convId, "Xem clip", file.Id)));
        Assert.Contains("25MB", ex.Message);
    }

    [Fact]
    public async Task UC101_SendChatMessage_EmptyTextAndNoAttachment_ThrowsAppException()
    {
        var convId = Guid.NewGuid();
        _db.Conversations.Add(new Conversation { Id = convId, TenantId = _tenant, Title = "General Chat" });
        await _db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.SendChatMessageAsync(_tenant, _user, new SendChatMessageRequest(convId, "   ", null)));
        Assert.Contains("văn bản hoặc file", ex.Message);
    }

    [Fact]
    public async Task UC101_ListChatMessages_RecalledMessage_HidesAttachmentDetails()
    {
        var convId = Guid.NewGuid();
        _db.Conversations.Add(new Conversation { Id = convId, TenantId = _tenant, Title = "General Chat" });
        await _db.SaveChangesAsync();

        var file = await _svc.UploadFileMetadataAsync(_tenant, _user, new FileUploadRequest("secret.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", 2048));
        var msg = await _svc.SendChatMessageAsync(_tenant, _user, new SendChatMessageRequest(convId, "Tin nhắn nhạy cảm", file.Id));

        await _svc.RecallChatMessageAsync(_tenant, _user, msg.Id);

        var list = await _svc.ListChatMessagesAsync(_tenant, convId);
        Assert.Single(list);
        Assert.Equal("Tin nhắn đã bị thu hồi", list[0].Body);
        Assert.Null(list[0].AttachmentFileId);
        Assert.Null(list[0].AttachmentFileName);
    }

    [Fact]
    public async Task UC101_RecallChatMessage_NotSender_ThrowsAppException()
    {
        var convId = Guid.NewGuid();
        _db.Conversations.Add(new Conversation { Id = convId, TenantId = _tenant, Title = "General Chat" });
        await _db.SaveChangesAsync();

        var msg = await _svc.SendChatMessageAsync(_tenant, _user, new SendChatMessageRequest(convId, "Tin nhắn của user 1"));

        var otherUser = Guid.NewGuid();
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.RecallChatMessageAsync(_tenant, otherUser, msg.Id));
        Assert.Equal(403, ex.StatusCode);
    }
}
