using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fin;

/// <summary>Chứng từ / dòng bảng kê GTGT (UC_FIN_052–053).</summary>
public class FinVatDocument : TenantEntity
{
    public string Code { get; set; } = "";
    /// <summary>Output · Input</summary>
    public string Direction { get; set; } = "Output";
    public Guid? TaxId { get; set; }
    public decimal RatePercent { get; set; }
    public string InvoiceNo { get; set; } = "";
    public string? InvoiceSeries { get; set; }
    public DateTimeOffset InvoiceDate { get; set; } = DateTimeOffset.UtcNow;
    public string? PartnerCode { get; set; }
    public string? PartnerName { get; set; }
    public string? PartnerTaxCode { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public Guid? PeriodId { get; set; }
    public Guid? ArInvoiceId { get; set; }
    public Guid? ApInvoiceId { get; set; }
    /// <summary>Draft · Posted · Void</summary>
    public string Status { get; set; } = "Draft";
    public DateTimeOffset? PostedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string? Note { get; set; }
}
