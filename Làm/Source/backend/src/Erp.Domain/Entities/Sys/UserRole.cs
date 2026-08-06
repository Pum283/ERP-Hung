using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class UserRole : TenantEntity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? ValidFrom { get; set; }
    public DateTimeOffset? ValidTo { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public Guid? AssignedBy { get; set; }
}
