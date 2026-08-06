using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Danh mục phụ cấp (UC_HRM_157).</summary>
public class AllowanceType : TenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal DefaultAmount { get; set; }
    public bool IsTaxable { get; set; } = true;
    public bool IsActive { get; set; } = true;
}
