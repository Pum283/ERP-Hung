using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Câu trong đề cố định (UC_LMS_012).</summary>
public class LmsExamQuestion : TenantEntity
{
    public Guid ExamId { get; set; }
    public Guid QuestionId { get; set; }
    public int SortOrder { get; set; }
    public decimal? PointsOverride { get; set; }
}
