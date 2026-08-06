using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Bảng giá theo điểm bán (UC_POS_016).</summary>
public class PosPriceList : TenantEntity
{
    public Guid StoreId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    /// <summary>Active · Inactive</summary>
    public string Status { get; set; } = "Active";
}
