using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

/// <summary>Đơn mua hàng PO (UC_PUR_026–033).</summary>
public class PurPurchaseOrder : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid VendorId { get; set; }
    public Guid? SourcePrId { get; set; }
    /// <summary>Draft · PendingApproval · Approved · Sent · Closed · Cancelled</summary>
    public string Status { get; set; } = "Draft";
    public int Version { get; set; } = 1;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "VND";
    public string? Note { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public DateTimeOffset? PrintedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public string? CancelReason { get; set; }
}
