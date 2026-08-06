using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Thực thể tự đánh giá nhân viên (UC_HRM_180).</summary>
public class HrmSelfEvaluation : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public string AppraisalPeriod { get; set; } = string.Empty;
    public string KeyAchievements { get; set; } = string.Empty;
    public string AreasForImprovement { get; set; } = string.Empty;
    public int SelfRating { get; set; } = 5; // 1 to 5 scale
    public string Status { get; set; } = "Draft"; // Draft | Submitted | Approved
}
