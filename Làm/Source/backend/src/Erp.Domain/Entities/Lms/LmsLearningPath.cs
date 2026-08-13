using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Lộ trình đào tạo theo chức danh / vị trí (UC_LMS_061).</summary>
public class LmsLearningPath : TenantEntity
{
    public string Title { get; set; } = "";
    public string JobTitle { get; set; } = "";
    public string Description { get; set; } = "";
    public int TargetDaysToComplete { get; set; } = 30;
    public bool IsActive { get; set; } = true;
}
