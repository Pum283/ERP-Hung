using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pjm;

/// <summary>Quản lý giá trị thu hoạch EVM (UC_PJM).</summary>
public class PjmEarnedValue : TenantEntity
{
    public Guid ProjectId { get; set; }
    public decimal PlannedValue { get; set; }  // PV
    public decimal EarnedValue { get; set; }   // EV
    public decimal ActualCost { get; set; }    // AC
    public decimal CostVariance => EarnedValue - ActualCost; // CV
    public decimal ScheduleVariance => EarnedValue - PlannedValue; // SV
    public decimal CostPerformanceIndex => ActualCost > 0 ? EarnedValue / ActualCost : 1.0m; // CPI
    public decimal SchedulePerformanceIndex => PlannedValue > 0 ? EarnedValue / PlannedValue : 1.0m; // SPI
}
