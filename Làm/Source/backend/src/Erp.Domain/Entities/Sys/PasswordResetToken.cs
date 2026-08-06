using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class PasswordResetToken : TenantEntity
{
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = "";
    public string OtpCode { get; set; } = "";
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
}
