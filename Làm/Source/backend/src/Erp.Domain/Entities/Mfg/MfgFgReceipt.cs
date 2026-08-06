using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Nhập thành phẩm (UC_MFG_022).</summary>
public class MfgFgReceipt : TenantEntity
{
    public Guid WorkOrderId { get; set; }
    public Guid ItemId { get; set; }
    public decimal Qty { get; set; }
    public string Unit { get; set; } = "CAI";
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid ReceivedBy { get; set; }
    public string? Note { get; set; }
}
