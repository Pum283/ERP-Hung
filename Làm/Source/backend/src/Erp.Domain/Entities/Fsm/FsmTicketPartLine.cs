using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Xuất linh kiện theo ticket (UC_FSM_024).</summary>
public class FsmTicketPartLine : TenantEntity
{
    public Guid TicketId { get; set; }
    public Guid PartId { get; set; }
    public decimal Qty { get; set; }
    public decimal UnitCost { get; set; }
    /// <summary>Tech | Warehouse</summary>
    public string Source { get; set; } = "Tech";
    public Guid? TechUserId { get; set; }
    public string? TechName { get; set; }
    public DateTimeOffset IssuedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? Note { get; set; }
}