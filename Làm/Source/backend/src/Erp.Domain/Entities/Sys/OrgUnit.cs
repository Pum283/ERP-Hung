using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

/// <summary>Cây công ty / chi nhánh (không thay Department).</summary>
public class OrgUnit : TenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public string UnitType { get; set; } = "Branch";
    public string Path { get; set; } = string.Empty;
    public Guid? ManagerUserId { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
