using Erp.Domain.Base;

namespace Erp.Domain.Entities.Prt;

/// <summary>Tài khoản portal KH (UC_PRT_001–003).</summary>
public class PrtAccount : TenantEntity
{
    public string Code { get; set; } = "";
    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";
    /// <summary>Stub hash — không dùng production</summary>
    public string PasswordHash { get; set; } = "";
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    /// <summary>Pending · Active · Locked</summary>
    public string Status { get; set; } = "Pending";
    public DateTimeOffset? LastLoginAt { get; set; }
    public string? ResetTokenStub { get; set; }
    public DateTimeOffset? ResetTokenExpiresAt { get; set; }
}
