using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Ghi danh học online + mở khóa sau thanh toán mock (UC_LMS_031, 033).</summary>
public class LmsOnlineEnrollment : TenantEntity
{
    public Guid CourseId { get; set; }
    public Guid UserId { get; set; }
    /// <summary>Pending · Unlocked · Completed</summary>
    public string Status { get; set; } = "Pending";
    public decimal PaidAmount { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public Guid? LastLessonId { get; set; }
}
