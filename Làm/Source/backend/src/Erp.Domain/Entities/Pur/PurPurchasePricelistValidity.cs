using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

/// <summary>Hiệu lực bảng giá mua (UC_PUR_011).</summary>
public class PurPurchasePricelistValidity : TenantEntity
{
    public Guid SupplierId { get; set; }
    public string PricelistCode { get; set; } = "";
    public string PricelistName { get; set; } = "";
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset EffectiveTo { get; set; }
    public string Currency { get; set; } = "VND";
    public bool IsActive { get; set; } = true;
    public string ItemsJson { get; set; } = "[]";
}
