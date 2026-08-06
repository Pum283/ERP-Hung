using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Đối soát linh kiện (UC_FSM_039).</summary>
public class FsmPartReconcileDoc : TenantEntity
{
    public string Code { get; set; } = "";
    /// <summary>Warehouse | Tech</summary>
    public string Scope { get; set; } = "Warehouse";
    public Guid? TechUserId { get; set; }
    public string? TechName { get; set; }
    /// <summary>Draft | Posted</summary>
    public string Status { get; set; } = "Draft";
    public string? Note { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public ICollection<FsmPartReconcileLine> Lines { get; set; } = new List<FsmPartReconcileLine>();
}

public class FsmPartReconcileLine : TenantEntity
{
    public Guid ReconcileDocId { get; set; }
    public Guid PartId { get; set; }
    public decimal SystemQty { get; set; }
    public decimal CountedQty { get; set; }
    public decimal DiffQty { get; set; }
    public decimal UnitCost { get; set; }
}
