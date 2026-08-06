using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Chứng chỉ điện tử (UC_LMS_044–045).</summary>
public class LmsCertificate : TenantEntity
{
    public Guid CourseId { get; set; }
    public Guid UserId { get; set; }
    public Guid? EnrollmentId { get; set; }
    public Guid? FinalAttemptId { get; set; }
    /// <summary>Mã xác thực ngắn (UC_LMS_046 stub)</summary>
    public string Code { get; set; } = "";
    public DateTimeOffset IssuedAt { get; set; }
    /// <summary>Active · Revoked</summary>
    public string Status { get; set; } = "Active";
    public decimal? ScoreAtIssue { get; set; }
}