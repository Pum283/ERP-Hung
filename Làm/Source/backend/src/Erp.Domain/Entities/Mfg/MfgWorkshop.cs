using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Xưởng / dây chuyền (UC_MFG_003).</summary>
public class MfgWorkshop : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    /// <summary>Workshop · Line</summary>
    public string WorkshopType { get; set; } = "Workshop";
    public string Status { get; set; } = "Active";
    public string? Note { get; set; }
}
