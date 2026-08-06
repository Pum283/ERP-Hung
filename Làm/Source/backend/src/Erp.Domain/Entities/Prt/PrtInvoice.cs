using Erp.Domain.Base;

namespace Erp.Domain.Entities.Prt;

/// <summary>Hóa đơn / công nợ portal (UC_PRT_014–015).</summary>
public class PrtInvoice : TenantEntity
{
    public Guid AccountId { get; set; }
    public string Code { get; set; } = "";
    public DateTimeOffset InvoiceDate { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OpenAmount { get; set; }
    /// <summary>Open · Partial · Paid</summary>
    public string Status { get; set; } = "Open";
}
