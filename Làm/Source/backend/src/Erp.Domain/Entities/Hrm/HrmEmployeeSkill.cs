using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Quản lý trình độ / kỹ năng nhân sự (UC_HRM_024).</summary>
public class HrmEmployeeSkill : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public string SkillName { get; set; } = "";
    /// <summary>Basic · Intermediate · Advanced · Expert</summary>
    public string ProficiencyLevel { get; set; } = "Intermediate";
    public string? CertificateRef { get; set; }
}
