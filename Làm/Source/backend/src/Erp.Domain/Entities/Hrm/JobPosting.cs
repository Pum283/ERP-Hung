using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Tin tuyển từ phiếu nhu cầu đã duyệt (L2 · UC_HRM_054+).</summary>
public class JobPosting : TenantEntity
{
    public Guid RecruitmentRequestId { get; set; }
    public string Title { get; set; } = "";
    /// <summary>Kênh đăng: Internal | Website | Facebook | LinkedIn | Other</summary>
    public string Channel { get; set; } = "Internal";
    /// <summary>Open | Closed</summary>
    public string Status { get; set; } = "Open";
    public Guid CreatedByUserId { get; set; }
}
