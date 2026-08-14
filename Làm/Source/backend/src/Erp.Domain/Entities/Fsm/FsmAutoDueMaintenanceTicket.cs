using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Ticket bảo trì tự động phát sinh khi đến hạn (UC_FSM_034).</summary>
public class FsmAutoDueMaintenanceTicket : TenantEntity
{
    public string GeneratedTicketNumber { get; set; } = "";
    public Guid AssetId { get; set; }
    public string SerialNumber { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string MaintenanceType { get; set; } = "Bảo Trì Định Kỳ Quý 3";
    public DateTimeOffset ScheduledServiceDate { get; set; } = DateTimeOffset.UtcNow.AddDays(7);
    public string Status { get; set; } = "Dispatched"; // Created | Dispatched | InProgress | Completed
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;
}
