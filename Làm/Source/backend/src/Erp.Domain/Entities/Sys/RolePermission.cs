using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class RolePermission : TenantEntity
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
}
