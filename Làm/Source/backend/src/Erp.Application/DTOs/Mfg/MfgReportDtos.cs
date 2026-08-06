namespace Erp.Application.DTOs.Mfg;

public sealed record MfgWoProgressRowDto(
    Guid WorkOrderId, string Code, string ItemCode, string ItemName,
    string? WorkshopCode, string? WorkshopName, string Status,
    decimal QtyPlanned, decimal QtyFgReceived, decimal QtyScrap,
    decimal ProgressPercent, DateTimeOffset? ReleasedAt, DateTimeOffset? ClosedAt);

public sealed record MfgOutputRowDto(
    string Day, string ShiftLabel, Guid? WorkshopId, string? WorkshopCode, string? WorkshopName,
    decimal QtyFg, int ReceiptCount, int WorkOrderCount);

public sealed record MfgMaterialVarianceRowDto(
    Guid WorkOrderId, string WorkOrderCode, string Status,
    Guid ItemId, string ItemCode, string ItemName,
    decimal QtyPlanned, decimal QtyActual, decimal QtyVariance, decimal VariancePercent);

public sealed record MfgDashboardDto(
    int DraftCount, int ReleasedCount, int InProgressCount, int PausedCount,
    int CompletedCount, int ClosedCount,
    decimal QtyPlannedOpen, decimal QtyFgPeriod, decimal QtyScrapPeriod,
    int OpenWoCount, int VarianceOverCount);
