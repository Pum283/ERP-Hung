using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Khảo sát hiểu bài & tuân thủ (UC_LMS_056, 057).</summary>
public class LmsSurvey : TenantEntity
{
    public string Title { get; set; } = "";
    /// <summary>Comprehension · Compliance · GeneralFeedback</summary>
    public string SurveyType { get; set; } = "Comprehension";
    public Guid? CourseId { get; set; }
    public bool IsMandatory { get; set; } = true;
    public bool MustCompleteBeforeShift { get; set; }
}
