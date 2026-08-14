using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Quy tắc tự động phân công KTV theo kỹ năng & địa bàn (UC_FSM_016).</summary>
public class FsmAutoDispatchRule : TenantEntity
{
    public string RuleName { get; set; } = "Auto Dispatch Theo Khu Vực & Kỹ Năng";
    public string TerritoryCode { get; set; } = "REGION-SOUTH-01";
    public string RequiredSkillCode { get; set; } = "SKILL-HVAC";
    public int MaxActiveTicketsPerTech { get; set; } = 5;
    public bool AutoAssignOnTicketCreation { get; set; } = true;
    public bool IsActive { get; set; } = true;
}
