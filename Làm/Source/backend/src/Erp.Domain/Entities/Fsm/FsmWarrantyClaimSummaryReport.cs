using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Báo cáo tổng hợp yêu cầu và chi phí bảo hành thiết bị (UC_FSM_049).</summary>
public class FsmWarrantyClaimSummaryReport : TenantEntity
{
    public string PeriodLabel { get; set; } = "Tháng 08/2026";
    public int TotalClaimsCount { get; set; } = 35;
    public int ApprovedClaimsCount { get; set; } = 32;
    public int RejectedClaimsCount { get; set; } = 3;
    public decimal TotalClaimCoveredAmountVnd { get; set; } = 155000000;
    public double ClaimApprovalRatePct { get; set; } = 91.4;
    public DateTimeOffset ReportGeneratedAt { get; set; } = DateTimeOffset.UtcNow;
}
