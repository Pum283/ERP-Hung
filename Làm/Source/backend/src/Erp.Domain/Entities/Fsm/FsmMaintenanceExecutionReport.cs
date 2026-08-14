using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Báo cáo thực hiện công tác bảo trì định kỳ (UC_FSM_036).</summary>
public class FsmMaintenanceExecutionReport : TenantEntity
{
    public string PeriodLabel { get; set; } = "Tháng 08/2026";
    public int TotalScheduledVisits { get; set; } = 48;
    public int CompletedVisitsCount { get; set; } = 46;
    public int DelayedVisitsCount { get; set; } = 2;
    public double OnTimeCompletionRatePct { get; set; } = 95.8;
    public decimal TotalMaintenanceRevenueVnd { get; set; } = 240000000;
    public DateTimeOffset ReportGeneratedAt { get; set; } = DateTimeOffset.UtcNow;
}
