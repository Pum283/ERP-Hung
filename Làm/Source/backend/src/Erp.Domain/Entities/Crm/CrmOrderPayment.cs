using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Theo dõi thanh toán đơn (UC_CRM_087).</summary>
public class CrmOrderPayment : TenantEntity
{
    public Guid OrderId { get; set; }
    public string Code { get; set; } = "";
    public DateTimeOffset PaidAt { get; set; } = DateTimeOffset.UtcNow;
    public decimal Amount { get; set; }
    public string Method { get; set; } = "Transfer";
    public string? Note { get; set; }
}
