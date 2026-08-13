using System.Text.RegularExpressions;
using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Sys;
using Erp.Application.Interfaces.Services.Sys;
using Erp.Domain.Entities.Sys;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Sys;

/// <summary>
/// Bước 155 — UC_SYS_093 (theme), 094 (role home), 103 (msg search), 104 (conv mute).
/// </summary>
public sealed class SysThemeRoleHomeMsgService : ISysThemeRoleHomeMsgService
{
    private static readonly Regex HexColor = new(@"^#([0-9A-Fa-f]{6}|[0-9A-Fa-f]{3})$", RegexOptions.Compiled);
    private static readonly Regex LandingPath = new(@"^/app(/[\w\-/]*)?$", RegexOptions.Compiled);

    private readonly AppDbContext _db;

    public SysThemeRoleHomeMsgService(AppDbContext db) => _db = db;

    // ── 093 ─────────────────────────────────────────────────────────────────

    public async Task<SysThemeDto> GetThemeAsync(Guid tenantId, CancellationToken ct = default)
    {
        var t = await _db.Tenants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Tenant không tồn tại.", 404);
        return MapTheme(t);
    }

    public async Task<SysThemeDto> UpsertThemeAsync(
        Guid tenantId, Guid userId, SysThemeUpsertRequest req, CancellationToken ct = default)
    {
        var t = await _db.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId && !x.IsDeleted, ct)
                ?? throw new AppException("Tenant không tồn tại.", 404);

        t.PrimaryColor = NormalizeColor(req.PrimaryColor, allowNull: true);
        t.AccentColor = NormalizeColor(req.AccentColor, allowNull: true);
        if (req.FaviconUrl is not null)
            t.FaviconUrl = string.IsNullOrWhiteSpace(req.FaviconUrl) ? null : req.FaviconUrl.Trim();
        t.UpdatedAt = DateTimeOffset.UtcNow;
        t.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return MapTheme(t);
    }

    // ── 094 ─────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<SysRoleHomeDto>> ListRoleHomesAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await (
            from h in _db.SysRoleHomeConfigs.AsNoTracking()
            join r in _db.Roles.AsNoTracking() on h.RoleId equals r.Id
            where h.TenantId == tenantId && !h.IsDeleted && !r.IsDeleted
            orderby h.Priority, r.Code
            select new SysRoleHomeDto(h.Id, h.RoleId, r.Code, r.Name, h.LandingPath, h.Priority, h.IsActive, h.Note)
        ).ToListAsync(ct);
    }

    public async Task<SysRoleHomeDto> UpsertRoleHomeAsync(
        Guid tenantId, Guid userId, SysRoleHomeUpsertRequest req, CancellationToken ct = default)
    {
        var path = (req.LandingPath ?? "").Trim();
        if (!LandingPath.IsMatch(path))
            throw new AppException("LandingPath phải bắt đầu bằng /app (vd. /app/hrm).");
        if (path.Length > 200) throw new AppException("LandingPath tối đa 200 ký tự.");
        if (req.Priority < 0 || req.Priority > 9999)
            throw new AppException("Priority phải trong 0–9999.");

        var role = await _db.Roles.AsNoTracking()
                       .FirstOrDefaultAsync(r => r.Id == req.RoleId && r.TenantId == tenantId && !r.IsDeleted, ct)
                   ?? throw new AppException("Vai trò không tồn tại.", 404);

        SysRoleHomeConfig entity;
        if (req.Id is Guid id)
        {
            entity = await _db.SysRoleHomeConfigs.FirstOrDefaultAsync(
                         x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                     ?? throw new AppException("Cấu hình trang chủ không tồn tại.", 404);
        }
        else
        {
            var clash = await _db.SysRoleHomeConfigs.AnyAsync(
                x => x.TenantId == tenantId && x.RoleId == req.RoleId && !x.IsDeleted, ct);
            if (clash) throw new AppException("Vai trò này đã có cấu hình trang chủ.");
            entity = new SysRoleHomeConfig { TenantId = tenantId, RoleId = req.RoleId, CreatedBy = userId };
            _db.SysRoleHomeConfigs.Add(entity);
        }

        entity.RoleId = req.RoleId;
        entity.LandingPath = path;
        entity.Priority = req.Priority;
        entity.IsActive = req.IsActive;
        entity.Note = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim();
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);
        return new SysRoleHomeDto(entity.Id, entity.RoleId, role.Code, role.Name,
            entity.LandingPath, entity.Priority, entity.IsActive, entity.Note);
    }

    public async Task DeleteRoleHomeAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var entity = await _db.SysRoleHomeConfigs.FirstOrDefaultAsync(
                         x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct)
                     ?? throw new AppException("Cấu hình trang chủ không tồn tại.", 404);
        entity.IsDeleted = true;
        entity.DeletedAt = DateTimeOffset.UtcNow;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<SysMyHomeDto> ResolveMyHomeAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var roleIds = await (
            from ur in _db.UserRoles.AsNoTracking()
            join r in _db.Roles.AsNoTracking() on ur.RoleId equals r.Id
            where ur.UserId == userId && ur.TenantId == tenantId
                  && ur.IsActive && !ur.IsDeleted && ur.RevokedAt == null
                  && (ur.ValidFrom == null || ur.ValidFrom <= now)
                  && (ur.ValidTo == null || ur.ValidTo >= now)
                  && r.IsActive && !r.IsDeleted
            select new { r.Id, r.Code }
        ).ToListAsync(ct);

        if (roleIds.Count == 0)
            return new SysMyHomeDto("/app", null, null);

        var ids = roleIds.Select(x => x.Id).ToList();
        var homes = await _db.SysRoleHomeConfigs.AsNoTracking()
            .Where(h => h.TenantId == tenantId && h.IsActive && !h.IsDeleted && ids.Contains(h.RoleId))
            .OrderBy(h => h.Priority)
            .ToListAsync(ct);

        if (homes.Count == 0)
            return new SysMyHomeDto("/app", null, null);

        var best = homes[0];
        var code = roleIds.First(r => r.Id == best.RoleId).Code;
        return new SysMyHomeDto(best.LandingPath, code, best.Priority);
    }

    // ── 103 ─────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<SysMessageSearchHitDto>> SearchMessagesAsync(
        Guid tenantId, Guid userId, string query, Guid? conversationId, int take, CancellationToken ct = default)
    {
        var q = (query ?? "").Trim();
        if (q.Length < 2)
            throw new AppException("Từ khóa tìm kiếm tối thiểu 2 ký tự.");
        if (q.Length > 200)
            throw new AppException("Từ khóa tối đa 200 ký tự.");
        if (take <= 0) take = 20;
        if (take > 100) take = 100;

        var myConvIds = await _db.ConversationMembers.AsNoTracking()
            .Where(m => m.TenantId == tenantId && m.UserId == userId && !m.IsDeleted)
            .Select(m => m.ConversationId)
            .ToListAsync(ct);

        if (conversationId is Guid cid)
        {
            if (!myConvIds.Contains(cid))
                throw new ForbiddenException("Bạn không thuộc hội thoại này.");
            myConvIds = new List<Guid> { cid };
        }

        if (myConvIds.Count == 0)
            return Array.Empty<SysMessageSearchHitDto>();

        var lower = q.ToLowerInvariant();
        var messages = await _db.ChatMessages.AsNoTracking()
            .Where(m => m.TenantId == tenantId && !m.IsDeleted && m.RecalledAt == null
                        && myConvIds.Contains(m.ConversationId)
                        && m.Body.ToLower().Contains(lower))
            .OrderByDescending(m => m.SentAt)
            .Take(take)
            .ToListAsync(ct);

        if (messages.Count == 0)
            return Array.Empty<SysMessageSearchHitDto>();

        var convIds = messages.Select(m => m.ConversationId).Distinct().ToList();
        var convs = await _db.Conversations.AsNoTracking()
            .Where(c => convIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, ct);
        var senderIds = messages.Select(m => m.SenderUserId).Distinct().ToList();
        var senders = await _db.Users.AsNoTracking()
            .Where(u => senderIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.Username, ct);

        return messages.Select(m =>
        {
            convs.TryGetValue(m.ConversationId, out var conv);
            senders.TryGetValue(m.SenderUserId, out var name);
            var preview = m.Body.Length > 160 ? m.Body[..160] + "…" : m.Body;
            return new SysMessageSearchHitDto(
                m.Id, m.ConversationId, conv?.Title,
                m.SenderUserId, name ?? "?", preview, m.SentAt);
        }).ToList();
    }

    // ── 104 ─────────────────────────────────────────────────────────────────

    public async Task<SysConversationMuteDto> SetConversationMuteAsync(
        Guid tenantId, Guid userId, Guid conversationId, SysConversationMuteRequest req, CancellationToken ct = default)
    {
        var me = await _db.ConversationMembers.FirstOrDefaultAsync(
                     m => m.TenantId == tenantId && m.UserId == userId &&
                          m.ConversationId == conversationId && !m.IsDeleted, ct)
                 ?? throw new AppException("Bạn không thuộc hội thoại này.", 404);

        if (req.Muted && req.MuteUntil is { } until && until <= DateTimeOffset.UtcNow)
            throw new AppException("MuteUntil phải ở tương lai khi bật mute có hạn.");

        me.Muted = req.Muted;
        me.MuteUntil = req.Muted ? req.MuteUntil : null;
        me.UpdatedAt = DateTimeOffset.UtcNow;
        me.UpdatedBy = userId;
        await _db.SaveChangesAsync(ct);

        var effective = IsEffectivelyMuted(me.Muted, me.MuteUntil, DateTimeOffset.UtcNow);
        return new SysConversationMuteDto(conversationId, me.Muted, me.MuteUntil, effective);
    }

    public async Task<SysConversationMuteDto> GetConversationMuteAsync(
        Guid tenantId, Guid userId, Guid conversationId, CancellationToken ct = default)
    {
        var me = await _db.ConversationMembers.AsNoTracking().FirstOrDefaultAsync(
                     m => m.TenantId == tenantId && m.UserId == userId &&
                          m.ConversationId == conversationId && !m.IsDeleted, ct)
                 ?? throw new AppException("Bạn không thuộc hội thoại này.", 404);

        // Auto-expire soft: nếu hết hạn thì coi như unmuted khi đọc
        var now = DateTimeOffset.UtcNow;
        var effective = IsEffectivelyMuted(me.Muted, me.MuteUntil, now);
        return new SysConversationMuteDto(conversationId, me.Muted, me.MuteUntil, effective);
    }

    public bool IsEffectivelyMuted(bool muted, DateTimeOffset? muteUntil, DateTimeOffset utcNow)
    {
        if (!muted) return false;
        if (muteUntil is { } until && until <= utcNow) return false;
        return true;
    }

    // ── helpers ─────────────────────────────────────────────────────────────

    private static SysThemeDto MapTheme(Tenant t) => new(
        t.Id, t.Name, t.LogoUrl, t.PrimaryColor, t.AccentColor, t.FaviconUrl);

    private static string? NormalizeColor(string? color, bool allowNull)
    {
        if (string.IsNullOrWhiteSpace(color))
            return allowNull ? null : throw new AppException("Màu không được để trống.");
        var c = color.Trim();
        if (!HexColor.IsMatch(c))
            throw new AppException("Màu phải dạng #RGB hoặc #RRGGBB.");
        return c.ToUpperInvariant();
    }
}
