using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Ca sản xuất & năng lực chuyền / máy (UC_MFG_005).</summary>
public class MfgShiftCapacity : TenantEntity
{
    public string ShiftCode { get; set; } = "";
    public string ShiftName { get; set; } = "";
    public string WorkCenterCode { get; set; } = "";
    public decimal AvailableHoursPerShift { get; set; } = 8.0m;
    public decimal EfficiencyFactorPct { get; set; } = 85.0m;
    public decimal MaxCapacityOutputUnits { get; set; } = 500;
    public bool IsActive { get; set; } = true;
}
