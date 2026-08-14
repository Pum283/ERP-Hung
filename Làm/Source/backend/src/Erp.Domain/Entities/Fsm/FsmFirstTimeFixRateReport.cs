using Erp.Domain.Base;

namespace Erp.Domain.Entities.Fsm;

/// <summary>Báo cáo tỷ lệ sửa chữa thành công lần đầu FTFR (UC_FSM_048).</summary>
public class FsmFirstTimeFixRateReport : TenantEntity
{
    public string PeriodLabel { get; set; } = "Tháng 08/2026";
    public int TotalResolvedTickets { get; set; } = 120;
    public int FirstTimeFixCount { get; set; } = 108;
    public int ReopenedOrRecallCount { get; set; } = 12;
    public double FirstTimeFixRatePct { get; set; } = 90.0;
    public DateTimeOffset ReportGeneratedAt { get; set; } = DateTimeOffset.UtcNow;
}
