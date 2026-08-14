using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Cảnh báo thiết bị sắp/đã hết hạn bảo hành (UC_FSM_011).</summary>
public class FsmWarrantyExpiryAlert : TenantEntity
{
    public Guid AssetId { get; set; }
    public string SerialNumber { get; set; } = "";
    public string ModelName { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public DateTimeOffset WarrantyEndDate { get; set; }
    public int DaysRemaining { get; set; }
    public string AlertStatus { get; set; } = "ExpiringSoon"; // ExpiringSoon | Expired | Renewed
    public bool IsNotifiedToCustomer { get; set; }
    public DateTimeOffset AlertGeneratedAt { get; set; } = DateTimeOffset.UtcNow;
}
