using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Lệnh sản xuất (UC_MFG_017–022).</summary>
public class MfgWorkOrder : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid ItemId { get; set; }
    public decimal Qty { get; set; }
    public Guid? WorkshopId { get; set; }
    public Guid? BomId { get; set; }
    public Guid? PlanId { get; set; }
    /// <summary>Draft · Approved · Released · MaterialsIssued · Paused · Completed · Closed · Cancelled</summary>
    public string Status { get; set; } = "Draft";
    public string? Note { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? ReleasedAt { get; set; }
    public DateTimeOffset? PrintedAt { get; set; }
    public decimal QtyIssuedMaterial { get; set; }
    public decimal QtyFgReceived { get; set; }
    public decimal QtyScrap { get; set; }
    public DateTimeOffset? PausedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public string? CancelReason { get; set; }
    /// <summary>Status trước khi Pause — dùng Resume.</summary>
    public string? ResumeStatus { get; set; }
}
