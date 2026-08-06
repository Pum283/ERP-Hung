using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class FileFolder : TenantEntity
{
    public string Name { get; set; } = "";
    public Guid? ParentId { get; set; }
}
