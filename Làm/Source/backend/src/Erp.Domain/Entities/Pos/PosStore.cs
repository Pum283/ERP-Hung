using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Điểm bán POS (UC_POS_001).</summary>
public class PosStore : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Address { get; set; }
    /// <summary>Active · Inactive</summary>
    public string Status { get; set; } = "Active";
}
