using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

/// <summary>Trả hàng nhà cung cấp (RTV - Return to Vendor) (UC_PUR_038).</summary>
public class PurVendorReturn : TenantEntity
{
    public Guid RejectionId { get; set; }
    public Guid SupplierId { get; set; }
    public string RtvNumber { get; set; } = "";
    public decimal TotalReturnValueVnd { get; set; }
    public string CreditMemoStatus { get; set; } = "PendingCreditMemo"; // PendingCreditMemo | CreditMemoIssued | Refunded
    public string Notes { get; set; } = "";
    public DateTimeOffset ReturnedAt { get; set; } = DateTimeOffset.UtcNow;
}
