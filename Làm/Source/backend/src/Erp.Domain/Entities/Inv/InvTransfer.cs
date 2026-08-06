using Erp.Domain.Base;

namespace Erp.Domain.Entities.Inv;

/// <summary>Phiếu chuyển kho (UC_INV_031, 033, 035).</summary>
public class InvTransfer : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid FromWarehouseId { get; set; }
    public Guid ToWarehouseId { get; set; }
    /// <summary>Draft · InTransit · Completed · Cancelled</summary>
    public string Status { get; set; } = "Draft";
    public DateTimeOffset? ShippedAt { get; set; }
    public DateTimeOffset? ReceivedAt { get; set; }
    public string? Note { get; set; }
}
