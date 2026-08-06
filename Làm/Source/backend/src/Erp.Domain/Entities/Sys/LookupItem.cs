using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class LookupItem : TenantEntity
{
    public Guid CategoryId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
