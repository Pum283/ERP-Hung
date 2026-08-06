using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Thang bậc lương (UC_HRM_152).</summary>
public class SalaryGrade : TenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public decimal BaseAmount { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Note { get; set; }
}
