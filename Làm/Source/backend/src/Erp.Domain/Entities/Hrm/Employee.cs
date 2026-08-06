using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

public class Employee : TenantEntity
{
    public string EmployeeCode { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateOnly? Dob { get; set; }
    public string? Gender { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public Guid OrgUnitId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? JobLevelId { get; set; }
    public Guid? JobTitleId { get; set; }
    public Guid? EmployeeTypeId { get; set; }
    public Guid? ManagerEmployeeId { get; set; }
    public string Status { get; set; } = "Active";
    public DateOnly? HireDate { get; set; }
    public DateOnly? TerminateDate { get; set; }
}
