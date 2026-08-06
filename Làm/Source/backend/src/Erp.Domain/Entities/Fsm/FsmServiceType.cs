using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Loại dịch vụ (UC_FSM_001).</summary>
public class FsmServiceType : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Status { get; set; } = "Active";
    public string? Note { get; set; }
}
