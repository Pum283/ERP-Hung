using Erp.Application.Common.Exceptions;
using Erp.Application.DTOs.Sys;
using Erp.Application.Interfaces.Realtime;
using Erp.Application.Interfaces.Services.Sys;
using Erp.Domain.Entities.Sys;
using Erp.Domain.Enums.Sys;
using Erp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Implementations.Services.Sys;

public sealed class MsgService : IMsgService
{
    private readonly AppDbContext _db;
    private readonly IMsgRealtimeNotifier _rt;

    public MsgService(AppDbContext db, IMsgRealtimeNotifier rt)
    {
        _db = db;
        _rt = rt;
    }

    public async Task<IReadOnlyList<ConversationDto>> ListConversationsAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var myConvIds = await _db.ConversationMembers.AsNoTracking()
            .Where(m => m.TenantId == tenantId && m.UserId == userId && !m.IsDeleted)
            .Select(m => m.ConversationId)
            .ToListAsync(ct);

        var convs = await _db.Conversations.AsNoTracking()
            .Where(c => c.TenantId == tenantId && myConvIds.Contains(c.Id) && !c.IsDeleted)
            .ToListAsync(ct);

        var members = await _db.ConversationMembers.AsNoTracking()
            .Where(m => m.TenantId == tenantId && myConvIds.Contains(m.ConversationId) && !m.IsDeleted)
            .ToListAsync(ct);

        var userIds = members.Select(m => m.UserId).Distinct().ToList();
        var users = await _db.Users.AsNoTracking()
            .Where(u => u.TenantId == tenantId && userIds.Contains(u.Id) && !u.IsDeleted)
            .ToDictionaryAsync(u => u.Id, ct);

        var lastAtByConv = await _db.ChatMessages.AsNoTracking()
            .Where(m => m.TenantId == tenantId && myConvIds.Contains(m.ConversationId) && !m.IsDeleted)
            .GroupBy(m => m.ConversationId)
            .Select(g => new { ConversationId = g.Key, SentAt = g.Max(x => x.SentAt) })
            .ToListAsync(ct);
        var lastByConv = new Dictionary<Guid, ChatMessage>();
        foreach (var la in lastAtByConv)
        {
            var msg = await _db.ChatMessages.AsNoTracking()
                .Where(m => m.TenantId == tenantId && m.ConversationId == la.ConversationId && m.SentAt == la.SentAt && !m.IsDeleted)
                .OrderByDescending(m => m.CreatedAt)
                .FirstAsync(ct);
            lastByConv[la.ConversationId] = msg;
        }

        var result = new List<ConversationDto>();
        foreach (var c in convs.OrderByDescending(x => lastByConv.TryGetValue(x.Id, out var lm) ? lm.SentAt : x.CreatedAt))
        {
            var mems = members.Where(m => m.ConversationId == c.Id).ToList();
            var me = mems.First(m => m.UserId == userId);
            var peer = mems.FirstOrDefault(m => m.UserId != userId);
            Guid? peerId = c.Kind == "Direct" ? peer?.UserId : null;
            string? peerName = peerId is Guid pid && users.TryGetValue(pid, out var pu)
                ? (pu.DisplayName ?? pu.Username)
                : c.Title;

            lastByConv.TryGetValue(c.Id, out var last);
            var unread = me.Muted ? 0 : await CountUnreadAsync(tenantId, c.Id, userId, me.LastReadAt, ct);
            var preview = last is null ? null
                : last.RecalledAt is not null ? "(đã thu hồi)"
                : (last.Body.Length > 80 ? last.Body[..80] + "…" : last.Body);

            result.Add(new ConversationDto(
                c.Id, c.Kind, c.Title, peerName, peerId, preview, last?.SentAt, unread, me.Muted));
        }

        return result;
    }

    public async Task<ConversationDto> CreateConversationAsync(Guid tenantId, Guid userId, CreateConversationRequest req, CancellationToken ct = default)
    {
        if (req.PeerUserId is Guid peerId)
        {
            if (peerId == userId)
                throw new AppException("Không thể tạo hội thoại với chính mình.");

            var peer = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == peerId && u.TenantId == tenantId && !u.IsDeleted, ct)
                ?? throw new AppException("Không tìm thấy người dùng.", 404);

            var key = DirectKey(userId, peerId);
            var existing = await _db.Conversations
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.DirectKey == key && !c.IsDeleted, ct);
            if (existing is not null)
                return (await ListConversationsAsync(tenantId, userId, ct)).First(x => x.Id == existing.Id);

            var conv = new Conversation
            {
                TenantId = tenantId,
                Kind = "Direct",
                DirectKey = key,
                CreatedBy = userId
            };
            _db.Conversations.Add(conv);
            _db.ConversationMembers.AddRange(
                new ConversationMember { TenantId = tenantId, ConversationId = conv.Id, UserId = userId, CreatedBy = userId },
                new ConversationMember { TenantId = tenantId, ConversationId = conv.Id, UserId = peerId, CreatedBy = userId });
            await _db.SaveChangesAsync(ct);

            await _rt.ConversationUpdatedAsync(new[] { userId, peerId }, new { conversationId = conv.Id, reason = "created" }, ct);
            return new ConversationDto(conv.Id, "Direct", null, peer.DisplayName ?? peer.Username, peerId, null, null, 0, false);
        }

        var memberIds = (req.MemberIds ?? Array.Empty<Guid>()).Where(x => x != userId).Distinct().ToList();
        if (memberIds.Count < 1)
            throw new AppException("Hội thoại nhóm cần ít nhất 1 thành viên khác.");
        if (string.IsNullOrWhiteSpace(req.Title))
            throw new AppException("Tên nhóm bắt buộc.");

        var valid = await _db.Users.AsNoTracking()
            .Where(u => u.TenantId == tenantId && memberIds.Contains(u.Id) && !u.IsDeleted)
            .Select(u => u.Id)
            .ToListAsync(ct);
        if (valid.Count != memberIds.Count)
            throw new AppException("Có thành viên không hợp lệ.");

        var group = new Conversation
        {
            TenantId = tenantId,
            Kind = "Group",
            Title = req.Title.Trim(),
            CreatedBy = userId
        };
        _db.Conversations.Add(group);
        var all = valid.Append(userId).Distinct().ToList();
        foreach (var uid in all)
            _db.ConversationMembers.Add(new ConversationMember
            {
                TenantId = tenantId, ConversationId = group.Id, UserId = uid, CreatedBy = userId
            });
        await _db.SaveChangesAsync(ct);
        await _rt.ConversationUpdatedAsync(all, new { conversationId = group.Id, reason = "created" }, ct);
        return new ConversationDto(group.Id, "Group", group.Title, group.Title, null, null, null, 0, false);
    }

    public async Task<IReadOnlyList<ChatMessageDto>> ListMessagesAsync(
        Guid tenantId, Guid userId, Guid conversationId, DateTimeOffset? before, int take, CancellationToken ct = default)
    {
        await EnsureMemberAsync(tenantId, userId, conversationId, ct);
        take = Math.Clamp(take <= 0 ? 50 : take, 1, 100);

        var q = _db.ChatMessages.AsNoTracking()
            .Where(m => m.TenantId == tenantId && m.ConversationId == conversationId && !m.IsDeleted);
        if (before is DateTimeOffset b)
            q = q.Where(m => m.SentAt < b);

        var rows = await q.OrderByDescending(m => m.SentAt).Take(take).ToListAsync(ct);
        rows.Reverse();
        return await MapMessagesAsync(rows, ct);
    }

    public async Task<ChatMessageDto> SendMessageAsync(
        Guid tenantId, Guid userId, Guid conversationId, SendMessageRequest req, CancellationToken ct = default)
    {
        await EnsureMemberAsync(tenantId, userId, conversationId, ct);
        var body = (req.Body ?? "").Trim();
        if (body.Length == 0 && req.AttachmentFileId is null && string.IsNullOrWhiteSpace(req.AttachmentStorageKey))
            throw new AppException("Nội dung tin nhắn trống.");
        if (body.Length > 4000)
            throw new AppException("Tin nhắn tối đa 4000 ký tự.");

        if (req.ParentMessageId is Guid parentId)
        {
            var parentOk = await _db.ChatMessages.AnyAsync(
                m => m.Id == parentId && m.ConversationId == conversationId && m.TenantId == tenantId && !m.IsDeleted, ct);
            if (!parentOk) throw new AppException("Tin nhắn trả lời không tồn tại.", 404);
        }

        var msg = new ChatMessage
        {
            TenantId = tenantId,
            ConversationId = conversationId,
            SenderUserId = userId,
            Body = body,
            AttachmentFileId = req.AttachmentFileId,
            AttachmentStorageKey = string.IsNullOrWhiteSpace(req.AttachmentStorageKey) ? null : req.AttachmentStorageKey.Trim(),
            ParentMessageId = req.ParentMessageId,
            SentAt = DateTimeOffset.UtcNow,
            CreatedBy = userId
        };
        _db.ChatMessages.Add(msg);

        var me = await _db.ConversationMembers
            .FirstAsync(m => m.TenantId == tenantId && m.ConversationId == conversationId && m.UserId == userId && !m.IsDeleted, ct);
        me.LastReadAt = msg.SentAt;

        await _db.SaveChangesAsync(ct);

        var dto = (await MapMessagesAsync(new[] { msg }, ct))[0];
        var memberIds = await MemberIdsAsync(tenantId, conversationId, ct);
        await _rt.MessageReceivedAsync(memberIds, dto, ct);
        await _rt.ConversationUpdatedAsync(memberIds, new { conversationId, reason = "message" }, ct);
        return dto;
    }

    public async Task<ChatMessageDto> EditMessageAsync(
        Guid tenantId, Guid userId, Guid conversationId, Guid messageId, EditMessageRequest req, CancellationToken ct = default)
    {
        await EnsureMemberAsync(tenantId, userId, conversationId, ct);
        var msg = await _db.ChatMessages.FirstOrDefaultAsync(
            m => m.Id == messageId && m.ConversationId == conversationId && m.TenantId == tenantId && !m.IsDeleted, ct)
            ?? throw new AppException("Tin nhắn không tồn tại.", 404);
        if (msg.SenderUserId != userId) throw new ForbiddenException("Chỉ người gửi mới sửa được.");
        if (msg.RecalledAt is not null) throw new AppException("Tin đã thu hồi không sửa được.");

        var body = (req.Body ?? "").Trim();
        if (body.Length == 0) throw new AppException("Nội dung trống.");
        if (body.Length > 4000) throw new AppException("Tin nhắn tối đa 4000 ký tự.");

        msg.Body = body;
        msg.IsEdited = true;
        msg.EditedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        var dto = (await MapMessagesAsync(new[] { msg }, ct))[0];
        await _rt.MessageEditedAsync(await MemberIdsAsync(tenantId, conversationId, ct), dto, ct);
        return dto;
    }

    public async Task MarkReadAsync(Guid tenantId, Guid userId, Guid conversationId, CancellationToken ct = default)
    {
        var me = await EnsureMemberAsync(tenantId, userId, conversationId, ct);
        me.LastReadAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        await _rt.ConversationUpdatedAsync(new[] { userId }, new { conversationId, reason = "read" }, ct);
    }

    public async Task SetMutedAsync(Guid tenantId, Guid userId, Guid conversationId, bool muted, CancellationToken ct = default)
    {
        var me = await EnsureMemberAsync(tenantId, userId, conversationId, ct);
        me.Muted = muted;
        await _db.SaveChangesAsync(ct);
        await _rt.ConversationUpdatedAsync(new[] { userId }, new { conversationId, reason = muted ? "muted" : "unmuted" }, ct);
    }

    public async Task<UnreadCountDto> GetUnreadCountAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var memberships = await _db.ConversationMembers.AsNoTracking()
            .Where(m => m.TenantId == tenantId && m.UserId == userId && !m.IsDeleted && !m.Muted)
            .Select(m => new { m.ConversationId, m.LastReadAt })
            .ToListAsync(ct);

        var total = 0;
        foreach (var m in memberships)
            total += await CountUnreadAsync(tenantId, m.ConversationId, userId, m.LastReadAt, ct);
        return new UnreadCountDto(total);
    }

    public async Task<IReadOnlyList<MsgDirectoryUserDto>> ListDirectoryAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        return await _db.Users.AsNoTracking()
            .Where(u => u.TenantId == tenantId && u.Id != userId && !u.IsDeleted && u.Status == UserStatus.Active)
            .OrderBy(u => u.DisplayName ?? u.Username)
            .Select(u => new MsgDirectoryUserDto(u.Id, u.Username, u.DisplayName ?? u.Username))
            .ToListAsync(ct);
    }

    public async Task<ChatMessageDto> RecallMessageAsync(Guid tenantId, Guid userId, Guid conversationId, Guid messageId, CancellationToken ct = default)
    {
        await EnsureMemberAsync(tenantId, userId, conversationId, ct);
        var msg = await _db.ChatMessages.FirstOrDefaultAsync(
            m => m.Id == messageId && m.ConversationId == conversationId && m.TenantId == tenantId && !m.IsDeleted, ct)
            ?? throw new AppException("Tin nhắn không tồn tại.", 404);
        if (msg.SenderUserId != userId)
            throw new ForbiddenException("Chỉ người gửi mới thu hồi được.");
        msg.RecalledAt = DateTimeOffset.UtcNow;
        msg.Body = "";
        await _db.SaveChangesAsync(ct);
        var dto = (await MapMessagesAsync(new[] { msg }, ct))[0];
        await _rt.MessageReceivedAsync(await MemberIdsAsync(tenantId, conversationId, ct), dto, ct);
        return dto;
    }

    public async Task<ReactionToggledDto> ToggleReactionAsync(
        Guid tenantId, Guid userId, Guid conversationId, Guid messageId, ToggleReactionRequest req, CancellationToken ct = default)
    {
        await EnsureMemberAsync(tenantId, userId, conversationId, ct);
        var msg = await _db.ChatMessages.AsNoTracking().FirstOrDefaultAsync(
            m => m.Id == messageId && m.ConversationId == conversationId && m.TenantId == tenantId && !m.IsDeleted, ct)
            ?? throw new AppException("Tin nhắn không tồn tại.", 404);
        if (msg.RecalledAt is not null) throw new AppException("Không react tin đã thu hồi.");

        var type = (req.ReactionType ?? "").Trim();
        if (type.Length == 0 || type.Length > 32) throw new AppException("Reaction không hợp lệ.");

        var existing = await _db.ChatMessageReactions.FirstOrDefaultAsync(
            r => r.TenantId == tenantId && r.MessageId == messageId && r.UserId == userId
                 && r.ReactionType == type && !r.IsDeleted, ct);

        var displayName = await _db.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.DisplayName ?? u.Username)
            .FirstAsync(ct);

        bool removed;
        if (existing is not null)
        {
            _db.ChatMessageReactions.Remove(existing);
            removed = true;
        }
        else
        {
            _db.ChatMessageReactions.Add(new ChatMessageReaction
            {
                TenantId = tenantId,
                MessageId = messageId,
                UserId = userId,
                ReactionType = type,
                CreatedBy = userId
            });
            removed = false;
        }

        await _db.SaveChangesAsync(ct);
        var payload = new ReactionToggledDto(messageId, conversationId, userId, displayName, type, removed);
        await _rt.ReactionToggledAsync(await MemberIdsAsync(tenantId, conversationId, ct), payload, ct);
        return payload;
    }

    public async Task<IReadOnlyList<ConversationMemberDto>> ListMembersAsync(Guid tenantId, Guid userId, Guid conversationId, CancellationToken ct = default)
    {
        await EnsureMemberAsync(tenantId, userId, conversationId, ct);
        var mems = await _db.ConversationMembers.AsNoTracking()
            .Where(m => m.TenantId == tenantId && m.ConversationId == conversationId && !m.IsDeleted)
            .Select(m => m.UserId)
            .ToListAsync(ct);
        var users = await _db.Users.AsNoTracking()
            .Where(u => mems.Contains(u.Id) && !u.IsDeleted)
            .ToListAsync(ct);
        return users
            .OrderBy(u => u.DisplayName ?? u.Username)
            .Select(u => new ConversationMemberDto(u.Id, u.DisplayName ?? u.Username, u.Username, u.Id == userId))
            .ToList();
    }

    public async Task AddMembersAsync(Guid tenantId, Guid userId, Guid conversationId, IReadOnlyList<Guid> memberIds, CancellationToken ct = default)
    {
        var conv = await _db.Conversations.FirstOrDefaultAsync(c => c.Id == conversationId && c.TenantId == tenantId && !c.IsDeleted, ct)
                   ?? throw new AppException("Hội thoại không tồn tại.", 404);
        if (conv.Kind != "Group") throw new AppException("Chỉ nhóm mới thêm thành viên.");
        await EnsureMemberAsync(tenantId, userId, conversationId, ct);

        var existing = await _db.ConversationMembers
            .Where(m => m.TenantId == tenantId && m.ConversationId == conversationId && !m.IsDeleted)
            .Select(m => m.UserId).ToListAsync(ct);
        var toAdd = memberIds.Where(id => id != userId && !existing.Contains(id)).Distinct().ToList();
        var valid = await _db.Users.AsNoTracking()
            .Where(u => u.TenantId == tenantId && toAdd.Contains(u.Id) && !u.IsDeleted)
            .Select(u => u.Id).ToListAsync(ct);
        foreach (var uid in valid)
            _db.ConversationMembers.Add(new ConversationMember
            {
                TenantId = tenantId, ConversationId = conversationId, UserId = uid, CreatedBy = userId
            });
        await _db.SaveChangesAsync(ct);
        var all = existing.Concat(valid).Append(userId).Distinct();
        await _rt.ConversationUpdatedAsync(all, new { conversationId, reason = "members" }, ct);
    }

    public async Task RemoveMemberAsync(Guid tenantId, Guid actorId, Guid conversationId, Guid memberUserId, CancellationToken ct = default)
    {
        var conv = await _db.Conversations.FirstOrDefaultAsync(c => c.Id == conversationId && c.TenantId == tenantId && !c.IsDeleted, ct)
                   ?? throw new AppException("Hội thoại không tồn tại.", 404);
        if (conv.Kind != "Group") throw new AppException("Chỉ nhóm mới rời/xóa thành viên.");
        await EnsureMemberAsync(tenantId, actorId, conversationId, ct);

        // Digi: tự rời hoặc creator kick
        if (memberUserId != actorId && conv.CreatedBy != actorId)
            throw new ForbiddenException("Chỉ người tạo nhóm mới xóa thành viên khác.");

        var mem = await _db.ConversationMembers.FirstOrDefaultAsync(
            m => m.TenantId == tenantId && m.ConversationId == conversationId && m.UserId == memberUserId && !m.IsDeleted, ct)
            ?? throw new AppException("Thành viên không tồn tại.", 404);
        mem.IsDeleted = true;
        mem.DeletedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        var rest = await MemberIdsAsync(tenantId, conversationId, ct);
        await _rt.ConversationUpdatedAsync(rest.Append(memberUserId), new { conversationId, reason = "members" }, ct);
    }

    private async Task<IReadOnlyList<ChatMessageDto>> MapMessagesAsync(IReadOnlyList<ChatMessage> rows, CancellationToken ct)
    {
        if (rows.Count == 0) return Array.Empty<ChatMessageDto>();
        var senderIds = rows.Select(r => r.SenderUserId).Distinct().ToList();
        var parentIds = rows.Where(r => r.ParentMessageId is not null).Select(r => r.ParentMessageId!.Value).Distinct().ToList();
        var msgIds = rows.Select(r => r.Id).ToList();
        var names = await _db.Users.AsNoTracking()
            .Where(u => senderIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.Username, ct);
        var parents = parentIds.Count == 0
            ? new Dictionary<Guid, ChatMessage>()
            : await _db.ChatMessages.AsNoTracking()
                .Where(m => parentIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, ct);

        var reactionRows = await _db.ChatMessageReactions.AsNoTracking()
            .Where(r => r.TenantId == rows[0].TenantId && msgIds.Contains(r.MessageId) && !r.IsDeleted)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(ct);
        var reactorIds = reactionRows.Select(r => r.UserId).Distinct().ToList();
        var reactorNames = reactorIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.Users.AsNoTracking()
                .Where(u => reactorIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.Username, ct);
        var reactionsByMsg = reactionRows
            .GroupBy(r => r.MessageId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<MessageReactionDto>)g
                    .Select(r => new MessageReactionDto(
                        r.Id, r.MessageId, r.UserId,
                        reactorNames.GetValueOrDefault(r.UserId, "?"),
                        r.ReactionType))
                    .ToList());

        return rows.Select(m =>
        {
            string? parentPreview = null;
            if (m.ParentMessageId is Guid pid && parents.TryGetValue(pid, out var p))
                parentPreview = p.RecalledAt is not null ? "(đã thu hồi)"
                    : (p.Body.Length > 60 ? p.Body[..60] + "…" : p.Body);
            return new ChatMessageDto(
                m.Id, m.ConversationId, m.SenderUserId,
                names.GetValueOrDefault(m.SenderUserId, "?"),
                m.RecalledAt is null ? m.Body : "",
                m.AttachmentFileId, m.AttachmentStorageKey, m.SentAt, m.RecalledAt is not null,
                m.ParentMessageId, parentPreview, m.IsEdited,
                reactionsByMsg.GetValueOrDefault(m.Id) ?? Array.Empty<MessageReactionDto>());
        }).ToList();
    }

    private async Task<List<Guid>> MemberIdsAsync(Guid tenantId, Guid conversationId, CancellationToken ct)
        => await _db.ConversationMembers.AsNoTracking()
            .Where(m => m.TenantId == tenantId && m.ConversationId == conversationId && !m.IsDeleted)
            .Select(m => m.UserId).ToListAsync(ct);

    private async Task<ConversationMember> EnsureMemberAsync(Guid tenantId, Guid userId, Guid conversationId, CancellationToken ct)
    {
        var mem = await _db.ConversationMembers
            .FirstOrDefaultAsync(m => m.TenantId == tenantId && m.ConversationId == conversationId && m.UserId == userId && !m.IsDeleted, ct);
        if (mem is null)
            throw new ForbiddenException("Bạn không thuộc hội thoại này.");
        var exists = await _db.Conversations.AnyAsync(c => c.Id == conversationId && c.TenantId == tenantId && !c.IsDeleted, ct);
        if (!exists)
            throw new AppException("Hội thoại không tồn tại.", 404);
        return mem;
    }

    private async Task<int> CountUnreadAsync(Guid tenantId, Guid conversationId, Guid userId, DateTimeOffset? lastReadAt, CancellationToken ct)
    {
        var q = _db.ChatMessages.AsNoTracking()
            .Where(m => m.TenantId == tenantId && m.ConversationId == conversationId && !m.IsDeleted
                        && m.SenderUserId != userId && m.RecalledAt == null);
        if (lastReadAt is DateTimeOffset lr)
            q = q.Where(m => m.SentAt > lr);
        return await q.CountAsync(ct);
    }

    private static string DirectKey(Guid a, Guid b)
    {
        var x = a.CompareTo(b) < 0 ? a : b;
        var y = a.CompareTo(b) < 0 ? b : a;
        return $"{x:D}:{y:D}";
    }
}
