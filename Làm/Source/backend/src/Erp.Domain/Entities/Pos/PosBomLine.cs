using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>BOM / định mức NVL cho SP (UC_POS_012).</summary>
public class PosBomLine : TenantEntity
{
    public Guid ProductId { get; set; }
    public string MaterialCode { get; set; } = "";
    public string MaterialName { get; set; } = "";
    public decimal Qty { get; set; }
    public string Unit { get; set; } = "cai";
}
