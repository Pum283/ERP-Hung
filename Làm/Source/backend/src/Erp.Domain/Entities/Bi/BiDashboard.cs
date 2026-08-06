using Erp.Domain.Base;

namespace Erp.Domain.Entities.Bi;

/// <summary>Dashboard lãnh đạo / theo module (UC_BI_006–007).</summary>
public class BiDashboard : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    /// <summary>Executive | Module</summary>
    public string DashboardType { get; set; } = "Executive";
    public string? ModuleCode { get; set; }
    public string Status { get; set; } = "Active";
    public string? Note { get; set; }
    public int SortOrder { get; set; }
}
