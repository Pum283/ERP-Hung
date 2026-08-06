using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class UserSession : TenantEntity
{
    public Guid UserId { get; set; }
    public string SessionKey { get; set; } = "";
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTimeOffset LastSeenAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
}
