using Erp.Domain.Base;

namespace Erp.Domain.Entities.Log;

/// <summary>Dòng hàng lệnh giao / pick (UC_LOG_006, 009).</summary>
public class LogDeliveryLine : TenantEntity
{
    public Guid DeliveryOrderId { get; set; }
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public decimal Qty { get; set; }
    public decimal QtyPicked { get; set; }
    public string Unit { get; set; } = "CAI";
    public string? Note { get; set; }
}
