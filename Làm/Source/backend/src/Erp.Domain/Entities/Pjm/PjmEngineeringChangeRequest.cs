using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pjm;

/// <summary>Phát sinh yêu cầu thay đổi thiết kế / kỹ thuật / phạm vi dự án ECR (UC_PJM_029).</summary>
public class PjmEngineeringChangeRequest : TenantEntity
{
    public string EcrNumber { get; set; } = "";
    public Guid ProjectId { get; set; }
    public string ProjectCode { get; set; } = "PRJ-2026-088";
    public string EcrTitle { get; set; } = "Bổ sung tủ tụ bù hạ thế 250kVAR";
    public string ChangeReason { get; set; } = "Khách hàng mở rộng xưởng sản xuất và nâng hệ số cos phi";
    public decimal EstimatedCostImpactVnd { get; set; } = 85000000;
    public int ScheduleImpactDays { get; set; } = 5;
    public string Status { get; set; } = "Submitted"; // Submitted | Approved | Rejected
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
