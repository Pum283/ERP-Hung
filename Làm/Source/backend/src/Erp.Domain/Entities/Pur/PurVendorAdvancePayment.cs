using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

/// <summary>Tạm ứng nhà cung cấp (UC_PUR_044).</summary>
public class PurVendorAdvancePayment : TenantEntity
{
    public Guid PurchaseOrderId { get; set; }
    public Guid SupplierId { get; set; }
    public string RequestNumber { get; set; } = "";
    public decimal AdvanceAmountVnd { get; set; }
    public string PaymentReason { get; set; } = "";
    public string Status { get; set; } = "Approved"; // PendingApproval | Approved | Paid
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
}
