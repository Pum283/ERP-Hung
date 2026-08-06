using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Phiếu xin điều chỉnh công (UC_HRM_120–122).</summary>
public class AttendanceAdjustRequest : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public DateOnly WorkDate { get; set; }
    public Guid? AttendanceRecordId { get; set; }
    public DateTimeOffset? RequestedCheckInAt { get; set; }
    public DateTimeOffset? RequestedCheckOutAt { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? EvidenceStorageKey { get; set; }
    /// <summary>Draft | Submitted | Approved | Rejected | Cancelled</summary>
    public string Status { get; set; } = "Draft";
    public Guid RequestedByUserId { get; set; }
    public Guid? DecidedByUserId { get; set; }
    public DateTimeOffset? DecidedAt { get; set; }
}
