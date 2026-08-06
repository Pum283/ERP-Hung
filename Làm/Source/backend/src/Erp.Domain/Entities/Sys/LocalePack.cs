using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class LocalePack : TenantEntity
{
    public string Code { get; set; } = "vi";
    public string Name { get; set; } = "Tiếng Việt";
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}
