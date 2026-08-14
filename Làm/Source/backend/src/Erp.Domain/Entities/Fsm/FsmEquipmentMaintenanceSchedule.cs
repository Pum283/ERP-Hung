using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Lịch bảo trì định kỳ theo thiết bị khách hàng (UC_FSM_033).</summary>
public class FsmEquipmentMaintenanceSchedule : TenantEntity
{
    public Guid AssetId { get; set; }
    public string SerialNumber { get; set; } = "";
    public string ModelName { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string MaintenanceFrequency { get; set; } = "Quarterly"; // Monthly | Quarterly | SemiAnnual | Annual
    public DateTimeOffset NextDueDate { get; set; } = DateTimeOffset.UtcNow.AddMonths(3);
    public bool AutoGenerateTicket { get; set; } = true;
    public bool IsActive { get; set; } = true;
}
