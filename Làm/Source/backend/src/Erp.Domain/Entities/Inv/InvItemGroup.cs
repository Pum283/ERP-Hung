using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Nhóm hàng / ngành hàng (UC_INV_002).</summary>
public class InvItemGroup : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
