using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Đối soát chứng từ đơn hàng (UC_CRM_102).</summary>
public class CrmOrderDocumentReconciliation : TenantEntity
{
    public Guid OrderId { get; set; }
    public string DocumentCode { get; set; } = "";
    /// <summary>VATInvoice | DeliveryNote | PaymentReceipt</summary>
    public string DocumentType { get; set; } = "VATInvoice";
    /// <summary>Pending | Matched | Discrepancy</summary>
    public string ReconciliationStatus { get; set; } = "Pending";
    public string Notes { get; set; } = "";
    public DateTimeOffset ReconciledAt { get; set; } = DateTimeOffset.UtcNow;
}
