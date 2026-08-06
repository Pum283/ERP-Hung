using Erp.Domain.Base;

namespace Erp.Domain.Entities.Hrm;

/// <summary>Đề xuất / lệnh điều động nhân sự (UC_HRM_092–097).</summary>
public class StaffTransfer : TenantEntity
{
    public string DocNo { get; set; } = string.Empty;
    /// <summary>Request | Order</summary>
    public string Kind { get; set; } = "Order";
    public Guid? EmployeeId { get; set; }
    public Guid FromOrgUnitId { get; set; }
    public Guid ToOrgUnitId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    /// <summary>Số người cần khi Kind=Request.</summary>
    public int? RequestedHeadcount { get; set; }
    /// <summary>
    /// Request: Draft|Submitted|Approved|Rejected|Converted
    /// Order: Draft|Issued|Acknowledged|Active|Completed|Cancelled
    /// </summary>
    public string Status { get; set; } = "Draft";
    /// <summary>Gắn nhãn công điều động khi chấm (UC_HRM_096).</summary>
    public bool AttendanceTagged { get; set; } = true;
    public string AttendanceTag { get; set; } = "TRANSFER";
    public decimal? PlannedHours { get; set; }
    public decimal? ActualHours { get; set; }
    public decimal? CostRate { get; set; }
    public Guid RequestedByUserId { get; set; }
    public Guid? AcknowledgedByUserId { get; set; }
    public DateTimeOffset? AcknowledgedAt { get; set; }
    public Guid? SourceRequestId { get; set; }
    public string? Note { get; set; }
}
