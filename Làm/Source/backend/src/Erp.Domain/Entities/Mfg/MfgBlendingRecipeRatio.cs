using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Định mức phối trộn tỷ lệ công thức hóa chất / nguyên liệu (UC_MFG_040).</summary>
public class MfgBlendingRecipeRatio : TenantEntity
{
    public string RecipeCode { get; set; } = "";
    public string RecipeName { get; set; } = "";
    public string IngredientProductCode { get; set; } = "";
    public string IngredientProductName { get; set; } = "";
    public decimal MixingRatioPercentage { get; set; } = 25.0m;
    public decimal TolerancePercentage { get; set; } = 0.5m;
    public string MixingOrderStep { get; set; } = "Bước 1: Hòa tan dung môi";
}
