using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Cảnh báo quá hạn đào tạo (UC_LMS_064).</summary>
public class LmsOverdueTrainingAlert : TenantEntity
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public DateTimeOffset DueDate { get; set; }
    public int OverdueDays { get; set; }
    public DateTimeOffset AlertSentAt { get; set; } = DateTimeOffset.UtcNow;
    public string AlertStatus { get; set; } = "Sent"; // Sent | Resolved | Escalated
}
