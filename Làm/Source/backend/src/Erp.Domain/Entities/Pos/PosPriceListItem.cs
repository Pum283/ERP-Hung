using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Dòng giá SP trong bảng giá.</summary>
public class PosPriceListItem : TenantEntity
{
    public Guid PriceListId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Price { get; set; }
    public Guid? TaxRateId { get; set; }
}
