using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Đánh giá sự hài lòng của khách hàng CSAT (UC_FSM_029).</summary>
public class FsmCustomerServiceFeedback : TenantEntity
{
    public Guid TicketId { get; set; }
    public string TicketNumber { get; set; } = "";
    public int StarRating { get; set; } = 5; // 1 to 5 Stars
    public string FeedbackComment { get; set; } = "Kỹ thuật viên nhiệt tình, xử lý nhanh chóng";
    public string CustomerSignerName { get; set; } = "Nguyễn Văn Hùng";
    public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.UtcNow;
}
