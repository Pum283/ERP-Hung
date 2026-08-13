using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Thuộc tính & Modifier sản phẩm POS (UC_POS_011, UC_POS_013).</summary>
public class PosProductAttributeModifier : TenantEntity
{
    public Guid ProductId { get; set; }
    public string AttributeName { get; set; } = "";
    public string OptionValue { get; set; } = "";
    public decimal ExtraPriceVnd { get; set; }
    public string ImageUrl { get; set; } = "";
    public int DisplayOrder { get; set; } = 1;
    public bool IsDefault { get; set; } = false;
}
