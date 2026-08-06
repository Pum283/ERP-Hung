namespace Erp.Application.DTOs.Fsm;

public sealed record FsmSlaComplianceRowDto(
    string Priority, int OpenCount, int OverdueOpenCount,
    int ClosedCount, int SlaMetCount, int SlaMissCount, decimal SlaHitRatePercent);

public sealed record FsmTechProductivityRowDto(
    Guid? TechUserId, string TechName,
    int AssignedCount, int ResolvedCount, int ClosedCount,
    int OnSlaCount, decimal OnSlaPercent, decimal AvgResolveHours);

public sealed record FsmDashboardDto(
    int OpenCount, int AssignedCount, int InProgressCount, int EscalatedCount,
    int ResolvedCount, int ClosedCount, int OverdueOpenCount,
    int ClosedTodayCount, decimal SlaHitRatePercent, int AppointmentTodayCount);

public sealed record FsmPartCostRowDto(
    Guid PartId, string PartCode, string PartName,
    decimal Qty, decimal Amount, int TicketCount);

public sealed record FsmPartCostSummaryDto(
    decimal TotalQty, decimal TotalAmount, int LineCount, int TicketCount,
    IReadOnlyList<FsmPartCostRowDto> ByPart);
