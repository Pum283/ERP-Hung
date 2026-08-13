using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Lệnh chế biến gửi bếp/bar POS (UC_POS_031 - KOT Ticket).</summary>
public class PosKitchenOrderTicket : TenantEntity
{
    public Guid OrderId { get; set; }
    public string TicketNumber { get; set; } = "";
    public string StationCode { get; set; } = "KITCHEN"; // KITCHEN | BAR
    public string ItemsJson { get; set; } = "[]";
    public string Status { get; set; } = "Sent"; // Sent | Preparing | Ready | Served
    public DateTimeOffset SentAt { get; set; } = DateTimeOffset.UtcNow;
}
