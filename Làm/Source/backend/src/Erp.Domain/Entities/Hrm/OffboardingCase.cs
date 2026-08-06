using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Đơn nghỉ việc / offboarding (UC_HRM_144–151).</summary>
public class OffboardingCase : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public DateOnly RequestDate { get; set; }
    public DateOnly LastWorkingDay { get; set; }
    public string ReasonCode { get; set; } = "Personal";
    public string? ReasonDetail { get; set; }
    /// <summary>Draft | Submitted | Approved | Rejected | InProgress | Completed | Cancelled</summary>
    public string Status { get; set; } = "Draft";
    public bool NoticeSatisfied { get; set; }
    public int RequiredNoticeDays { get; set; }
    public string ChecklistJson { get; set; } = "[]";
    public bool AccessRevoked { get; set; }
    public DateTimeOffset? AccessRevokedAt { get; set; }
    public decimal? LeaveDaysRemaining { get; set; }
    public decimal? LeaveSettlementAmount { get; set; }
    public decimal? FinalPayEstimate { get; set; }
    public string? SettlementNote { get; set; }
    public string? InterviewNotes { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public string? RejectReason { get; set; }
}
