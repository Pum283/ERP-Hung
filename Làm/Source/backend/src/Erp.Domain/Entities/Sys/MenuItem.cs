using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

public class MenuItem : TenantEntity
{
    public string Code { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public string ModuleCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? RoutePath { get; set; }
    public string? PermissionCode { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
