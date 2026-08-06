using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Dòng định mức NVL/BTP (UC_MFG_008).</summary>
public class MfgBomLine : TenantEntity
{
    public Guid BomId { get; set; }
    public Guid ComponentItemId { get; set; }
    public decimal Qty { get; set; }
    public string Unit { get; set; } = "CAI";
    /// <summary>Cấp trong BOM (1 = trực tiếp).</summary>
    public int Level { get; set; } = 1;
    public string? Note { get; set; }
}
