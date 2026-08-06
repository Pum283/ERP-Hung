using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fin;

/// <summary>Nhóm tài khoản (UC_FIN_002).</summary>
public class FinAccountGroup : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
