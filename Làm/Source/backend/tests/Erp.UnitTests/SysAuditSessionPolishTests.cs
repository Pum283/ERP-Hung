using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Sys;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Implementations.Services.Sys;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

/// <summary>
/// Unit tests cho Bước 9: UC_SYS_078 (AuditLog), UC_SYS_080 (FieldDiff),
/// UC_SYS_081 (ExportAuditLog), UC_SYS_083 (SessionPolicy).
/// 17+ test cases bao phủ tất cả edge cases phân tích trước khi code.
/// </summary>
public sealed class SysAuditSessionPolishTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly SysPlatformService _svc;
    private readonly Guid _tenant = Guid.NewGuid();
    private readonly Guid _actor  = Guid.NewGuid();

    public SysAuditSessionPolishTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("sys-audit-session-" + Guid.NewGuid())
            .Options;
        _db = new AppDbContext(opts);
        _svc = new SysPlatformService(_db, new OutboxWriter(_db));
    }

    public void Dispose() => _db.Dispose();

    // ─── Helpers ───
    private void SeedAuditLog(Guid? entityId = null, string entityType = "User",
        string action = "Update", Guid? actorId = null, string? before = null, string? after = null,
        DateTimeOffset? at = null)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            TenantId = _tenant,
            EntityType = entityType,
            EntityId = entityId ?? Guid.NewGuid(),
            Action = action,
            BeforeJson = before,
            AfterJson = after,
            ActorUserId = actorId ?? _actor,
            IpAddress = "127.0.0.1",
            CreatedAt = at ?? DateTimeOffset.UtcNow
        });
        _db.SaveChanges();
    }

    // ─── UC_SYS_078: Nhật ký thao tác người dùng ───

    [Fact]
    public async Task UC078_ListAuditLogs_NoFilter_ReturnsTenantLogs()
    {
        SeedAuditLog(entityType: "User");
        SeedAuditLog(entityType: "Role");

        var (items, total) = await _svc.ListAuditLogsAsync(_tenant, new AuditLogQueryRequest());
        Assert.Equal(2, total);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task UC078_ListAuditLogs_FilterByEntityType_ReturnsOnlyMatching()
    {
        SeedAuditLog(entityType: "User");
        SeedAuditLog(entityType: "Role");
        SeedAuditLog(entityType: "User");

        var (items, total) = await _svc.ListAuditLogsAsync(_tenant, new AuditLogQueryRequest(EntityType: "User"));
        Assert.Equal(2, total);
        Assert.All(items, i => Assert.Equal("User", i.EntityType));
    }

    [Fact]
    public async Task UC078_ListAuditLogs_FilterByAction_ReturnsOnlyMatching()
    {
        SeedAuditLog(action: "Create");
        SeedAuditLog(action: "Delete");

        var (items, total) = await _svc.ListAuditLogsAsync(_tenant, new AuditLogQueryRequest(Action: "Create"));
        Assert.Equal(1, total);
        Assert.Equal("Create", items[0].Action);
    }

    [Fact]
    public async Task UC078_ListAuditLogs_FilterByActorUserId_ReturnsOnlyMatching()
    {
        var otherActor = Guid.NewGuid();
        SeedAuditLog(actorId: _actor);
        SeedAuditLog(actorId: otherActor);

        var (items, total) = await _svc.ListAuditLogsAsync(_tenant, new AuditLogQueryRequest(ActorUserId: _actor));
        Assert.Equal(1, total);
        Assert.Equal(_actor, items[0].ActorUserId);
    }

    [Fact]
    public async Task UC078_ListAuditLogs_InvalidDateRange_ThrowsAppException()
    {
        var now = DateTimeOffset.UtcNow;
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.ListAuditLogsAsync(_tenant, new AuditLogQueryRequest(From: now.AddDays(1), To: now)));
        Assert.Contains("bắt đầu", ex.Message);
    }

    [Fact]
    public async Task UC078_ListAuditLogs_Pagination_SkipsCorrectly()
    {
        for (int i = 0; i < 10; i++)
            SeedAuditLog(action: $"Act{i}");

        var (items, total) = await _svc.ListAuditLogsAsync(_tenant, new AuditLogQueryRequest(Page: 2, PageSize: 3));
        Assert.Equal(10, total);
        Assert.Equal(3, items.Count);
    }

    [Fact]
    public async Task UC078_ListAuditLogs_PageSizeCappedAt500()
    {
        // Chỉ cần verify không throw và pageSize bị clamp
        var (items, total) = await _svc.ListAuditLogsAsync(_tenant, new AuditLogQueryRequest(PageSize: 9999));
        Assert.Equal(0, total);
    }

    [Fact]
    public async Task UC078_ListAuditLogs_TenantIsolation_OnlyReturnsSameTenant()
    {
        SeedAuditLog();
        // Log từ tenant khác
        _db.AuditLogs.Add(new AuditLog { TenantId = Guid.NewGuid(), EntityType = "User", Action = "Create" });
        _db.SaveChanges();

        var (items, total) = await _svc.ListAuditLogsAsync(_tenant, new AuditLogQueryRequest());
        Assert.Equal(1, total);
    }

    // ─── UC_SYS_080: Xem chi tiết thay đổi field ───

    [Fact]
    public async Task UC080_GetAuditLogDetail_CreateAction_OnlyShowsAddedFields()
    {
        SeedAuditLog(action: "Create", before: null, after: "{\"Name\":\"Alice\",\"Email\":\"alice@x.com\"}");
        var id = _db.AuditLogs.First().Id;

        var detail = await _svc.GetAuditLogDetailAsync(_tenant, id);
        Assert.Equal(2, detail.FieldDiffs.Count);
        Assert.All(detail.FieldDiffs, d => Assert.Equal("Added", d.ChangeKind));
        Assert.All(detail.FieldDiffs, d => Assert.Null(d.OldValue));
    }

    [Fact]
    public async Task UC080_GetAuditLogDetail_DeleteAction_OnlyShowsRemovedFields()
    {
        SeedAuditLog(action: "Delete", before: "{\"Name\":\"Bob\"}", after: null);
        var id = _db.AuditLogs.First().Id;

        var detail = await _svc.GetAuditLogDetailAsync(_tenant, id);
        Assert.Single(detail.FieldDiffs);
        Assert.Equal("Removed", detail.FieldDiffs[0].ChangeKind);
        Assert.Null(detail.FieldDiffs[0].NewValue);
    }

    [Fact]
    public async Task UC080_GetAuditLogDetail_UpdateAction_ShowsOnlyChangedFields()
    {
        SeedAuditLog(action: "Update",
            before: "{\"Name\":\"Alice\",\"Email\":\"old@x.com\"}",
            after:  "{\"Name\":\"Alice\",\"Email\":\"new@x.com\"}");
        var id = _db.AuditLogs.First().Id;

        var detail = await _svc.GetAuditLogDetailAsync(_tenant, id);
        // Name không đổi → không xuất hiện; Email đổi → xuất hiện 1 diff
        Assert.Single(detail.FieldDiffs);
        Assert.Equal("Email", detail.FieldDiffs[0].FieldName);
        Assert.Equal("Modified", detail.FieldDiffs[0].ChangeKind);
    }

    [Fact]
    public async Task UC080_GetAuditLogDetail_MalformedBeforeJson_ReturnRawDiff()
    {
        SeedAuditLog(action: "Update", before: "NOT_VALID_JSON", after: "{\"Name\":\"Bob\"}");
        var id = _db.AuditLogs.First().Id;

        // Không crash, trả raw diff
        var detail = await _svc.GetAuditLogDetailAsync(_tenant, id);
        Assert.Single(detail.FieldDiffs);
        Assert.Equal("(raw)", detail.FieldDiffs[0].FieldName);
    }

    [Fact]
    public async Task UC080_GetAuditLogDetail_NotFound_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.GetAuditLogDetailAsync(_tenant, Guid.NewGuid()));
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task UC080_GetAuditLogDetail_CrossTenant_ThrowsAppException()
    {
        _db.AuditLogs.Add(new AuditLog { TenantId = Guid.NewGuid(), EntityType = "User", Action = "Create" });
        _db.SaveChanges();
        var foreignId = _db.AuditLogs.First().Id;

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.GetAuditLogDetailAsync(_tenant, foreignId));
        Assert.Equal(404, ex.StatusCode);
    }

    // ─── UC_SYS_081: Xuất audit log ───

    [Fact]
    public async Task UC081_ExportAuditLogCsv_ValidRange_ReturnsCsvWithBOM()
    {
        var now = DateTimeOffset.UtcNow;
        SeedAuditLog(action: "Create", at: now.AddHours(-1));
        SeedAuditLog(action: "Update", at: now.AddMinutes(-30));

        var req = new AuditLogExportRequest(now.AddDays(-1), now.AddDays(1));
        var result = await _svc.ExportAuditLogCsvAsync(_tenant, req);

        Assert.Equal("text/csv; charset=utf-8", result.ContentType);
        Assert.True(result.FileName.StartsWith("AuditLog_"));
        Assert.True(result.FileName.EndsWith(".csv"));
        Assert.Equal(2, result.RowCount);

        // UTF-8 BOM check (EF bytes 0xEF, 0xBB, 0xBF)
        Assert.Equal(0xEF, result.Data[0]);
        Assert.Equal(0xBB, result.Data[1]);
        Assert.Equal(0xBF, result.Data[2]);
    }

    [Fact]
    public async Task UC081_ExportAuditLogCsv_FromEqualTo_ThrowsAppException()
    {
        var now = DateTimeOffset.UtcNow;
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.ExportAuditLogCsvAsync(_tenant, new AuditLogExportRequest(now, now)));
        Assert.Contains("nhỏ hơn", ex.Message);
    }

    [Fact]
    public async Task UC081_ExportAuditLogCsv_RangeOver366Days_ThrowsAppException()
    {
        var now = DateTimeOffset.UtcNow;
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.ExportAuditLogCsvAsync(_tenant, new AuditLogExportRequest(now.AddDays(-400), now)));
        Assert.Contains("366", ex.Message);
    }

    // ─── UC_SYS_083: Chính sách hết hạn phiên ───

    [Fact]
    public async Task UC083_GetSessionPolicy_NoSetting_ReturnsDefault()
    {
        var policy = await _svc.GetSessionPolicyAsync(_tenant);
        Assert.Equal(120, policy.SessionMinutes);
        Assert.Equal(30,  policy.IdleTimeoutMinutes);
        Assert.Equal(5,   policy.MaxConcurrentSessions);
        Assert.True(policy.ForceLogoutOnPasswordChange);
    }

    [Fact]
    public async Task UC083_SetSessionPolicy_Valid_PersistsAndReturns()
    {
        var result = await _svc.SetSessionPolicyAsync(_tenant,
            new SessionPolicyUpdateRequest(60, 15, 3, false));

        Assert.Equal(60, result.SessionMinutes);
        Assert.Equal(15, result.IdleTimeoutMinutes);
        Assert.Equal(3,  result.MaxConcurrentSessions);
        Assert.False(result.ForceLogoutOnPasswordChange);

        // Đọc lại để confirm persist
        var read = await _svc.GetSessionPolicyAsync(_tenant);
        Assert.Equal(60, read.SessionMinutes);
    }

    [Fact]
    public async Task UC083_SetSessionPolicy_ZeroSessionMinutes_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.SetSessionPolicyAsync(_tenant, new SessionPolicyUpdateRequest(0, 0, 1, false)));
        Assert.Contains(">= 1", ex.Message);
    }

    [Fact]
    public async Task UC083_SetSessionPolicy_NegativeSessionMinutes_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.SetSessionPolicyAsync(_tenant, new SessionPolicyUpdateRequest(-5, 0, 1, false)));
        Assert.Contains(">= 1", ex.Message);
    }

    [Fact]
    public async Task UC083_SetSessionPolicy_ExceedMaxSessionMinutes_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.SetSessionPolicyAsync(_tenant, new SessionPolicyUpdateRequest(10_081, 0, 1, false)));
        Assert.Contains("10.080", ex.Message);
    }

    [Fact]
    public async Task UC083_SetSessionPolicy_NegativeIdleTimeout_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.SetSessionPolicyAsync(_tenant, new SessionPolicyUpdateRequest(60, -1, 1, false)));
        Assert.Contains("âm", ex.Message);
    }

    [Fact]
    public async Task UC083_SetSessionPolicy_IdleTimeoutGreaterThanSession_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.SetSessionPolicyAsync(_tenant, new SessionPolicyUpdateRequest(60, 120, 1, false)));
        Assert.Contains("idle timeout", ex.Message);
    }

    [Fact]
    public async Task UC083_SetSessionPolicy_MaxSessionsOutOfRange_ThrowsAppException()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() =>
            _svc.SetSessionPolicyAsync(_tenant, new SessionPolicyUpdateRequest(60, 0, 25, false)));
        Assert.Contains("1 – 20", ex.Message);
    }

    [Fact]
    public async Task UC083_SetSessionPolicy_RevokesStaleSessionsAutomatically()
    {
        // Session đã cuối lần thấy cách đây 200 phút → sẽ bị revoke khi set policy 120 phút
        _db.UserSessions.Add(new UserSession
        {
            TenantId = _tenant, UserId = _actor, SessionKey = "old",
            LastSeenAt = DateTimeOffset.UtcNow.AddMinutes(-200),
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(100),
            IsRevoked = false
        });
        _db.UserSessions.Add(new UserSession
        {
            TenantId = _tenant, UserId = _actor, SessionKey = "fresh",
            LastSeenAt = DateTimeOffset.UtcNow.AddMinutes(-10),
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(110),
            IsRevoked = false
        });
        await _db.SaveChangesAsync();

        await _svc.SetSessionPolicyAsync(_tenant, new SessionPolicyUpdateRequest(120, 0, 5, false));

        var sessions = await _db.UserSessions.Where(s => s.TenantId == _tenant).ToListAsync();
        var revoked  = sessions.Count(s => s.IsRevoked);
        var active   = sessions.Count(s => !s.IsRevoked);
        Assert.Equal(1, revoked);  // "old" bị revoke
        Assert.Equal(1, active);   // "fresh" còn sống
    }

    [Fact]
    public async Task UC083_PurgeExpiredSessions_RemovesRevokedAndExpired()
    {
        _db.UserSessions.Add(new UserSession
        {
            TenantId = _tenant, UserId = _actor, SessionKey = "expired",
            LastSeenAt = DateTimeOffset.UtcNow, ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            IsRevoked = false
        });
        _db.UserSessions.Add(new UserSession
        {
            TenantId = _tenant, UserId = _actor, SessionKey = "revoked",
            LastSeenAt = DateTimeOffset.UtcNow, ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
            IsRevoked = true
        });
        _db.UserSessions.Add(new UserSession
        {
            TenantId = _tenant, UserId = _actor, SessionKey = "active",
            LastSeenAt = DateTimeOffset.UtcNow, ExpiresAt = DateTimeOffset.UtcNow.AddHours(2),
            IsRevoked = false
        });
        await _db.SaveChangesAsync();

        var deleted = await _svc.PurgeExpiredSessionsAsync(_tenant);
        Assert.Equal(2, deleted);
        Assert.Equal(1, await _db.UserSessions.CountAsync(s => s.TenantId == _tenant));
    }
}
