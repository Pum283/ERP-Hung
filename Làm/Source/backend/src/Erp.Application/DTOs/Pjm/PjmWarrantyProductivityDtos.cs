namespace Erp.Application.DTOs;

// ────────────────────────────────────────────────────────────────────────────
// UC_PJM_037: Bảo hành sau dự án
// ────────────────────────────────────────────────────────────────────────────

public record PjmCreateWarrantyCoverageRequest(
    Guid ProjectId,
    string ProjectCode,
    string CustomerName,
    DateTimeOffset WarrantyStartDate,
    DateTimeOffset WarrantyEndDate,
    int WarrantyPeriodMonths,
    string SupportHotline
);

public record PjmPostProjectWarrantyCoverageDto(
    Guid Id,
    Guid ProjectId,
    string ProjectCode,
    string CustomerName,
    DateTimeOffset WarrantyStartDate,
    DateTimeOffset WarrantyEndDate,
    int WarrantyPeriodMonths,
    string SupportHotline,
    bool IsActive
);

// ────────────────────────────────────────────────────────────────────────────
// UC_PJM_041: Năng suất nguồn lực
// ────────────────────────────────────────────────────────────────────────────

public record PjmResourceProductivityReportDto(
    Guid Id,
    string PeriodLabel,
    int TotalEngineersCount,
    decimal TotalAllocatedHours,
    decimal TotalBillableTimesheetHours,
    double ResourceUtilizationRatePct,
    decimal AverageOutputPerEngineerVnd,
    DateTimeOffset ReportGeneratedAt
);
