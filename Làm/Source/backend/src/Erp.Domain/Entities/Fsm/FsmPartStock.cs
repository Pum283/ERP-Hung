using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Tồn linh kiện kho KT / túi KTV (UC_FSM_037–038).</summary>
public class FsmPartStock : TenantEntity
{
    public Guid PartId { get; set; }
    /// <summary>Warehouse | Tech</summary>
    public string LocationType { get; set; } = "Warehouse";
    public Guid? TechUserId { get; set; }
    public string? TechName { get; set; }
    public decimal QtyOnHand { get; set; }
    public decimal UnitCost { get; set; }
}
