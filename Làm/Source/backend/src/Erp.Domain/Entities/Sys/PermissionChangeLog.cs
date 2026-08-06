using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class PermissionChangeLog : TenantEntity
{
    public Guid? ActorUserId { get; set; }
    public string ChangeType { get; set; } = "";
    public Guid? RoleId { get; set; }
    public Guid? TargetUserId { get; set; }
    public string DetailJson { get; set; } = "{}";
}
