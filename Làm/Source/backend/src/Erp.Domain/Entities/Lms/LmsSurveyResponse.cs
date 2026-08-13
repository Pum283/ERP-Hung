using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Kết quả thực hiện khảo sát của học viên (UC_LMS_056, UC_LMS_057).</summary>
public class LmsSurveyResponse : TenantEntity
{
    public Guid SurveyId { get; set; }
    public Guid StudentUserId { get; set; }
    public string AnswersJson { get; set; } = "{}";
    public decimal Score { get; set; }
    public bool IsPassed { get; set; }
    public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.UtcNow;
}
