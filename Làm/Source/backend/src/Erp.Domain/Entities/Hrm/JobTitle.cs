using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

public class JobTitle : TenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? DefaultJobLevelId { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
