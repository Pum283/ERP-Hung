using Erp.Domain.Base;

namespace Erp.Domain.Entities.Prt;

/// <summary>Lịch sử thanh toán portal (UC_PRT_016).</summary>
public class PrtPayment : TenantEntity
{
    public Guid AccountId { get; set; }
    public Guid? InvoiceId { get; set; }
    public string Code { get; set; } = "";
    public DateTimeOffset PaidAt { get; set; } = DateTimeOffset.UtcNow;
    public decimal Amount { get; set; }
    public string Method { get; set; } = "Transfer";
    public string? Note { get; set; }
}
