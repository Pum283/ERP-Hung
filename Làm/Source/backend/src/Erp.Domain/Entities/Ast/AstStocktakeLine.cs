using Erp.Domain.Base;

namespace Erp.Domain.Entities.Ast;

public class AstStocktakeLine : TenantEntity
{
    public Guid StocktakeId { get; set; }
    public Guid AssetId { get; set; }
    public string AssetCode { get; set; } = "";
    public string AssetName { get; set; } = "";
    public Guid? LocationId { get; set; }
    public string? LocationName { get; set; }
    /// <summary>1 = có trên sổ (Active snapshot).</summary>
    public int ExpectedPresent { get; set; } = 1;
    /// <summary>null = chưa đếm · true=có · false=không.</summary>
    public bool? CountedPresent { get; set; }
    /// <summary>-1 thiếu · 0 khớp · +1 thừa.</summary>
    public int Variance { get; set; }
    public string? Note { get; set; }
}
