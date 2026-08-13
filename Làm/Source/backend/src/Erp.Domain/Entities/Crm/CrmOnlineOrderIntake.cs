using Erp.Domain.Base;

namespace Erp.Domain.Entities.Crm;

/// <summary>Tiếp nhận đơn hàng từ kênh Online (UC_CRM_080).</summary>
public class CrmOnlineOrderIntake : TenantEntity
{
    public string Channel { get; set; } = "Zalo";
    public string ExternalOrderCode { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string Phone { get; set; } = "";
    public decimal TotalAmount { get; set; }
    /// <summary>Received | Verified | Processed</summary>
    public string Status { get; set; } = "Received";
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
}
