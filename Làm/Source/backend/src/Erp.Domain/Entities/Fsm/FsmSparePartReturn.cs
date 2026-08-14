using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Phiếu hoàn trả linh kiện thừa về kho (UC_FSM_025).</summary>
public class FsmSparePartReturn : TenantEntity
{
    public string ReturnSlipNumber { get; set; } = "";
    public Guid TicketId { get; set; }
    public string TicketNumber { get; set; } = "";
    public string PartCode { get; set; } = "";
    public string PartName { get; set; } = "";
    public decimal ReturnedQuantity { get; set; }
    public string Reason { get; set; } = "Thừa sau khi thay thế linh kiện chính";
    public string DestinationWarehouseCode { get; set; } = "KHO-LINH-KIEN-FSM";
    public string Status { get; set; } = "Received"; // Pending | Received | Cancelled
    public DateTimeOffset ReturnedAt { get; set; } = DateTimeOffset.UtcNow;
}
