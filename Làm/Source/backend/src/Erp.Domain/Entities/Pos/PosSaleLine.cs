using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Dòng SP trên đơn POS (UC_POS_027, 038).</summary>
public class PosSaleLine : TenantEntity
{
    public Guid SaleId { get; set; }
    public Guid? ProductId { get; set; }
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TaxRatePct { get; set; }
    public decimal LineAmount { get; set; }
    /// <summary>Active · Cancelled</summary>
    public string Status { get; set; } = "Active";
    public int LineNo { get; set; }
}
