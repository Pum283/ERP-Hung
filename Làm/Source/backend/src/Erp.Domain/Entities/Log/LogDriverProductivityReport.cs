using Erp.Domain.Base;

namespace Erp.Domain.Entities.Log;

/// <summary>Năng suất tài xế / chuyến (UC_LOG_036).</summary>
public class LogDriverProductivityReport : TenantEntity
{
    public Guid DriverVehicleId { get; set; }
    public string DriverName { get; set; } = "";
    public int CompletedTripsCount { get; set; }
    public int DeliveredOrdersCount { get; set; }
    public decimal TotalWeightDeliveredKg { get; set; }
    public double OnTimeDeliveryRatePct { get; set; } = 98.5;
    public DateTimeOffset PeriodStartDate { get; set; }
    public DateTimeOffset PeriodEndDate { get; set; }
}
