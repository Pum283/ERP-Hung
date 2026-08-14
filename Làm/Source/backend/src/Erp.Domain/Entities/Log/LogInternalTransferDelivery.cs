using Erp.Domain.Base;

namespace Erp.Domain.Entities.Log;

/// <summary>Lệnh giao nội bộ & xác nhận nhận hàng (UC_LOG_031, UC_LOG_032).</summary>
public class LogInternalTransferDelivery : TenantEntity
{
    public string InternalDeliveryNumber { get; set; } = "";
    public Guid FromWarehouseId { get; set; }
    public string FromWarehouseName { get; set; } = "";
    public Guid ToWarehouseId { get; set; }
    public string ToWarehouseName { get; set; } = "";
    public string DriverName { get; set; } = "";
    public string VehiclePlateNumber { get; set; } = "";
    public decimal DispatchedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public string Status { get; set; } = "InTransit"; // InTransit | Received | DiscrepancyReported
    public string ReceiverStaffName { get; set; } = "";
    public DateTimeOffset DispatchedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ReceivedAt { get; set; }
}
