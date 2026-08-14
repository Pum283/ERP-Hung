using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Theo dõi hành trình serial (UC_INV_046).</summary>
public class InvSerialTrackingHistory : TenantEntity
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = "";
    public string SerialNumber { get; set; } = "";
    public string EventType { get; set; } = "GoodsReceipt"; // GoodsReceipt | InternalTransfer | SalesDispatch | CustomerReturn | Maintenance
    public string CurrentLocation { get; set; } = "Kho Tổng TP.HCM";
    public string DocumentReference { get; set; } = "";
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
