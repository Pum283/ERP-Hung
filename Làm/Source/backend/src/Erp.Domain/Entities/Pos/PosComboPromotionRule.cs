using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Khuyến mại theo gói combo sản phẩm (UC_POS_023).</summary>
public class PosComboPromotionRule : TenantEntity
{
    public string ComboCode { get; set; } = "";
    public string ComboName { get; set; } = "";
    public string ProductIdsJson { get; set; } = "[]";
    public decimal FixedComboPriceVnd { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}
