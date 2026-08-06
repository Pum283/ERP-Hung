using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Quầy / máy POS (UC_POS_002).</summary>
public class PosTerminal : TenantEntity
{
    public Guid StoreId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    /// <summary>Active · Inactive</summary>
    public string Status { get; set; } = "Active";
}
