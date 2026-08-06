using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Mã lỗi (UC_FSM_002).</summary>
public class FsmFaultCode : TenantEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    /// <summary>Low · Medium · High</summary>
    public string Severity { get; set; } = "Medium";
    public string Status { get; set; } = "Active";
    public string? Note { get; set; }
}
