using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class LoginAudit : TenantEntity
{
    public Guid? UserId { get; set; }
    public string Username { get; set; } = "";
    public bool Success { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? FailureReason { get; set; }
    public DateTimeOffset AttemptedAt { get; set; } = DateTimeOffset.UtcNow;
}
