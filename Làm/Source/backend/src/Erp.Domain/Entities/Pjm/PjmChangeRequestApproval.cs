using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pjm;

/// <summary>Phê duyệt yêu cầu thay đổi và điều chỉnh ngân sách/tiến độ dự án (UC_PJM_030).</summary>
public class PjmChangeRequestApproval : TenantEntity
{
    public Guid ChangeRequestId { get; set; }
    public string EcrNumber { get; set; } = "";
    public bool IsApproved { get; set; } = true;
    public decimal ApprovedCostAdjustmentVnd { get; set; } = 85000000;
    public int ApprovedScheduleAdjustmentDays { get; set; } = 5;
    public string ApproverName { get; set; } = "Giám Đốc Dự Án";
    public string ApprovalComments { get; set; } = "Đồng ý bổ sung phạm vi công việc và điều chỉnh phụ lục hợp đồng";
    public DateTimeOffset ApprovedAt { get; set; } = DateTimeOffset.UtcNow;
}
