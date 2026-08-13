using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

/// <summary>UC_SYS_094 — Trang chủ / landing theo vai trò.</summary>
public class SysRoleHomeConfig : TenantEntity
{
    public Guid RoleId { get; set; }
    /// <summary>Đường dẫn FE, vd. /app/hrm hoặc /app/sys/users</summary>
    public string LandingPath { get; set; } = "/app";
    /// <summary>Ưu tiên khi user có nhiều role — số nhỏ hơn = ưu tiên cao hơn.</summary>
    public int Priority { get; set; } = 100;
    public bool IsActive { get; set; } = true;
    public string? Note { get; set; }
}
