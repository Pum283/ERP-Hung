using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Thanh toán đơn POS (UC_POS_033–035).</summary>
public class PosSalePayment : TenantEntity
{
    public Guid SaleId { get; set; }
    public string Code { get; set; } = "";
    public DateTimeOffset PaidAt { get; set; } = DateTimeOffset.UtcNow;
    public decimal Amount { get; set; }
    /// <summary>Cash · Transfer · Card · Wallet</summary>
    public string Method { get; set; } = "Cash";
    public string? Note { get; set; }
}
