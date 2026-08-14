using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Vùng địa lý phụ trách dịch vụ kỹ thuật (UC_FSM_007).</summary>
public class FsmTerritoryCoverage : TenantEntity
{
    public string TerritoryCode { get; set; } = "";
    public string TerritoryName { get; set; } = "";
    public string ProvinceOrCity { get; set; } = "Hồ Chí Minh";
    public string AssignedHubWarehouseCode { get; set; } = "HUB-HCM-01";
    public Guid LeadTechnicianUserId { get; set; }
    public string LeadTechnicianName { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
