using Erp.Application.DTOs.Sys;

namespace Erp.Application.Interfaces.Services.Sys;

public interface ISysStep155Service
{
    // 093 theme
    Task<SysThemeDto> GetThemeAsync(Guid tenantId, CancellationToken ct = default);
    Task<SysThemeDto> UpsertThemeAsync(Guid tenantId, Guid userId, SysThemeUpsertRequest req, CancellationToken ct = default);

    // 094 role home
    Task<IReadOnlyList<SysRoleHomeDto>> ListRoleHomesAsync(Guid tenantId, CancellationToken ct = default);
    Task<SysRoleHomeDto> UpsertRoleHomeAsync(Guid tenantId, Guid userId, SysRoleHomeUpsertRequest req, CancellationToken ct = default);
    Task DeleteRoleHomeAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<SysMyHomeDto> ResolveMyHomeAsync(Guid tenantId, Guid userId, CancellationToken ct = default);

    // 103 search
    Task<IReadOnlyList<SysMessageSearchHitDto>> SearchMessagesAsync(
        Guid tenantId, Guid userId, string query, Guid? conversationId, int take, CancellationToken ct = default);

    // 104 mute
    Task<SysConversationMuteDto> SetConversationMuteAsync(
        Guid tenantId, Guid userId, Guid conversationId, SysConversationMuteRequest req, CancellationToken ct = default);
    Task<SysConversationMuteDto> GetConversationMuteAsync(
        Guid tenantId, Guid userId, Guid conversationId, CancellationToken ct = default);
    bool IsEffectivelyMuted(bool muted, DateTimeOffset? muteUntil, DateTimeOffset utcNow);
}
