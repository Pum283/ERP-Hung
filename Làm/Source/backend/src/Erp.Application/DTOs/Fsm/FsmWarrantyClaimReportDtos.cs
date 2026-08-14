namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_FSM_049: Báo cáo bảo hành
// ────────────────────────────────────────────────────────────────────────────

public record FsmWarrantyClaimSummaryReportDto(
    Guid Id,
    string PeriodLabel,
    int TotalClaimsCount,
    int ApprovedClaimsCount,
    int RejectedClaimsCount,
    decimal TotalClaimCoveredAmountVnd,
    double ClaimApprovalRatePct,
    DateTimeOffset ReportGeneratedAt
);
