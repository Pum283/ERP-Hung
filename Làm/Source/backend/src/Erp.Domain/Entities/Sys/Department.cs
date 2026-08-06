using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

/// <summary>Phòng ban — trục data scope Department.</summary>
public class Department : TenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public Guid OrgUnitId { get; set; }
    public Guid? ManagerUserId { get; set; }
    public string Path { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
