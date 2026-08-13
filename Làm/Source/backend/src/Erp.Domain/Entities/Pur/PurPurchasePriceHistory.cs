using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

/// <summary>Lịch sử biến động giá mua sản phẩm (UC_PUR_012).</summary>
public class PurPurchasePriceHistory : TenantEntity
{
    public Guid ProductId { get; set; }
    public Guid SupplierId { get; set; }
    public decimal UnitPriceVnd { get; set; }
    public decimal PreviousUnitPriceVnd { get; set; }
    public double ChangePercentage { get; set; }
    public string SourceDocumentType { get; set; } = "PO"; // PO | Quotation | Contract
    public string SourceDocumentNumber { get; set; } = "";
    public DateTimeOffset EffectiveDate { get; set; } = DateTimeOffset.UtcNow;
}
