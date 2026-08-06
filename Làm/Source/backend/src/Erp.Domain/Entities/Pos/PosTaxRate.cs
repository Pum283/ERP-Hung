using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Thuế GTGT (UC_POS_019).</summary>
public class PosTaxRate : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal RatePct { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}
