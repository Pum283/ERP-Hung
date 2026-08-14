using Erp.Domain.Base;

namespace Erp.Domain.Entities.Log;

/// <summary>Chi phí vận chuyển & phân bổ cước (UC_LOG_037).</summary>
public class LogShippingCostAllocation : TenantEntity
{
    public string CostAllocationNumber { get; set; } = "";
    public string TripNumber { get; set; } = "";
    public decimal TotalFuelCostVnd { get; set; }
    public decimal TotalTollFeeVnd { get; set; }
    public decimal DriverAllowanceVnd { get; set; }
    public decimal TotalTripCostVnd { get; set; }
    public int AllocatedOrdersCount { get; set; }
    public decimal AverageCostPerOrderVnd { get; set; }
    public DateTimeOffset CalculatedAt { get; set; } = DateTimeOffset.UtcNow;
}
