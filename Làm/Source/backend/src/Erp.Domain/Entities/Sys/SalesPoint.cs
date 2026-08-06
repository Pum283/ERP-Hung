using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class SalesPoint : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public Guid? OrgUnitId { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
}
