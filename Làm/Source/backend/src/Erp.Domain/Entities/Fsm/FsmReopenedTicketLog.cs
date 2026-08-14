using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Nhật ký tái mở ticket sau nghiệm thu (UC_FSM_031).</summary>
public class FsmReopenedTicketLog : TenantEntity
{
    public Guid TicketId { get; set; }
    public string TicketNumber { get; set; } = "";
    public string ReopenReason { get; set; } = "Lỗi tái diễn sau khi chạy thử 2 giờ";
    public string ReopenedBy { get; set; } = "Khách hàng gọi Hotline";
    public string RootCauseClassification { get; set; } = "Linh kiện thay thế không tương thích";
    public DateTimeOffset ReopenedAt { get; set; } = DateTimeOffset.UtcNow;
}
