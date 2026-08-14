using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pjm;

/// <summary>Nhật ký thay đổi kế hoạch / đường cơ sở baseline dự án (UC_PJM_018).</summary>
public class PjmPlanChangeAuditLog : TenantEntity
{
    public Guid ProjectId { get; set; }
    public string ProjectCode { get; set; } = "PRJ-2026-088";
    public string ChangeTitle { get; set; } = "Gia hạn thêm 7 ngày do nhà máy cắt điện nguồn";
    public string ReasonForChange { get; set; } = "Khách hàng yêu cầu dừng thi công để nghiệm thu PCCC nội bộ";
    public string RequestedBy { get; set; } = "PM Nguyễn Văn Tuấn";
    public string ApprovalStatus { get; set; } = "Approved"; // Pending | Approved | Rejected
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
}
