using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Hồ sơ onboarding gắn NV (thường từ ứng viên Accepted).</summary>
public class OnboardingCase : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public Guid? CandidateId { get; set; }
    public Guid? MentorEmployeeId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly OnboardingDueDate { get; set; }
    public DateOnly TrialEndDate { get; set; }
    /// <summary>InProgress | TrialPassed | Converted | Cancelled</summary>
    public string Status { get; set; } = "InProgress";
    public int? TrialScore { get; set; }
    public string? TrialComment { get; set; }
    /// <summary>JSON checklist: [{key,label,done}]</summary>
    public string ChecklistJson { get; set; } = "[]";
}
