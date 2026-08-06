namespace Erp.Application.DTOs.Log;

public sealed record LogCodMarkRequest(decimal Amount, int? DueDays, string? Note);
public sealed record LogCodAmountRequest(decimal Amount, string? Note);
public sealed record LogCodCollectRequest(string? Note);

public sealed record LogCodHandoverCreateRequest(
    IReadOnlyList<Guid> DeliveryOrderIds,
    Guid? DriverUserId,
    string? DriverName,
    string? Note);

public sealed record LogCodReconcileRequest(decimal RemittedAmount, string? Note);
public sealed record LogCodResolveVarianceRequest(decimal? RemittedAmount, string Note);

public sealed record LogCodHandoverDto(
    Guid Id, string Code, string Status,
    Guid? DriverUserId, string? DriverName,
    decimal ExpectedAmount, decimal CollectedAmount, decimal RemittedAmount, decimal VarianceAmount,
    string? Note, string? VarianceNote,
    DateTimeOffset? SubmittedAt, DateTimeOffset? ReconciledAt,
    int LineCount, DateTimeOffset CreatedAt);

public sealed record LogCodHandoverLineDto(
    Guid Id, Guid HandoverId, Guid DeliveryOrderId,
    string DeliveryCode, string CustomerName, decimal CodAmount, string? Note);

public sealed record LogCodHandoverDetailDto(
    LogCodHandoverDto Header,
    IReadOnlyList<LogCodHandoverLineDto> Lines);

public sealed record LogCodReportDto(
    decimal PendingAmount, int PendingCount,
    decimal CollectedAmount, int CollectedCount,
    decimal RemittedAmount, int RemittedCount,
    decimal ReconciledAmount, int ReconciledCount,
    decimal OverdueAmount, int OverdueCount,
    decimal VarianceAmount, int VarianceCount);
