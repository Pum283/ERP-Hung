using Erp.Domain.Base;
using Erp.Domain.Enums.Sys;

namespace Erp.Domain.Entities.Sys;

/// <summary>Cấp bậc — mang default_scope_type.</summary>
public class JobLevel : TenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int LevelOrder { get; set; }
    public ScopeType DefaultScopeType { get; set; } = ScopeType.Own;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
