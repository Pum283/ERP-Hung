using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Nhóm sản phẩm bán (UC_POS_009).</summary>
public class PosProductCategory : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
