using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Linh kiện (UC_FSM_003).</summary>
public class FsmPart : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Unit { get; set; } = "CAI";
    public string Status { get; set; } = "Active";
    public string? Note { get; set; }
}
