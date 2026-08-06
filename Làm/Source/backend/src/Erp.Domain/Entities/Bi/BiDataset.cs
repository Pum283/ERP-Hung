using Erp.Domain.Base;

namespace Erp.Domain.Entities.Bi;

/// <summary>Catalog dataset theo module (UC_BI_001–002).</summary>
public class BiDataset : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string ModuleCode { get; set; } = "";
    public string? Description { get; set; }
    /// <summary>Ready · Refreshing · Error · Stale</summary>
    public string Status { get; set; } = "Ready";
    public DateTimeOffset? LastRefreshedAt { get; set; }
    public string? LastRefreshNote { get; set; }
    public int RowCountEstimate { get; set; }
}
