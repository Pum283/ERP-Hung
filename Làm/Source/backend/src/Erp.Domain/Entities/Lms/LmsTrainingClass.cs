using Erp.Domain.Base;

namespace Erp.Domain.Entities.Lms;

/// <summary>Lớp đào tạo offline (UC_LMS_016).</summary>
public class LmsTrainingClass : TenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public Guid? InstructorId { get; set; }
    public string? InstructorName { get; set; }
    public string? Location { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    /// <summary>Draft | Open | InProgress | Closed</summary>
    public string Status { get; set; } = "Draft";
    public string? SummaryNote { get; set; }
}
