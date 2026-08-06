using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

/// <summary>Hội thoại 1-1 hoặc nhóm (SYS-13).</summary>
public class Conversation : TenantEntity
{
    /// <summary>Direct | Group</summary>
    public string Kind { get; set; } = "Direct";
    public string? Title { get; set; }
    /// <summary>Với Direct: khóa chuẩn "{minUserId}:{maxUserId}" để tránh trùng.</summary>
    public string? DirectKey { get; set; }
}
