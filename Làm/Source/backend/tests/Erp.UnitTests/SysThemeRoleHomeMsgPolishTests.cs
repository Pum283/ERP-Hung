using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Sys;
using Erp.Domain.Entities.Sys;
using Erp.Domain.Enums.Sys;
using Erp.Infrastructure.Implementations.Services.Sys;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Erp.UnitTests;

public sealed class SysThemeRoleHomeMsgPolishTests
{
    private static (AppDbContext db, SysThemeRoleHomeMsgService svc, Guid tenantId) Create(string name)
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(name).Options;
        var db = new AppDbContext(opts);
        var tenantId = Guid.NewGuid();
        db.Tenants.Add(new Tenant { Id = tenantId, Code = "T", Name = "Demo Tenant" });
        db.SaveChanges();
        return (db, new SysThemeRoleHomeMsgService(db), tenantId);
    }

    [Fact]
    public async Task Theme_UpsertAndGet_Succeeds()
    {
        var (_, svc, tenantId) = Create(nameof(Theme_UpsertAndGet_Succeeds));
        var dto = await svc.UpsertThemeAsync(tenantId, Guid.NewGuid(),
            new SysThemeUpsertRequest("#0EA5E9", "#F59E0B", "https://cdn.example/favicon.ico"));
        Assert.Equal("#0EA5E9", dto.PrimaryColor);
        Assert.Equal("#F59E0B", dto.AccentColor);
        var got = await svc.GetThemeAsync(tenantId);
        Assert.Equal(dto.PrimaryColor, got.PrimaryColor);
    }

    [Fact]
    public async Task Theme_InvalidColor_Fails()
    {
        var (_, svc, tenantId) = Create(nameof(Theme_InvalidColor_Fails));
        await Assert.ThrowsAsync<AppException>(() =>
            svc.UpsertThemeAsync(tenantId, Guid.NewGuid(), new SysThemeUpsertRequest("blue", null, null)));
    }

    [Fact]
    public async Task RoleHome_UpsertResolveByPriority()
    {
        var (db, svc, tenantId) = Create(nameof(RoleHome_UpsertResolveByPriority));
        var userId = Guid.NewGuid();
        var r1 = new Role { TenantId = tenantId, Code = "HR", Name = "HR", IsActive = true };
        var r2 = new Role { TenantId = tenantId, Code = "FIN", Name = "FIN", IsActive = true };
        db.Roles.AddRange(r1, r2);
        db.UserRoles.AddRange(
            new UserRole { TenantId = tenantId, UserId = userId, RoleId = r1.Id, IsActive = true },
            new UserRole { TenantId = tenantId, UserId = userId, RoleId = r2.Id, IsActive = true });
        await db.SaveChangesAsync();

        await svc.UpsertRoleHomeAsync(tenantId, userId, new SysRoleHomeUpsertRequest(null, r1.Id, "/app/hrm", 20, true, null));
        await svc.UpsertRoleHomeAsync(tenantId, userId, new SysRoleHomeUpsertRequest(null, r2.Id, "/app/fin", 10, true, null));

        var home = await svc.ResolveMyHomeAsync(tenantId, userId);
        Assert.Equal("/app/fin", home.LandingPath);
        Assert.Equal("FIN", home.MatchedRoleCode);
    }

    [Fact]
    public async Task RoleHome_InvalidPath_Fails()
    {
        var (db, svc, tenantId) = Create(nameof(RoleHome_InvalidPath_Fails));
        var role = new Role { TenantId = tenantId, Code = "X", Name = "X" };
        db.Roles.Add(role);
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<AppException>(() =>
            svc.UpsertRoleHomeAsync(tenantId, Guid.NewGuid(),
                new SysRoleHomeUpsertRequest(null, role.Id, "https://evil.com", 1, true, null)));
    }

    [Fact]
    public async Task RoleHome_NoConfig_DefaultsToApp()
    {
        var (_, svc, tenantId) = Create(nameof(RoleHome_NoConfig_DefaultsToApp));
        var home = await svc.ResolveMyHomeAsync(tenantId, Guid.NewGuid());
        Assert.Equal("/app", home.LandingPath);
    }

    [Fact]
    public async Task RoleHome_DuplicateRole_Fails()
    {
        var (db, svc, tenantId) = Create(nameof(RoleHome_DuplicateRole_Fails));
        var role = new Role { TenantId = tenantId, Code = "A", Name = "A" };
        db.Roles.Add(role);
        await db.SaveChangesAsync();
        var uid = Guid.NewGuid();
        await svc.UpsertRoleHomeAsync(tenantId, uid, new SysRoleHomeUpsertRequest(null, role.Id, "/app/sys", 1, true, null));
        await Assert.ThrowsAsync<AppException>(() =>
            svc.UpsertRoleHomeAsync(tenantId, uid, new SysRoleHomeUpsertRequest(null, role.Id, "/app/hrm", 2, true, null)));
    }

    [Fact]
    public async Task Search_FindsMessagesInMyConversationsOnly()
    {
        var (db, svc, tenantId) = Create(nameof(Search_FindsMessagesInMyConversationsOnly));
        var me = Guid.NewGuid();
        var other = Guid.NewGuid();
        var mine = new Conversation { TenantId = tenantId, Kind = "Direct", Title = "Mine" };
        var theirs = new Conversation { TenantId = tenantId, Kind = "Direct", Title = "Theirs" };
        db.Conversations.AddRange(mine, theirs);
        db.ConversationMembers.Add(new ConversationMember { TenantId = tenantId, ConversationId = mine.Id, UserId = me });
        db.Users.Add(new AppUser { Id = me, TenantId = tenantId, Username = "me", DisplayName = "Me", Status = UserStatus.Active });
        db.ChatMessages.AddRange(
            new ChatMessage { TenantId = tenantId, ConversationId = mine.Id, SenderUserId = me, Body = "Hello invoice 123" },
            new ChatMessage { TenantId = tenantId, ConversationId = theirs.Id, SenderUserId = other, Body = "invoice secret" });
        await db.SaveChangesAsync();

        var hits = await svc.SearchMessagesAsync(tenantId, me, "invoice", null, 20);
        Assert.Single(hits);
        Assert.Equal(mine.Id, hits[0].ConversationId);
    }

    [Fact]
    public async Task Search_ShortQuery_Fails()
    {
        var (_, svc, tenantId) = Create(nameof(Search_ShortQuery_Fails));
        await Assert.ThrowsAsync<AppException>(() =>
            svc.SearchMessagesAsync(tenantId, Guid.NewGuid(), "a", null, 10));
    }

    [Fact]
    public async Task Search_ForeignConversation_Forbidden()
    {
        var (db, svc, tenantId) = Create(nameof(Search_ForeignConversation_Forbidden));
        var me = Guid.NewGuid();
        var conv = new Conversation { TenantId = tenantId, Kind = "Group", Title = "G" };
        db.Conversations.Add(conv);
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            svc.SearchMessagesAsync(tenantId, me, "hello", conv.Id, 10));
    }

    [Fact]
    public async Task Mute_WithUntil_AndExpiry()
    {
        var (db, svc, tenantId) = Create(nameof(Mute_WithUntil_AndExpiry));
        var userId = Guid.NewGuid();
        var conv = new Conversation { TenantId = tenantId, Kind = "Direct" };
        db.Conversations.Add(conv);
        db.ConversationMembers.Add(new ConversationMember
        {
            TenantId = tenantId, ConversationId = conv.Id, UserId = userId
        });
        await db.SaveChangesAsync();

        var until = DateTimeOffset.UtcNow.AddHours(2);
        var muted = await svc.SetConversationMuteAsync(tenantId, userId, conv.Id,
            new SysConversationMuteRequest(true, until));
        Assert.True(muted.EffectivelyMuted);

        Assert.True(svc.IsEffectivelyMuted(true, until, DateTimeOffset.UtcNow));
        Assert.False(svc.IsEffectivelyMuted(true, DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow));
        Assert.False(svc.IsEffectivelyMuted(false, null, DateTimeOffset.UtcNow));
    }

    [Fact]
    public async Task Mute_PastUntil_Fails()
    {
        var (db, svc, tenantId) = Create(nameof(Mute_PastUntil_Fails));
        var userId = Guid.NewGuid();
        var conv = new Conversation { TenantId = tenantId, Kind = "Direct" };
        db.Conversations.Add(conv);
        db.ConversationMembers.Add(new ConversationMember
        {
            TenantId = tenantId, ConversationId = conv.Id, UserId = userId
        });
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<AppException>(() =>
            svc.SetConversationMuteAsync(tenantId, userId, conv.Id,
                new SysConversationMuteRequest(true, DateTimeOffset.UtcNow.AddMinutes(-5))));
    }

    [Fact]
    public async Task Mute_Unmute_ClearsUntil()
    {
        var (db, svc, tenantId) = Create(nameof(Mute_Unmute_ClearsUntil));
        var userId = Guid.NewGuid();
        var conv = new Conversation { TenantId = tenantId, Kind = "Direct" };
        db.Conversations.Add(conv);
        db.ConversationMembers.Add(new ConversationMember
        {
            TenantId = tenantId, ConversationId = conv.Id, UserId = userId, Muted = true,
            MuteUntil = DateTimeOffset.UtcNow.AddDays(1)
        });
        await db.SaveChangesAsync();
        var r = await svc.SetConversationMuteAsync(tenantId, userId, conv.Id,
            new SysConversationMuteRequest(false, null));
        Assert.False(r.Muted);
        Assert.Null(r.MuteUntil);
        Assert.False(r.EffectivelyMuted);
    }

    [Fact]
    public async Task Theme_ClearColors_Succeeds()
    {
        var (_, svc, tenantId) = Create(nameof(Theme_ClearColors_Succeeds));
        await svc.UpsertThemeAsync(tenantId, Guid.NewGuid(), new SysThemeUpsertRequest("#111111", "#222222", null));
        var cleared = await svc.UpsertThemeAsync(tenantId, Guid.NewGuid(), new SysThemeUpsertRequest(null, null, null));
        Assert.Null(cleared.PrimaryColor);
        Assert.Null(cleared.AccentColor);
    }

    [Fact]
    public async Task RoleHome_Delete_Succeeds()
    {
        var (db, svc, tenantId) = Create(nameof(RoleHome_Delete_Succeeds));
        var role = new Role { TenantId = tenantId, Code = "Z", Name = "Z" };
        db.Roles.Add(role);
        await db.SaveChangesAsync();
        var row = await svc.UpsertRoleHomeAsync(tenantId, Guid.NewGuid(),
            new SysRoleHomeUpsertRequest(null, role.Id, "/app/sys/users", 5, true, "note"));
        await svc.DeleteRoleHomeAsync(tenantId, row.Id);
        Assert.Empty(await svc.ListRoleHomesAsync(tenantId));
    }

    [Fact]
    public async Task Search_SkipsRecalled()
    {
        var (db, svc, tenantId) = Create(nameof(Search_SkipsRecalled));
        var me = Guid.NewGuid();
        var conv = new Conversation { TenantId = tenantId, Kind = "Direct" };
        db.Conversations.Add(conv);
        db.ConversationMembers.Add(new ConversationMember { TenantId = tenantId, ConversationId = conv.Id, UserId = me });
        db.Users.Add(new AppUser { Id = me, TenantId = tenantId, Username = "u", Status = UserStatus.Active });
        db.ChatMessages.Add(new ChatMessage
        {
            TenantId = tenantId, ConversationId = conv.Id, SenderUserId = me,
            Body = "recalled keyword", RecalledAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
        Assert.Empty(await svc.SearchMessagesAsync(tenantId, me, "keyword", null, 10));
    }
}
