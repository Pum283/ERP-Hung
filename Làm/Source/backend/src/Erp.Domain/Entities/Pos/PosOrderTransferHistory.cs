using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Lịch sử chuyển đơn/bàn giữa các quầy POS (UC_POS_029).</summary>
public class PosOrderTransferHistory : TenantEntity
{
    public Guid OrderId { get; set; }
    public string FromCounterCode { get; set; } = "";
    public string ToCounterCode { get; set; } = "";
    public Guid TransferByUserId { get; set; }
    public string Notes { get; set; } = "";
    public DateTimeOffset TransferredAt { get; set; } = DateTimeOffset.UtcNow;
}
