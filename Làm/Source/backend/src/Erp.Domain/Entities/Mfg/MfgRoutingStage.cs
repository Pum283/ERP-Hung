using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Danh mục công đoạn sản xuất (UC_MFG_004).</summary>
public class MfgRoutingStage : TenantEntity
{
    public string StageCode { get; set; } = "";
    public string StageName { get; set; } = "";
    public string WorkCenterCode { get; set; } = "";
    public decimal StandardCycleTimeMinutes { get; set; } = 15;
    public decimal StandardSetupTimeMinutes { get; set; } = 30;
    public bool IsOutsourced { get; set; } = false;
    public bool IsActive { get; set; } = true;
}
