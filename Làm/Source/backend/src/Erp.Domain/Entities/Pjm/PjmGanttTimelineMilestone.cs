using Erp.Domain.Base;

namespace Erp.Domain.Entities.Pjm;

/// <summary>Gantt milestone và các mốc tiến độ dự án (UC_PJM_016).</summary>
public class PjmGanttTimelineMilestone : TenantEntity
{
    public Guid ProjectId { get; set; }
    public string MilestoneCode { get; set; } = "MS-01";
    public string MilestoneName { get; set; } = "Hoàn tất lắp đặt hạ tầng mạng & cáp quang";
    public DateTimeOffset PlannedStartDate { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset PlannedEndDate { get; set; } = DateTimeOffset.UtcNow.AddDays(14);
    public double CompletionProgressPct { get; set; } = 75.0;
    public string PredecessorMilestoneCode { get; set; } = "";
    public string Status { get; set; } = "InProgress"; // Planned | InProgress | Completed | Delayed
}
