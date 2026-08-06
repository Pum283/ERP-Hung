using Erp.Domain.Base;

namespace Erp.Domain.Entities.Sys;

/// <summary>Catalog quyền chức năng — global (không tenant).</summary>
public class Permission : BaseEntity
{
    public string ModuleCode { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
