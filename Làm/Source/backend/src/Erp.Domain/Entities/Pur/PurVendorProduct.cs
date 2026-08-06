using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

/// <summary>Gắn sản phẩm – NCC (UC_PUR_009).</summary>
public class PurVendorProduct : TenantEntity
{
    public Guid VendorId { get; set; }
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public bool IsPreferred { get; set; }
}
