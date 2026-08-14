using Erp.Domain.Base;

namespace Erp.Domain.Entities.Mfg;

/// <summary>Lịch sản xuất chi tiết theo xưởng/ca (UC_MFG_016).</summary>
public class MfgWorkshopShiftSchedule : TenantEntity
{
    public string ScheduleNumber { get; set; } = "";
    public string WorkshopCode { get; set; } = "";
    public string ShiftCode { get; set; } = "";
    public DateTimeOffset ScheduledDate { get; set; }
    public Guid WorkOrderId { get; set; }
    public string WorkOrderNumber { get; set; } = "";
    public decimal TargetQuantity { get; set; }
    public string Status { get; set; } = "Scheduled"; // Scheduled | Running | Completed
}
