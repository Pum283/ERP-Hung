using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

/// <summary>Báo giá từ nhà cung cấp (UC_PUR_022 & UC_PUR_024).</summary>
public class PurVendorQuotation : TenantEntity
{
    public Guid RfqId { get; set; }
    public Guid SupplierId { get; set; }
    public string QuotationNumber { get; set; } = "";
    public decimal TotalAmountVnd { get; set; }
    public int DeliveryLeadTimeDays { get; set; }
    public string PaymentTerms { get; set; } = "Net 30";
    public bool IsAwardedWinner { get; set; } = false;
    public string ItemsJson { get; set; } = "[]";
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
}
