using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

public class InvStocktakeLine : TenantEntity
{
    public Guid StocktakeId { get; set; }
    public Guid SkuId { get; set; }
    public string SkuCode { get; set; } = "";
    public string SkuName { get; set; } = "";
    public string? LotCode { get; set; }
    public decimal SystemQty { get; set; }
    public decimal? CountedQty { get; set; }
    public decimal VarianceQty { get; set; }
}
