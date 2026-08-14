using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Kỹ năng và chứng chỉ kỹ thuật viên (UC_FSM_006).</summary>
public class FsmTechnicianSkillCert : TenantEntity
{
    public Guid TechnicianUserId { get; set; }
    public string TechnicianName { get; set; } = "";
    public string SkillCode { get; set; } = "";
    public string SkillName { get; set; } = "Cơ Điện Tử & Lạnh Công Nghiệp";
    public string CertificationLevel { get; set; } = "Bậc 4/7";
    public string CertificateNumber { get; set; } = "";
    public DateTimeOffset IssuedDate { get; set; } = DateTimeOffset.UtcNow.AddYears(-1);
    public DateTimeOffset? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
}
