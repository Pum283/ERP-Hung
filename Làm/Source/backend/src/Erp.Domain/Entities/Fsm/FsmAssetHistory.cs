using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Lịch sử BH / sửa chữa (UC_FSM_010).</summary>
public class FsmAssetHistory : TenantEntity
{
    public Guid AssetId { get; set; }
    /// <summary>Warranty · Repair · Ticket · Note</summary>
    public string EventType { get; set; } = "Note";
    public string Summary { get; set; } = "";
    public Guid? TicketId { get; set; }
    public Guid ActorUserId { get; set; }
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}
