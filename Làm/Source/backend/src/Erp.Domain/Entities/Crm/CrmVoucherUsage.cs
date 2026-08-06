using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Lịch sử sử dụng voucher (UC_CRM_035).</summary>
public class CrmVoucherUsage : TenantEntity
{
    public Guid VoucherId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? QuoteId { get; set; }
    public Guid? SalesOrderId { get; set; }
    public decimal DiscountApplied { get; set; }
    public DateTimeOffset UsedAt { get; set; } = DateTimeOffset.UtcNow;
}
