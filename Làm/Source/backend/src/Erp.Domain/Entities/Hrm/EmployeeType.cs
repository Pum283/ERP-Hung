using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

public class EmployeeType : TenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
