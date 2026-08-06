using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Thu hồi chứng chỉ LMS (UC_LMS_047).</summary>
public class LmsCertificateRevocation : TenantEntity
{
    public Guid CertificateId { get; set; }
    public string RevocationReason { get; set; } = "";
    public DateTimeOffset RevokedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid RevokedByUserId { get; set; }
}
