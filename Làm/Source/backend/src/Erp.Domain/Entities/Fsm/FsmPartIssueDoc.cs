using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Phiếu cấp linh kiện kho KT → KTV (UC_FSM_038).</summary>
public class FsmPartIssueDoc : TenantEntity
{
    public string Code { get; set; } = "";
    public Guid TechUserId { get; set; }
    public string TechName { get; set; } = "";
    /// <summary>Draft | Posted</summary>
    public string Status { get; set; } = "Draft";
    public string? Note { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public ICollection<FsmPartIssueLine> Lines { get; set; } = new List<FsmPartIssueLine>();
}

public class FsmPartIssueLine : TenantEntity
{
    public Guid IssueDocId { get; set; }
    public Guid PartId { get; set; }
    public decimal Qty { get; set; }
    public decimal UnitCost { get; set; }
}
