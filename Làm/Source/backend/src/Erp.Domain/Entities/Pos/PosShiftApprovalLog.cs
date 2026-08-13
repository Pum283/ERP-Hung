using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pos;

/// <summary>Nhật ký duyệt đóng ca bán hàng của Quản lý POS (UC_POS_049).</summary>
public class PosShiftApprovalLog : TenantEntity
{
    public Guid ShiftId { get; set; }
    public Guid ManagerUserId { get; set; }
    public string Status { get; set; } = "Approved"; // Approved | Rejected
    public decimal DiscrepancyVnd { get; set; }
    public string ManagerComments { get; set; } = "";
    public DateTimeOffset DecisionTime { get; set; } = DateTimeOffset.UtcNow;
}
