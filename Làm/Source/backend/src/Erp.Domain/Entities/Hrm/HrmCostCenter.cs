using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Trung tâm chi phí nhân sự (UC_HRM_011).</summary>
public class HrmCostCenter : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public Guid? OrgUnitId { get; set; }
    public decimal AllocationPercentage { get; set; } = 100m;
    public bool IsActive { get; set; } = true;
}
