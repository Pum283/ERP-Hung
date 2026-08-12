using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Thực thể gán tag kỹ năng / vị trí cho khóa học (UC_LMS_007).</summary>
public class LmsCourseSkillTag : TenantEntity
{
    public Guid CourseId { get; set; }
    public string TagName { get; set; } = string.Empty;
    /// <summary>Skill | Position | General</summary>
    public string TagType { get; set; } = "Skill";
    public Guid? RelatedRefId { get; set; }
}
