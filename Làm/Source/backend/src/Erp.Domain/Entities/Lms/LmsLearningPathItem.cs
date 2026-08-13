using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Chi tiết khóa học trong lộ trình đào tạo (UC_LMS_061).</summary>
public class LmsLearningPathItem : TenantEntity
{
    public Guid LearningPathId { get; set; }
    public Guid CourseId { get; set; }
    public int SequenceOrder { get; set; } = 1;
    public bool IsMandatory { get; set; } = true;
}
