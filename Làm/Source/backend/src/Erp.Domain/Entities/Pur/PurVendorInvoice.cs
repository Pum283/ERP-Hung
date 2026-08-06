using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pur;

/// <summary>Hóa đơn NCC (UC_PUR_040–043).</summary>
public class PurVendorInvoice : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid VendorId { get; set; }
    public Guid? PoId { get; set; }
    public string InvoiceNumber { get; set; } = "";
    public DateTimeOffset InvoiceDate { get; set; } = DateTimeOffset.UtcNow;
    /// <summary>Draft · Matched · Posted · Disputed</summary>
    public string Status { get; set; } = "Draft";
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    /// <summary>Pending · Matched · Variance</summary>
    public string MatchStatus { get; set; } = "Pending";
    public string? MatchNote { get; set; }
    /// <summary>None · Pushed · Failed</summary>
    public string ApPushStatus { get; set; } = "None";
    public string? Note { get; set; }
}
