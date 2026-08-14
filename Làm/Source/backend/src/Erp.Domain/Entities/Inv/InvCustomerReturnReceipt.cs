using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Nhập trả từ khách hàng (UC_INV_021).</summary>
public class InvCustomerReturnReceipt : TenantEntity
{
    public string ReceiptNumber { get; set; } = "";
    public Guid CustomerId { get; set; }
    public Guid SalesOrderId { get; set; }
    public string ReturnReason { get; set; } = "";
    public string InspectionCondition { get; set; } = "GoodRestockable"; // GoodRestockable | DamagedScrap | NeedsRefurbish
    public decimal TotalRefundAmountVnd { get; set; }
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
}
