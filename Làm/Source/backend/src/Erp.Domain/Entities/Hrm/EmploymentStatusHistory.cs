using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

public class EmploymentStatusHistory : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public string? Note { get; set; }
    public Guid? ChangedByUserId { get; set; }
}
