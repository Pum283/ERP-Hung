using Erp.Domain.Base;
using Erp.Domain.Enums.Sys;

namespace Erp.Domain.Entities.Sys;

public class AppUser : TenantEntity
{
    public string Username { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? PasswordHash { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public bool MustChangePassword { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public int FailedLoginCount { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
    public Guid? PrimaryOrgUnitId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? JobLevelId { get; set; }
    public Guid? ManagerUserId { get; set; }
    public Guid? EmployeeId { get; set; }
    public bool TotpEnabled { get; set; }
    public string? TotpSecret { get; set; }
    public string PreferredLocale { get; set; } = "vi";
}
