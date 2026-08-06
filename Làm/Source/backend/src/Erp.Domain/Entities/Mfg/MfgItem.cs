using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>TP / BTP / NVL (UC_MFG_001–002).</summary>
public class MfgItem : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    /// <summary>FG · SFG · RM</summary>
    public string ItemType { get; set; } = "FG";
    public string Unit { get; set; } = "CAI";
    /// <summary>Giá chuẩn NVL/TP dùng tập hợp giá thành (UC_MFG_027).</summary>
    public decimal StandardCost { get; set; }
    /// <summary>Active · Inactive</summary>
    public string Status { get; set; } = "Active";
    public string? Note { get; set; }
}
