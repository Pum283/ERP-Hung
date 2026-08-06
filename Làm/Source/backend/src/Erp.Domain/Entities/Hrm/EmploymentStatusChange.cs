using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

public class EmploymentStatusChange : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public string FromStatus { get; set; } = "";
    public string ToStatus { get; set; } = "";
    public DateOnly EffectiveDate { get; set; }
    public string? Reason { get; set; }
    public Guid? OrgUnitId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? JobTitleId { get; set; }
}
