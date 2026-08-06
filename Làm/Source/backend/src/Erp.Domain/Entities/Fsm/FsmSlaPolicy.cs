using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Cấu hình SLA theo mức ưu tiên (UC_FSM_005, 014).</summary>
public class FsmSlaPolicy : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    /// <summary>Low · Normal · High · Critical</summary>
    public string Priority { get; set; } = "Normal";
    public int ResponseHours { get; set; } = 8;
    public int ResolveHours { get; set; } = 48;
    public bool IsActive { get; set; } = true;
    public string? Note { get; set; }
}
