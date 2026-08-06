using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

/// <summary>Bảng giá mua theo NCC (UC_PUR_010).</summary>
public class PurVendorPrice : TenantEntity
{
    public Guid VendorId { get; set; }
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "VND";
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
}
