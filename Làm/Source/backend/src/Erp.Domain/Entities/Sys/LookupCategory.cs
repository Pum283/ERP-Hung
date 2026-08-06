using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class LookupCategory : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
